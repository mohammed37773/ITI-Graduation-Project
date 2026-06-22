using NurseriesNetwork.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Interfaces.Services
{
    public interface IAiService
    {
<<<<<<< HEAD
        Task<string> GetRecommendationAsync(string userMessage, double? lat, double? lng);
=======
        Task<string> GetRecommendationAsync(string message, double? lat, double? lng);
>>>>>>> main
        Task GenerateAndSaveEmbeddingAsync(Nursery nursery);
    }
}
