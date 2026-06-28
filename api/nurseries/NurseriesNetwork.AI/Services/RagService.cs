using System.Text.Json;
using Microsoft.Extensions.Logging;
using NurseriesNetwork.Core.DTOs.AI;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces.Repositories;

namespace NurseriesNetwork.AI.Services;

public class RagService
{
    // ✅ الـ Embedding يفضل معتمد على Gemini فقط (لا يوجد Fallback له لأن Groq/Grok لا يدعمان Embeddings)
    private readonly GeminiService _gemini;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<RagService> _logger; // ⚠️ مؤقت للتشخيص

    public RagService(GeminiService gemini, IUnitOfWork uow, ILogger<RagService> logger)
    {
        _gemini = gemini;
        _uow = uow;
        _logger = logger;
    }

    // ===========================
    // خطوة 1: لما تتضاف حضانة، نعمل لها Embedding
    // ===========================
    public async Task GenerateAndSaveEmbeddingAsync(Nursery nursery)
    {
        var text = $"اسم الحضانة: {nursery.Name}. " +
                   $"العنوان: {nursery.Location?.Address}. " +
                   $"المدينة: {nursery.Location?.City}. " +
                   $"السعر اليومي: {nursery.DailyPrice} جنيه. " +
                   $"التقييم: {nursery.AvgRating} من 5. " +
                   $"السن من {nursery.AgeRangeMin} لـ {nursery.AgeRangeMax} شهر. " +
                   $"الوصف: {nursery.Description}";

        var embedding = await _gemini.GetEmbeddingAsync(text);

        nursery.EmbeddingVector = JsonSerializer.Serialize(embedding);
        _uow.Nurseries.Update(nursery);
        await _uow.SaveChangesAsync();
    }

