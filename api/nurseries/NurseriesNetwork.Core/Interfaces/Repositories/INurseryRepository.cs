using NurseriesNetwork.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
namespace NurseriesNetwork.Core.Interfaces.Repositories
{
    public interface INurseryRepository : IGenericRepository<Nursery>
    {
        Task<IEnumerable<Nursery>> GetNearbyAsync(double lat, double lng, double radiusKm);
        Task<IEnumerable<Nursery>> FilterAsync(decimal? maxPrice, double? minRating, string? city);
        Task<Nursery?> GetWithReviewsAsync(int id);
        Task<IEnumerable<Nursery>> GetAllWithEmbeddingsAsync();
    }
}
