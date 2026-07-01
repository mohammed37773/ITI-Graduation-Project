using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NurseriesNetwork.Core.DTOs.AI;

namespace NurseriesNetwork.AI.Services;

public class GeminiService : ILlmService
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
    // Chat عادي
    // ===========================
    public async Task<string> GetChatResponseAsync(
        string systemPrompt, string userMessage)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:ChatModel"];

        var url = $"{BaseUrl}/{model}:generateContent?key={apiKey}";

        // ✅ فصل صريح بين system_instruction والـ user message
        //    (قبل التعديل كان كل حاجة بتتبعت كرسالة user واحدة، وده كان بيقلل التزام الموديل بالـ context)
        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = userMessage }
                    }
                }
            }
        };

        var response = await SendWithRetryAsync(url, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            _logger.LogError(
                "Gemini Chat failed. StatusCode: {StatusCode}. Response: {Error}",
                response.StatusCode,
                error);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                return "وصلنا للحد المسموح من الطلبات حاليًا، جرب تاني بعد دقايق.";

            return "معذرة، خدمة الذكاء الاصطناعي غير متاحة حالياً، حاول مرة أخرى بعد قليل.";
        }

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
    // Embedding — معتمد على Gemini فقط (لا يوجد Fallback، Groq/Grok لا يدعمان Embeddings)
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

        var response = await SendWithRetryAsync(url, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            _logger.LogError(
                "Gemini Embedding failed. StatusCode: {StatusCode}. Response: {Error}",
                response.StatusCode,
                error);

            return Array.Empty<float>();
        }

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
    // ✅ Intent Classification (قبل RAG)
    // ===========================
    public async Task<IntentClassificationResult> ClassifyIntentAsync(string userMessage)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:LiteModel"];

        var url = $"{BaseUrl}/{model}:generateContent?key={apiKey}";

        var systemInstruction = """
            أنت Classifier مهمتك تحديد نوع رسالة المستخدم في تطبيق حضانات.
            رد فقط بـ JSON بدون أي شرح أو Markdown، بالشكل ده بالضبط:

            {
              "intent": "GREETING" | "THANKS" | "GENERAL" | "SEARCH" | "BOOKING" | "MEDICAL_CONCERN",
              "reply": "نص الرد المباشر لو الـ intent هو GREETING أو THANKS أو GENERAL أو MEDICAL_CONCERN، غير ذلك سيبه فاضي"
            }

            القواعد:
            - GREETING: لو الرسالة تحية بس (السلام عليكم، أهلاً، ازيك، صباح الخير...)
            - THANKS: لو الرسالة شكر (شكراً، تسلم، ربنا يخليك...)
            - GENERAL: لو سؤال عام عن التطبيق نفسه أو خارج نطاق الحضانات (مين أنت؟ التطبيق ده بيعمل إيه؟)
            - MEDICAL_CONCERN: لو الرسالة بتعبر عن قلق صحي أو حالة طبية للطفل (وزن ناقص، مرض، تأخر نمو، إلخ) من غير طلب بحث صريح عن حضانة. هنا لا تبحث عن حضانات، فقط اطلب توضيح هل المستخدم يريد حضانة متخصصة في الرعاية الطبية أم يحتاج نصيحة طبية (ولو الأخيرة، وضح إنك مساعد حضانات وليس طبيب).
            - SEARCH: لو المستخدم بيدور على حضانة أو بيسأل عن حضانات بمعايير معينة (مدينة، سعر، تقييم، سن معين...)
            - BOOKING: لو المستخدم بوضوح عايز يحجز حضانة (احجزلي، عايز أحجز، سجل طفلي...)

            لو الـ intent هو GREETING أو THANKS أو GENERAL أو MEDICAL_CONCERN، اكتب رد مناسب وودود بالعربي في حقل reply.
            لو الـ intent هو SEARCH أو BOOKING، سيب حقل reply فاضي "" لأن النظام هيكمل بنفسه.

            لا تكتب أي حاجة غير الـ JSON. لا تستخدم ```json أو أي تنسيق Markdown.
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
            generationConfig = new
            {
                response_mime_type = "application/json"
            }
        };

        var response = await SendWithRetryAsync(url, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Gemini Intent Classification failed. StatusCode: {StatusCode}. Response: {Error}",
                response.StatusCode, error);

            // فشل التصنيف؟ خليه يعتبرها SEARCH افتراضيًا (Fail-safe) عشان مايضيعش طلب حقيقي
            return new IntentClassificationResult("SEARCH", "");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
            return new IntentClassificationResult("SEARCH", "");

        try
        {
            using var resultDoc = JsonDocument.Parse(text);
            var intent = resultDoc.RootElement.GetProperty("intent").GetString() ?? "SEARCH";
            var reply = resultDoc.RootElement.TryGetProperty("reply", out var replyEl)
                ? replyEl.GetString() ?? ""
                : "";

            return new IntentClassificationResult(intent, reply);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex,
                "Failed to parse intent classification response: {Text}", text);
            return new IntentClassificationResult("SEARCH", "");
        }
    }

    // ===========================
    // ✅ استخراج فلاتر منطقية (سعر/مدينة/عمر) من رسالة المستخدم
    // ===========================
    public async Task<SearchFilters> ExtractSearchFiltersAsync(string userMessage)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:LiteModel"];

        var url = $"{BaseUrl}/{model}:generateContent?key={apiKey}";

        var systemInstruction = """
            أنت تستخرج فلاتر بحث من رسالة مستخدم يدور على حضانة.
            رد فقط بـ JSON بدون أي شرح، بالشكل ده بالضبط:

            {
              "city": "اسم المدينة لو مذكورة أو null",
              "maxPrice": رقم السعر الأعلى المطلوب لو مذكور أو null,
              "minRating": رقم التقييم الأدنى المطلوب لو مذكور أو null,
              "childAgeMonths": عمر الطفل بالشهور لو مذكور أو null
            }

            لو القيمة غير مذكورة في الرسالة، استخدم null. لا تخترع قيم.
            لا تكتب أي حاجة غير الـ JSON.
            """;

        var requestBody = new
        {
            system_instruction = new { parts = new[] { new { text = systemInstruction } } },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = userMessage } } }
            },
            generationConfig = new { response_mime_type = "application/json" }
        };

        var response = await SendWithRetryAsync(url, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini ExtractSearchFilters failed, returning empty filters.");
            return new SearchFilters(null, null, null, null);
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
            return new SearchFilters(null, null, null, null);

        try
        {
            using var resultDoc = JsonDocument.Parse(text);
            var root = resultDoc.RootElement;

            string? city = root.TryGetProperty("city", out var c) && c.ValueKind != JsonValueKind.Null
                ? c.GetString() : null;

            decimal? maxPrice = root.TryGetProperty("maxPrice", out var p) && p.ValueKind == JsonValueKind.Number
                ? p.GetDecimal() : null;

            double? minRating = root.TryGetProperty("minRating", out var r) && r.ValueKind == JsonValueKind.Number
                ? r.GetDouble() : null;

            int? childAgeMonths = root.TryGetProperty("childAgeMonths", out var a) && a.ValueKind == JsonValueKind.Number
                ? a.GetInt32() : null;

            return new SearchFilters(city, maxPrice, minRating, childAgeMonths);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse search filters response: {Text}", text);
            return new SearchFilters(null, null, null, null);
        }
    }

    // ===========================
    // ✅ Function Calling
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

            قواعد صارمة لازم تتبعها:
            - إذا كانت رسالة المستخدم مجرد تحية (السلام عليكم، أهلاً، ازيك...)، لا تستخدم أي Function، رد بتحية ودودة وسؤال عن طلبه.
            - إذا كانت الرسالة شكر (شكراً، تسلم...)، لا تستخدم أي Function، رد بالعفو وتحت أمره.
            - إذا لم تكن الرسالة عن الحضانات أو الحجز بشكل واضح، لا تستخدم أي Function، رد برد عام مناسب.
            - إذا كان الطلب واضح ومحدد (يدور على حضانة أو يريد الحجز)، استخدم الـ Function المناسبة مباشرة.
            - إذا كان الطلب غامض أو ناقص معلومة أساسية للحجز (مثل اسم الطفل أو التاريخ)، اطلب التوضيح بدل استدعاء Function.
            - لا تخترع أسماء حضانات أو معلومات غير موجودة في نتائج الـ Function.
            - لا تؤكد حجز لم يتم تنفيذه فعليًا عبر create_booking.
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


        var response = await SendWithRetryAsync(url, requestBody);

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

        var response = await SendWithRetryAsync(url, requestBody);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            _logger.LogError(
                "Gemini Final Response failed. StatusCode: {StatusCode}. Response: {Error}",
                response.StatusCode,
                error);

            return functionResult;
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? functionResult;
    }

    // ===========================
    // ✅ إرسال الطلب مع إعادة المحاولة. عند استمرار 429 بعد كل المحاولات،
    //    يرمي GeminiRateLimitException عشان LlmFallbackService يحوّل الطلب لـ Groq
    // ===========================
    private async Task<HttpResponseMessage> SendWithRetryAsync(
        string url,
        object body,
        int maxRetries = 2)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            var response = await _httpClient.PostAsJsonAsync(url, body);

            if (response.IsSuccessStatusCode)
                return response;

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                if (attempt >= maxRetries)
                {
                    _logger.LogWarning(
                        "Gemini quota exceeded after {MaxRetries} attempts. Triggering fallback.",
                        maxRetries);
                    throw new GeminiRateLimitException("Gemini quota exceeded (429).");
                }

                var delaySeconds = response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 2;

                _logger.LogWarning(
                    "Gemini rate limited (429). Retry {Attempt}/{MaxRetries} after {Delay}s",
                    attempt, maxRetries, delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                continue;
            }

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable &&
                attempt < maxRetries)
            {
                _logger.LogWarning(
                    "Gemini unavailable. Retry {Attempt}/{MaxRetries}",
                    attempt, maxRetries);

                await Task.Delay(TimeSpan.FromSeconds(attempt * 3));
                continue;
            }

            return response;
        }

        throw new Exception("Failed after retries.");
    }




    // ===========================
    // ✅ جديد — Function Calling خاص بـ NurseryAdmin
    // ضيف الميثود دي جوه كلاس GeminiService الموجود عندك، في أي مكان بعد GetFunctionCallAsync
    // ===========================
    public async Task<AdminFunctionCallResult> GetAdminFunctionCallAsync(string userMessage)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:ChatModel"];

        var url = $"{BaseUrl}/{model}:generateContent?key={apiKey}";

        var tools = new[]
        {
        new
        {
            function_declarations = new object[]
            {
                new
                {
                    name = "get_nursery_performance",
                    description = "تحليل أداء حضانة المستخدم (عدد الحجوزات، المدفوعات، الإيرادات، التقييم) للشهر الحالي",
                    parameters = new
                    {
                        type = "object",
                        properties = new { },
                        required = Array.Empty<string>()
                    }
                },
                new
                {
                    name = "search_my_bookings",
                    description = "البحث الذكي في حجوزات حضانة المستخدم بفلاتر متعددة (حالة الحجز، حالة الدفع، عمر الطفل، فترة زمنية)",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            booking_status = new
                            {
                                type = "string",
                                description = "حالة الحجز: Pending, Confirmed, Cancelled, Completed"
                            },
                            payment_status = new
                            {
                                type = "string",
                                description = "حالة الدفع: Pending, Paid, Failed, Refunded"
                            },
                            max_child_age_months = new
                            {
                                type = "integer",
                                description = "أقصى عمر للطفل بالشهور (مثلاً 12 لو طلب 'أقل من سنة')"
                            },
                            min_child_age_months = new
                            {
                                type = "integer",
                                description = "أقل عمر للطفل بالشهور"
                            },
                            within_last_days = new
                            {
                                type = "integer",
                                description = "عدد الأيام الماضية للفلترة الزمنية (مثلاً 7 لو طلب 'الأسبوع ده')"
                            }
                        },
                        required = Array.Empty<string>()
                    }
                }
            }
        }
    };

        var systemInstruction = """
        أنت مساعد ذكي يساعد مدير حضانة (NurseryAdmin) على إدارة حضانته بكفاءة.

        مهمتك:
        1. لو سأل عن أداء حضانته (إيرادات، حجوزات، تقييم) استخدم get_nursery_performance.
        2. لو طلب البحث أو الفلترة في حجوزاته (حسب الحالة، الدفع، عمر الطفل، فترة زمنية) استخدم search_my_bookings.

        قواعد:
        - إذا كانت الرسالة تحية أو شكر أو غير متعلقة بإدارة الحضانة، لا تستخدم أي Function، رد مباشرة بأدب.
        - حول الفترات الزمنية الكلامية لعدد أيام تقريبي (مثلاً "الأسبوع ده" = 7, "الشهر ده" = 30).
        - حول "أقل من سنة" إلى max_child_age_months = 12، و"أكبر من سنتين" إلى min_child_age_months = 24، وهكذا.
        - لا تخترع بيانات أو نتائج، اعتمد فقط على نتيجة تنفيذ الـ Function.
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

        var response = await SendWithRetryAsync(url, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Gemini Admin Function Call failed. StatusCode: {StatusCode}. Response: {Content}",
                response.StatusCode, errorContent);
            return new AdminFunctionCallResult(false, null, null, "حصل خطأ في فهم الطلب");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = System.Text.Json.JsonDocument.Parse(json);

        var content = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content");

        var parts = content.GetProperty("parts");

        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("functionCall", out var functionCall))
            {
                var functionName = functionCall.GetProperty("name").GetString()!;
                var argsJson = functionCall.GetProperty("args").GetRawText();

                _logger.LogInformation(
                    "Gemini decided to call admin function: {FunctionName} with args: {Args}",
                    functionName, argsJson);

                return new AdminFunctionCallResult(true, functionName, argsJson, null);
            }

            if (part.TryGetProperty("text", out var textElement))
            {
                return new AdminFunctionCallResult(false, null, null, textElement.GetString());
            }
        }

        return new AdminFunctionCallResult(
            false, null, null, "معذرة، مش فاهم طلبك، ممكن توضحه أكتر؟");
    }


}