    // ===========================
    // خطوة 2: البحث عن أقرب حضانات لسؤال المستخدم
    // ===========================
    public async Task<List<Nursery>> SemanticSearchAsync(
        string userQuery,
        double? lat = null,
        double? lng = null,
        SearchFilters? filters = null)
    {
        var nurseries = await _uow.Nurseries.GetAllWithEmbeddingsAsync();

        // 🔍 DEBUG 1: كل الحضانات الراجعة من GetAllWithEmbeddingsAsync قبل أي فلترة
        _logger.LogInformation(
            $"RAG Debug [1]: GetAllWithEmbeddingsAsync returned {nurseries.Count()} nurseries total.");
        foreach (var n in nurseries)
        {
            _logger.LogInformation(
                $"RAG Debug [1.a]: Id={n.Id}, Name={n.Name}, City={n.Location?.City ?? "(null)"}, " +
                $"IsVerified={n.IsVerified}, HasEmbedding={!string.IsNullOrEmpty(n.EmbeddingVector)}");
        }

        var candidates = nurseries
            .Where(n => !string.IsNullOrEmpty(n.EmbeddingVector))
            .Where(n => n.IsVerified) // لا يجوز ترشيح حضانة غير موثقة
            .ToList();

        // 🔍 DEBUG 2: بعد فلتر Embedding + Verified
        _logger.LogInformation(
            $"RAG Debug [2]: After Embedding+Verified filter -> {candidates.Count} candidates remain: " +
            $"[{string.Join(", ", candidates.Select(c => c.Name + "/" + c.Location?.City))}]");

        // 🔍 DEBUG 3: الفلاتر المستخرجة من رسالة المستخدم
        _logger.LogInformation(
            $"RAG Debug [3]: Filters received -> City='{filters?.City ?? "(null)"}', " +
            $"MaxPrice={filters?.MaxPrice}, MinRating={filters?.MinRating}, " +
            $"ChildAgeMonths={filters?.ChildAgeMonths}, ExtractionFailed={filters?.ExtractionFailed ?? false}");

        // ✅ لو فشل استخراج الفلاتر فعليًا (خطأ API)، ميصحش نتعامل معاها كـ "مفيش فلاتر"
        if (filters != null && filters.ExtractionFailed)
        {
            _logger.LogWarning("RAG Debug: Returning empty - ExtractionFailed=true");
            return new List<Nursery>();
        }

        // ✅ فلترة منطقية صريحة أولًا (City / Price / Rating / Age) — قبل أي Semantic Scoring
        bool hadExplicitFilters = false;

        if (filters != null)
        {
            if (!string.IsNullOrWhiteSpace(filters.City))
            {
                hadExplicitFilters = true;
                candidates = candidates.Where(n =>
                    n.Location?.City != null &&
                    n.Location.City.Contains(filters.City, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                _logger.LogInformation(
                    $"RAG Debug [4]: After City filter '{filters.City}' -> {candidates.Count} candidates remain: " +
                    $"[{string.Join(", ", candidates.Select(c => c.Name + "/" + c.Location?.City))}]");
            }

            if (filters.MaxPrice.HasValue)
            {
                hadExplicitFilters = true;
                candidates = candidates.Where(n => n.DailyPrice <= filters.MaxPrice.Value).ToList();
                _logger.LogInformation(
                    $"RAG Debug [5]: After MaxPrice filter <= {filters.MaxPrice.Value} -> {candidates.Count} candidates remain");
            }

            if (filters.MinRating.HasValue)
            {
                hadExplicitFilters = true;
                candidates = candidates.Where(n => n.AvgRating >= filters.MinRating.Value).ToList();
                _logger.LogInformation(
                    $"RAG Debug [6]: After MinRating filter >= {filters.MinRating.Value} -> {candidates.Count} candidates remain");
            }

            if (filters.ChildAgeMonths.HasValue)
            {
                hadExplicitFilters = true;
                candidates = candidates.Where(n =>
                    filters.ChildAgeMonths.Value >= n.AgeRangeMin &&
                    filters.ChildAgeMonths.Value <= n.AgeRangeMax)
                    .ToList();
                _logger.LogInformation(
                    $"RAG Debug [7]: After ChildAgeMonths filter ({filters.ChildAgeMonths.Value}) -> {candidates.Count} candidates remain");
            }
        }

        // ✅ لو فيه فلاتر صريحة (زي مدينة محددة) ومفيش نتائج بعد الفلترة، رجّع قائمة فاضية فورًا
        if (hadExplicitFilters && !candidates.Any())
        {
            _logger.LogWarning(
                $"RAG Debug [8]: Returning EMPTY because hadExplicitFilters=true and candidates.Count=0 after filtering.");
            return new List<Nursery>();
        }

        // ✅ الـ Semantic Search دلوقتي بيرتب فقط (Ranking)، مش بيفلتر (Filtering)
        var queryEmbedding = await _gemini.GetEmbeddingAsync(userQuery);

        _logger.LogInformation(
            $"RAG Debug [9]: Query embedding length = {queryEmbedding.Length} (0 means embedding call failed)");

        var scored = candidates
            .Select(n => new
            {
                Nursery = n,
                Score = CosineSimilarity(
                    queryEmbedding,
                    JsonSerializer.Deserialize<float[]>(n.EmbeddingVector!)!)
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        _logger.LogInformation(
            $"RAG Debug [10]: Scored candidates -> " +
            $"[{string.Join(", ", scored.Select(s => $"{s.Nursery.Name}={s.Score:F3}"))}]");

        var result = scored.Select(x => x.Nursery).ToList();

        // ✅ ترتيب إضافي بالقرب الجغرافي لو الموقع متوفر
        if (lat.HasValue && lng.HasValue)
        {
            result = result
                .OrderBy(n => DistanceKm(
                    lat.Value, lng.Value,
                    n.Location?.Latitude ?? 0,
                    n.Location?.Longitude ?? 0))
                .ToList();
        }

        var final = result.Take(5).ToList();

        _logger.LogInformation(
            $"RAG Debug [11]: FINAL result -> [{string.Join(", ", final.Select(n => n.Name))}]");

        return final;
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length == 0 || b.Length == 0 || a.Length != b.Length) return 0;

        var dot = a.Zip(b, (x, y) => x * y).Sum();
        var magA = Math.Sqrt(a.Sum(x => x * x));
        var magB = Math.Sqrt(b.Sum(x => x * x));
        return magA == 0 || magB == 0
            ? 0
            : (float)(dot / (magA * magB));
    }

    // ✅ حساب المسافة بين نقطتين جغرافيتين (Haversine formula)
    private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        if (lat2 == 0 && lon2 == 0) return double.MaxValue; // حضانة بدون موقع تتأخر في الترتيب

        const double R = 6371; // نصف قطر الأرض بالكيلومتر
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double deg) => deg * Math.PI / 180;
}