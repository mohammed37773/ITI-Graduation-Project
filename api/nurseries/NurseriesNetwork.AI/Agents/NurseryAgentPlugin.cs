using System.Text.Json;
using Microsoft.Extensions.Logging;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Core.Interfaces.Services;

namespace NurseriesNetwork.AI.Agents;

public class NurseryAgentPlugin
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;
    private readonly ILogger<NurseryAgentPlugin> _logger;

    public NurseryAgentPlugin(
        IUnitOfWork uow,
        IEmailService emailService,
        ILogger<NurseryAgentPlugin> logger)
    {
        _uow = uow;
        _emailService = emailService;
        _logger = logger;
    }

    // ===========================
    // البحث عن حضانات — بالاسم/المدينة بدل Coordinates
    // ===========================
    public async Task<string> FindNurseriesAsync(string? city, decimal? maxPrice)
    {
        var nurseries = await _uow.Nurseries
            .FilterAsync(maxPrice, null, city);

        if (!nurseries.Any())
            return "مفيش حضانات متاحة بالمعايير دي";

        var result = nurseries.Take(5).Select(n => new
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
    // إنشاء حجز — بالاسم بدل الـ ID مباشرة
    // ===========================
    public async Task<string> CreateBookingByNameAsync(
        string nurseryName,
        string parentId,
        string childName,
        DateOnly startDate,
        string? parentEmail = null)
    {
        // البحث عن الحضانة بالاسم
        var allNurseries = await _uow.Nurseries.GetAllAsync();
        var nursery = allNurseries.FirstOrDefault(n =>
            n.Name.Contains(nurseryName, StringComparison.OrdinalIgnoreCase));

        if (nursery == null)
            return $"معذرة، مفيش حضانة بإسم '{nurseryName}'";

        if (!nursery.IsVerified)
            return "الحضانة دي لسه مش متأكدة من الإدارة";

        // البحث عن الطفل بالاسم لنفس الـ Parent
        var children = await _uow.Children
            .FindAsync(c => c.ParentId == parentId);
        var child = children.FirstOrDefault(c =>
            c.FullName.Contains(childName, StringComparison.OrdinalIgnoreCase));

        if (child == null)
            return $"معذرة، مفيش طفل مسجل بإسم '{childName}' في حسابك";

        // فحص السن
        var ageInMonths = (startDate.Year - child.DateOfBirth.Year) * 12 +
                           startDate.Month - child.DateOfBirth.Month;

        if (ageInMonths < nursery.AgeRangeMin || ageInMonths > nursery.AgeRangeMax)
            return "سن الطفل مش مناسب لرينج السن المسموح في الحضانة دي";

        // فحص تكرار الحجز
        var existingBookings = await _uow.Bookings.FindAsync(b =>
            b.NurseryId == nursery.Id &&
            b.ChildId == child.Id &&
            b.Status != BookingStatus.Cancelled);

        if (existingBookings.Any())
            return "الطفل ده عنده حجز قائم بالفعل في الحضانة دي";

        var booking = new Booking
        {
            NurseryId = nursery.Id,
            ParentId = parentId,
            ChildId = child.Id,
            StartDate = startDate,
            TotalPrice = nursery.DailyPrice,
            Status = BookingStatus.Pending
        };

        await _uow.Bookings.AddAsync(booking);
        await _uow.SaveChangesAsync();

        if (!string.IsNullOrEmpty(parentEmail))
        {
            try
            {
                await _emailService.SendBookingConfirmationAsync(
                    parentEmail, booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Agent: Failed to send booking confirmation email for BookingId: {BookingId}",
                    booking.Id);
            }
        }

        return $"تم إنشاء الحجز بنجاح في حضانة {nursery.Name} للطفل {child.FullName}، رقم الحجز: {booking.Id}";
    }
}