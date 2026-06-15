using NurseriesNetwork.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Interfaces.Services
{
    public interface IAiService
    {
        Task<string> GetRecommendationAsync(string userMessage, double? lat, double? lng);
        Task GenerateAndSaveEmbeddingAsync(Nursery nursery);
    }
}
