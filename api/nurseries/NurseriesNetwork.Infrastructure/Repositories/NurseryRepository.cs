using Microsoft.EntityFrameworkCore;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Infrastructure.Data;

namespace NurseriesNetwork.Infrastructure.Repositories;

public class NurseryRepository : GenericRepository<Nursery>, INurseryRepository
{
    public NurseryRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Nursery>> GetNearbyAsync(
        double lat, double lng, double radiusKm)
    {
        var all = await _context.Nurseries
            .Include(n => n.Location)
            .Include(n => n.Images)
            .Where(n => n.Location != null)
            .ToListAsync();

        return all
            .Select(n => new
            {
                Nursery = n,
                Distance = HaversineDistance(
                    lat, lng,
                    n.Location!.Latitude, n.Location.Longitude)
            })
            .Where(x => x.Distance <= radiusKm)
            .OrderBy(x => x.Distance)
            .Select(x => x.Nursery)
            .ToList();
    }

    public async Task<IEnumerable<Nursery>> FilterAsync(
        decimal? maxPrice, double? minRating, string? city)
    {
        var query = _context.Nurseries
            .Include(n => n.Location)
            .Include(n => n.Images)
            .AsQueryable();

        if (maxPrice.HasValue)
            query = query.Where(n => n.DailyPrice <= maxPrice.Value);

        if (minRating.HasValue)
            query = query.Where(n => n.AvgRating >= minRating.Value);

        if (!string.IsNullOrEmpty(city))
            query = query.Where(n =>
                n.Location != null && n.Location.City.Contains(city));

        return await query.ToListAsync();
    }

    public async Task<Nursery?> GetWithDetailsAsync(int id)
    {
        return await _context.Nurseries
            .Include(n => n.Location)
            .Include(n => n.Images)
            .Include(n => n.Reviews)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Nursery>> GetAllWithEmbeddingsAsync()
    {
        return await _context.Nurseries
            .Include(n => n.Location)   // ✅ ده المفروض كان موجود من الأول!
            .Where(n => n.EmbeddingVector != null)
            .ToListAsync();
    }

    private static double HaversineDistance(
        double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) *
                Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    
}