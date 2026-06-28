using NurseriesNetwork.Core.DTOs.AI;

namespace NurseriesNetwork.AI.Services;

public interface ILlmService
{

    Task<string> GetChatResponseAsync(string systemPrompt, string userMessage);
    Task<IntentClassificationResult> ClassifyIntentAsync(string userMessage);
    Task<SearchFilters> ExtractSearchFiltersAsync(string userMessage);
    Task<GeminiFunctionCallResult> GetFunctionCallAsync(string userMessage, string conversationContext = "");
    Task<string> GetFinalResponseAfterFunctionAsync(string userMessage, string functionName, string functionResult);

    // ✅ جديد — Function Calling خاص بـ NurseryAdmin (تحليل الأداء + البحث الذكي في الحجوزات)
    Task<AdminFunctionCallResult> GetAdminFunctionCallAsync(string userMessage);
}

