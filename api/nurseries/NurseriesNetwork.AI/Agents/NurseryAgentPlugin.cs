using System.Text.Json;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Repositories;

namespace NurseriesNetwork.AI.Agents;

public class NurseryAgentPlugin
{
    private readonly IUnitOfWork _uow;

    public NurseryAgentPlugin(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // ===========================
    // البحث عن حضانات قريبة
    // ===========================
    public async Task<string> FindNearbyNurseriesAsync(
        double latitude,
        double longitude,
        double radiusKm = 10,
        decimal? maxPrice = null)
    {
        var nurseries = await _uow.Nurseries
            .GetNearbyAsync(latitude, longitude, radiusKm);

        if (maxPrice.HasValue)
            nurseries = nurseries
                .Where(n => n.DailyPrice <= maxPrice.Value);

        if (!nurseries.Any())
            return "مفيش حضانات في المنطقة دي";

        var result = nurseries.Select(n => new
        {
            n.Id,
            n.Name,
            City = n.Location?.City,
            n.DailyPrice,
            n.AvgRating
        });

        return JsonSerializer.Serialize(result);
    }

    // ===========================
    // عمل حجز تلقائي
    // ===========================
    public async Task<string> CreateBookingAsync(
        int nurseryId,
        string parentId,
        int childId,
        DateOnly startDate)
    {
        var nursery = await _uow.Nurseries.GetByIdAsync(nurseryId);
        if (nursery == null)
            return "الحضانة دي مش موجودة";

        if (!nursery.IsVerified)
            return "الحضانة دي لسه مش متأكدة من الإدارة";

        var booking = new Booking
        {
            NurseryId = nurseryId,
            ParentId = parentId,
            ChildId = childId,
            StartDate = startDate,
            TotalPrice = nursery.DailyPrice,
            Status = BookingStatus.Pending
        };

        await _uow.Bookings.AddAsync(booking);
        await _uow.SaveChangesAsync();

        return $"تم إنشاء الحجز بنجاح، رقم الحجز: {booking.Id}";
    }
}