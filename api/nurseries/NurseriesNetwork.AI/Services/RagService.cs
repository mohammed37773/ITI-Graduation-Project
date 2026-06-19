using System.Text.Json;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces.Repositories;

namespace NurseriesNetwork.AI.Services;

public class RagService
{
    private readonly GeminiService _gemini;
    private readonly IUnitOfWork _uow;

    public RagService(GeminiService gemini, IUnitOfWork uow)
    {
        _gemini = gemini;
        _uow = uow;
    }

    // خطوة 1: لما تتضاف حضانة، نعمل لها Embedding
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

    // خطوة 2: البحث عن أقرب حضانات لسؤال المستخدم
    public async Task<List<Nursery>> SemanticSearchAsync(string userQuery)
    {
        var queryEmbedding = await _gemini.GetEmbeddingAsync(userQuery);
        var nurseries = await _uow.Nurseries.GetAllWithEmbeddingsAsync();

        return nurseries
            .Where(n => !string.IsNullOrEmpty(n.EmbeddingVector))
            .Select(n => new
            {
                Nursery = n,
                Score = CosineSimilarity(
                    queryEmbedding,
                    JsonSerializer.Deserialize<float[]>(n.EmbeddingVector!)!)
            })
            .OrderByDescending(x => x.Score)
            .Take(5)
            .Select(x => x.Nursery)
            .ToList();
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        var dot = a.Zip(b, (x, y) => x * y).Sum();
        var magA = Math.Sqrt(a.Sum(x => x * x));
        var magB = Math.Sqrt(b.Sum(x => x * x));
        return magA == 0 || magB == 0
            ? 0
            : (float)(dot / (magA * magB));
    }
}