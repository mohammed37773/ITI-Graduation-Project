using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NurseriesNetwork.AI.Services;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<GeminiService> _logger;
    private const string BaseUrl =
        "https://generativelanguage.googleapis.com/v1beta/models";

    public GeminiService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    // ===========================
    // Chat عادي (موجود بالفعل)
    // ===========================
    public async Task<string> GetChatResponseAsync(
        string systemPrompt, string userMessage)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:ChatModel"];

        var url = $"{BaseUrl}/{model}:generateContent?key={apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = $"{systemPrompt}\n\nسؤال المستخدم: {userMessage}" }
                    }
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? "معذرة، حصلت مشكلة في الرد";
    }

    // ===========================
    // Embedding (موجود بالفعل)
    // ===========================
    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:EmbeddingModel"];

        var url = $"{BaseUrl}/{model}:embedContent?key={apiKey}";

        var requestBody = new
        {
            model = $"models/{model}",
            content = new
            {
                parts = new[] { new { text } }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var values = doc.RootElement
            .GetProperty("embedding")
            .GetProperty("values")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();

        return values;
    }

    // ===========================
    // ✅ جديد — Function Calling
    // ===========================
    public async Task<GeminiFunctionCallResult> GetFunctionCallAsync(
        string userMessage,
        string conversationContext = "")
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:ChatModel"];

        var url = $"{BaseUrl}/{model}:generateContent?key={apiKey}";

        // تعريف الـ Functions المتاحة للموديل
        var tools = new[]
        {
            new
            {
                function_declarations = new object[]
                {
                    new
                    {
                        name = "find_nearby_nurseries",
                        description = "البحث عن حضانات قريبة من موقع معين بسعر معين",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                city = new
                                {
                                    type = "string",
                                    description = "اسم المدينة المطلوب البحث فيها (مثل القاهرة)"
                                },
                                max_price = new
                                {
                                    type = "number",
                                    description = "أعلى سعر يومي مقبول بالجنيه المصري"
                                }
                            },
                            required = Array.Empty<string>()
                        }
                    },
                    new
                    {
                        name = "create_booking",
                        description = "إنشاء حجز جديد لطفل في حضانة معينة",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                nursery_name = new
                                {
                                    type = "string",
                                    description = "اسم الحضانة المطلوب الحجز فيها"
                                },
                                child_name = new
                                {
                                    type = "string",
                                    description = "اسم الطفل المطلوب حجزه"
                                },
                                start_date = new
                                {
                                    type = "string",
                                    description = "تاريخ بدء الحجز بصيغة YYYY-MM-DD"
                                }
                            },
                            required = new[] { "nursery_name", "child_name", "start_date" }
                        }
                    }
                }
            }
        };

        var systemInstruction = """
            أنت مساعد ذكي لتطبيق حضانات في مصر اسمه Nurseries Network.
            مهمتك مساعدة الآباء في:
            1. البحث عن حضانات مناسبة (استخدم find_nearby_nurseries)
            2. حجز حضانة لطفلهم (استخدم create_booking)
            
            لو الطلب واضح ومحدد، استخدم الـ Function المناسبة مباشرة.
            لو الطلب غامض أو ناقص معلومة أساسية، اطلب التوضيح بدل استدعاء Function.
            """;

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemInstruction } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = userMessage } }
                }
            },
            tools
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Gemini Function Call failed. StatusCode: {StatusCode}. Response: {Content}",
                response.StatusCode, errorContent);
            return new GeminiFunctionCallResult(false, null, null, "حصل خطأ في فهم الطلب");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var content = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content");

        var parts = content.GetProperty("parts");

        foreach (var part in parts.EnumerateArray())
        {
            // لو الموديل قرر يستدعي Function
            if (part.TryGetProperty("functionCall", out var functionCall))
            {
                var functionName = functionCall.GetProperty("name").GetString()!;
                var argsJson = functionCall.GetProperty("args").GetRawText();

                _logger.LogInformation(
                    "Gemini decided to call function: {FunctionName} with args: {Args}",
                    functionName, argsJson);

                return new GeminiFunctionCallResult(
                    true, functionName, argsJson, null);
            }

            // لو الموديل رد بنص عادي (مثلاً طلب توضيح)
            if (part.TryGetProperty("text", out var textElement))
            {
                return new GeminiFunctionCallResult(
                    false, null, null, textElement.GetString());
            }
        }

        return new GeminiFunctionCallResult(
            false, null, null, "معذرة، مش فاهم طلبك، ممكن توضحه أكتر؟");
    }

    // ===========================
    // ✅ بعد تنفيذ الـ Function، نرجع النتيجة للموديل يصيغها كرد طبيعي
    // ===========================
    public async Task<string> GetFinalResponseAfterFunctionAsync(
        string userMessage, string functionName, string functionResult)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:ChatModel"];

        var url = $"{BaseUrl}/{model}:generateContent?key={apiKey}";

        var prompt = $"""
            سؤال المستخدم: {userMessage}
            
            تم تنفيذ العملية: {functionName}
            النتيجة: {functionResult}
            
            اكتب رداً طبيعياً وودوداً بالعربي للمستخدم يلخص النتيجة.
            """;

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? functionResult;
    }
}

// ===========================
// Result Model
// ===========================
public record GeminiFunctionCallResult(
    bool ShouldCallFunction,
    string? FunctionName,
    string? ArgumentsJson,
    string? DirectTextResponse
);