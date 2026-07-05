using System.Net;
using Microsoft.Extensions.Logging;
using NurseriesNetwork.Core.DTOs.AI;

namespace NurseriesNetwork.AI.Services;

public class LlmFallbackService : ILlmService
{
    private readonly GeminiService _gemini;
    private readonly GroqService _grok;
    private readonly ILogger<LlmFallbackService> _logger;

    public LlmFallbackService(
        GeminiService gemini,
        GroqService grok,
        ILogger<LlmFallbackService> logger)
    {
        _gemini = gemini;
        _grok = grok;
        _logger = logger;
    }

    public async Task<string> GetChatResponseAsync(
    string systemPrompt, string userMessage,
    List<ConversationMessage>? history = null)
    {
        try
        {
            return await _gemini.GetChatResponseAsync(systemPrompt, userMessage, history);
        }
        catch (GeminiRateLimitException)
        {
            _logger.LogWarning("Gemini rate-limited. Falling back to Groq for GetChatResponseAsync.");
            return await _grok.GetChatResponseAsync(systemPrompt, userMessage, history);
        }
    }

    public async Task<GeminiFunctionCallResult> GetFunctionCallAsync(
        string userMessage,
        List<ConversationMessage>? history = null)
    {
        try
        {
            return await _gemini.GetFunctionCallAsync(userMessage, history);
        }
        catch (GeminiRateLimitException)
        {
            _logger.LogWarning("Gemini rate-limited. Falling back to Groq for GetFunctionCallAsync.");
            return await _grok.GetFunctionCallAsync(userMessage, history);
        }
    }

    public async Task<AdminFunctionCallResult> GetAdminFunctionCallAsync(
        string userMessage,
        List<ConversationMessage>? history = null)
    {
        try
        {
            return await _gemini.GetAdminFunctionCallAsync(userMessage, history);
        }
        catch (GeminiRateLimitException)
        {
            _logger.LogWarning("Gemini rate-limited. Falling back to Groq for GetAdminFunctionCallAsync.");
            return await _grok.GetAdminFunctionCallAsync(userMessage, history);
        }
    }

    public async Task<IntentClassificationResult> ClassifyIntentAsync(
    string userMessage,
    List<ConversationMessage>? history = null)
    {
        try
        {
            return await _gemini.ClassifyIntentAsync(userMessage, history);
        }
        catch (GeminiRateLimitException)
        {
            _logger.LogWarning("Gemini rate-limited. Falling back to Groq for ClassifyIntentAsync.");
            return await _grok.ClassifyIntentAsync(userMessage, history);
        }
    }

    public async Task<SearchFilters> ExtractSearchFiltersAsync(string userMessage)
    {
        try
        {
            return await _gemini.ExtractSearchFiltersAsync(userMessage);
        }
        catch (GeminiRateLimitException)
        {
            _logger.LogWarning("Gemini rate-limited. Falling back to Grok for ExtractSearchFiltersAsync.");
            return await _grok.ExtractSearchFiltersAsync(userMessage);
        }
    }

    public async Task<string> GetFinalResponseAfterFunctionAsync(
        string userMessage, string functionName, string functionResult)
    {
        try
        {
            return await _gemini.GetFinalResponseAfterFunctionAsync(userMessage, functionName, functionResult);
        }
        catch (GeminiRateLimitException)
        {
            _logger.LogWarning("Gemini rate-limited. Falling back to Grok for GetFinalResponseAfterFunctionAsync.");
            return await _grok.GetFinalResponseAfterFunctionAsync(userMessage, functionName, functionResult);
        }
    }



    // ===========================
    // ✅ جديد — ضيف الميثود دي جوه كلاس LlmFallbackService الموجود عندك
    // ===========================
    public async Task<AdminFunctionCallResult> GetAdminFunctionCallAsync(string userMessage)
    {
        try
        {
            return await _gemini.GetAdminFunctionCallAsync(userMessage);
        }
        catch (GeminiRateLimitException)
        {
            _logger.LogWarning("Gemini rate-limited. Falling back to Groq for GetAdminFunctionCallAsync.");
            return await _grok.GetAdminFunctionCallAsync(userMessage);
        }
    }
}

// ===========================
// Exception مخصصة عشان نفرق 429 عن أي error تاني
// ===========================
public class GeminiRateLimitException : Exception
{
    public GeminiRateLimitException(string message) : base(message) { }
}