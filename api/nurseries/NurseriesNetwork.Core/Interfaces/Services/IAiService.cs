using NurseriesNetwork.Core.DTOs.AI;
using NurseriesNetwork.Core.Entities;

namespace NurseriesNetwork.Core.Interfaces.Services
{
    public interface IAiService
    {
        Task<RecommendationResult> GetRecommendationAsync(
        string message, double? lat, double? lng,
        IntentClassificationResult? precomputedIntent = null);

        Task GenerateAndSaveEmbeddingAsync(Nursery nursery);

    }
}
