using NurseriesNetwork.Core.DTOs.AI;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces.Services;

namespace NurseriesNetwork.AI.Services;

public class RecommendationService : IAiService
{
    private readonly RagService _rag;
    private readonly ILlmService _llm; // ✅ يدعم Fallback (Gemini -> Groq)

    public RecommendationService(RagService rag, ILlmService llm)
    {
        _rag = rag;
        _llm = llm;
    }

    public async Task<RecommendationResult> GetRecommendationAsync(
    string message, double? lat, double? lng,
    IntentClassificationResult? precomputedIntent = null,
    List<ConversationMessage>? history = null)  // ✅ جديد
    {
        var intent = precomputedIntent ?? await _llm.ClassifyIntentAsync(message);

        if (intent.Intent is "GREETING" or "THANKS" or "GENERAL" or "MEDICAL_CONCERN")
        {
            var directReply = string.IsNullOrWhiteSpace(intent.DirectReply)
                ? "أهلاً بك! أقدر أساعدك في إيه؟"
                : intent.DirectReply;
            return new RecommendationResult(directReply, new List<NurseryDto>());
        }

        var extractedFilters = await _llm.ExtractSearchFiltersAsync(message);

        if (extractedFilters.ExtractionFailed)
        {
            return new RecommendationResult(
                "معذرة، حصلت مشكلة مؤقتة في فهم تفاصيل طلبك، ممكن تحاول تبعت رسالتك تاني؟",
                new List<NurseryDto>());
        }

        var relevantNurseries = await _rag.SemanticSearchAsync(
            message, lat, lng, extractedFilters);

        if (!relevantNurseries.Any())
        {
            var noContextPrompt = """
            أنت مساعد ذكي لتطبيق حضانات في مصر.
            مفيش حضانات متاحة دلوقتي تطابق طلب المستخدم.
            رد بأدب واطلب منه يجرب كلام تاني أو يوسع منطقة البحث.
            رد بالعربي فقط.
            """;
            var noResultsReply = await _llm.GetChatResponseAsync(
                noContextPrompt, message, history);  // ✅ مرر history
            return new RecommendationResult(noResultsReply, new List<NurseryDto>());
        }

        var context = string.Join("\n", relevantNurseries.Select((n, i) =>
            $"{i + 1}. {n.Name} - {n.Location?.City} - " +
            $"السعر: {n.DailyPrice} جنيه/يوم - التقييم: {n.AvgRating}/5"));

        var systemPrompt = $"""
        أنت مساعد ذكي متخصص في مساعدة الآباء لإيجاد أفضل حضانة لأطفالهم في مصر.

        القائمة الوحيدة المسموح لك استخدامها في ردك هي الحضانات التالية، ولا يوجد غيرها حاليًا:

        {context}

        قواعد صارمة يجب اتباعها بدقة:
        - اعتمد فقط وبشكل كامل على القائمة أعلاه، ولا تستخدم أي معرفة سابقة لديك عن حضانات أخرى.
        - يُمنع منعًا تامًا ذكر أي اسم حضانة غير موجود حرفيًا في القائمة أعلاه.
        - يُمنع اختراع أسعار أو تقييمات مختلفة عن المذكورة أعلاه بالضبط.
        - رد بالعربي فقط.
        - اذكر الأسعار والتقييمات كما هي مذكورة في القائمة فقط.
        - كن ودوداً ومختصراً.
        - لو طلب المستخدم حجز، وضحله إنه يقدر يطلب الحجز بشكل مباشر وأنت هتظبطه.
        - إذا لم تجد في القائمة أعلاه ما يناسب طلب المستخدم تحديدًا، قل ذلك بصراحة.
        """;

        var aiReply = await _llm.GetChatResponseAsync(
            systemPrompt, message, history);  // ✅ مرر history

        var nurseryDtos = relevantNurseries.Select(n => new NurseryDto(
            n.Id, n.Name, n.DailyPrice, n.AvgRating,
            n.Location?.City, n.Location?.Address)).ToList();

        bool aiReplyMentionsAnyNursery = relevantNurseries.Any(n =>
            !string.IsNullOrWhiteSpace(aiReply) &&
            aiReply.Contains(n.Name, StringComparison.OrdinalIgnoreCase));

        if (!aiReplyMentionsAnyNursery)
            aiReply = BuildFallbackReplyFromData(relevantNurseries);

        return new RecommendationResult(aiReply, nurseryDtos);
    }


    // ✅ رد احتياطي مبني مباشرة من بيانات الداتابيز، بدون أي تدخل من الموديل
    //    يُستخدم فقط لو رد الموديل لم يحتوِ على أي اسم حضانة فعلي من القائمة (مؤشر هلوسة)
    private static string BuildFallbackReplyFromData(List<Core.Entities.Nursery> nurseries)
    {
        var lines = nurseries.Select((n, i) =>
            $"{i + 1}. {n.Name} - {n.Location?.City} - السعر: {n.DailyPrice} جنيه/يوم - التقييم: {n.AvgRating}/5");

        return "تمام، لقيت لك الحضانات المتاحة دي:\n\n" + string.Join("\n", lines);
    }

    public async Task GenerateAndSaveEmbeddingAsync(Nursery nursery)
        => await _rag.GenerateAndSaveEmbeddingAsync(nursery);


}