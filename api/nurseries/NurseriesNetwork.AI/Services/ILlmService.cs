using NurseriesNetwork.Core.DTOs.AI;

namespace NurseriesNetwork.AI.Services;

public interface ILlmService
{
    Task<string> GetChatResponseAsync(
        string systemPrompt,
        string userMessage,
        List<ConversationMessage>? history = null);

    Task<IntentClassificationResult> ClassifyIntentAsync(
    string userMessage,
    List<ConversationMessage>? history = null);  // ✅ ضيف history

    Task<SearchFilters> ExtractSearchFiltersAsync(string userMessage);

    Task<GeminiFunctionCallResult> GetFunctionCallAsync(
        string userMessage,
        List<ConversationMessage>? history = null);

    Task<string> GetFinalResponseAfterFunctionAsync(
        string userMessage,
        string functionName,
        string functionResult);

    Task<AdminFunctionCallResult> GetAdminFunctionCallAsync(
        string userMessage,
        List<ConversationMessage>? history = null);
}