using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces.Services;

namespace NurseriesNetwork.AI.Services;

public class RecommendationService : IAiService
{
    private readonly RagService _rag;
    private readonly GeminiService _gemini;

    public RecommendationService(RagService rag, GeminiService gemini)
    {
        _rag = rag;
        _gemini = gemini;
    }

    public async Task<string> GetRecommendationAsync(
        string message, double? lat, double? lng)
    {
        var relevantNurseries = await _rag.SemanticSearchAsync(message);

        if (!relevantNurseries.Any())
        {
            var noContextPrompt = """
                أنت مساعد ذكي لتطبيق حضانات في مصر.
                مفيش حضانات متاحة دلوقتي تطابق طلب المستخدم.
                رد بأدب واطلب منه يجرب كلام تاني أو يوسع منطقة البحث.
                رد بالعربي فقط.
                """;
            return await _gemini.GetChatResponseAsync(noContextPrompt, message);
        }

        var context = string.Join("\n", relevantNurseries.Select((n, i) =>
            $"{i + 1}. {n.Name} - {n.Location?.City} - " +
            $"السعر: {n.DailyPrice} جنيه/يوم - التقييم: {n.AvgRating}/5"));

        var systemPrompt = $"""
            أنت مساعد ذكي متخصص في مساعدة الآباء لإيجاد أفضل حضانة لأطفالهم في مصر.
            بناءً على الحضانات المتاحة التالية، قدم توصية شخصية ومفيدة:

            {context}

            القواعد:
            - رد بالعربي فقط
            - اذكر الأسعار والتقييمات
            - كن ودوداً ومختصراً
            """;

        return await _gemini.GetChatResponseAsync(systemPrompt, message);
    }

    public async Task GenerateAndSaveEmbeddingAsync(Nursery nursery)
        => await _rag.GenerateAndSaveEmbeddingAsync(nursery);
}