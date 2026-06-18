using Microsoft.EntityFrameworkCore;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Infrastructure.Data;
using NurseriesNetwork.Infrastructure.Services;

namespace NurseriesNetwork.Infrastructure.Repositories
{

    public class NurseryRepository : GenericRepository<Nursery>, INurseryRepository
    {
        public NurseryRepository(AppDbContext context) : base(context) { }

        // Nearby Nurseries using Haversine Formula
        public async Task<IEnumerable<Nursery>> GetNearbyAsync(double lat, double lng, double radiusKm)
        {
            const double EarthRadiusKm = 6371;

            var nurseries = await _dbSet
                .Include(n => n.Location)
                .Include(n => n.Images.Where(i => i.IsMain))
                .Where(n => n.Location != null)
                .ToListAsync(); 

            return nurseries.Where(n =>
            {
                var dLat = Utilities.ToRad(n.Location!.Latitude - lat);
                var dLng = Utilities.ToRad(n.Location!.Longitude - lng);

                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(Utilities.ToRad(lat)) * Math.Cos(Utilities.ToRad(n.Location.Latitude)) *
                        Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

                var distance = 2 * EarthRadiusKm * Math.Asin(Math.Sqrt(a));
                return distance <= radiusKm;
            });
        }

        public async Task<IEnumerable<Nursery>> FilterAsync(decimal? maxPrice, double? minRating, string? city)
        {
            var query = _dbSet
                .Include(n => n.Location)
                .Include(n => n.Images.Where(i => i.IsMain))
                .AsQueryable();

            if (maxPrice.HasValue)
                query = query.Where(n => n.DailyPrice <= maxPrice.Value);

            if (minRating.HasValue)
                query = query.Where(n => n.AvgRating >= minRating.Value);

            if (!string.IsNullOrEmpty(city))
                query = query.Where(n => n.Location != null && n.Location.City == city);

            return await query.ToListAsync();
        }

        public async Task<Nursery?> GetWithReviewsAsync(int id)
        {
            return await _dbSet
                .Include(n => n.Location)
                .Include(n => n.Images)
                .Include(n => n.Reviews)
                    .ThenInclude(r => r.Parent)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<Nursery>> GetAllWithEmbeddingsAsync()
        {
            return await _dbSet
                .Include(n => n.Location)
                .Where(n => n.EmbeddingVector != null)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Nursery>> GetAllAsync()
        {
            return await _dbSet
                .Include(n => n.Location)
                .Include(n => n.Images)
                .ToListAsync();
        }

        public override async Task<Nursery?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(n => n.Location)
                .Include(n => n.Images)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

    }
}