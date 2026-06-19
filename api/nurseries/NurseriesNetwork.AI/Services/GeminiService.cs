using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace NurseriesNetwork.AI.Services;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private const string BaseUrl =
        "https://generativelanguage.googleapis.com/v1beta/models";

    public GeminiService(
        HttpClient httpClient,
        IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    // ===========================
    // توليد رد نصي من Gemini (LLM)
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
    // توليد Embedding لنص معين (RAG)
    // ===========================
    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:EmbeddingModel"];

        var url = $"{BaseUrl}/{model}:embedContent?key={apiKey}";

        var requestBody = new
        {
            model = $"models/{model}",   // ← أضف السطر ده
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
}