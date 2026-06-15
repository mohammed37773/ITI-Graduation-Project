using Microsoft.EntityFrameworkCore;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Infrastructure.Data;

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
                var dLat = ToRad(n.Location!.Latitude - lat);
                var dLng = ToRad(n.Location!.Longitude - lng);

                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(ToRad(lat)) * Math.Cos(ToRad(n.Location.Latitude)) *
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

        public async Task<Nursery?> GetWithDetailsAsync(int id)
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

        private static double ToRad(double degrees) => degrees * Math.PI / 180;
    }
}