using NurseriesNetwork.Core.DTOs.AI;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Repositories;

namespace NurseriesNetwork.AI.Agents;

public class NurseryAdminAgentPlugin
{
    private readonly IUnitOfWork _uow;

    public NurseryAdminAgentPlugin(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // ===========================
    // ✅ تحليل أداء الحضانة (الأهم) — "إزاي حضانتي شغالة الشهر ده؟"
    // ===========================
    public async Task<string> GetNurseryPerformanceSummaryAsync(string adminUserId)
    {
        // ⚠️ هام: لازم Nursery Entity يكون فيها property بتربطها بالـ Admin (مثلاً OwnerId)
        //    عدّل الشرط ده حسب اسم العمود الفعلي عندك لو مختلف عن OwnerId
        var nursery = (await _uow.Nurseries.GetAllAsync())
            .FirstOrDefault(n => n.OwnerId == adminUserId);

        if (nursery == null)
            return "معذرة، لم يتم العثور على حضانة مرتبطة بحسابك.";

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var allBookings = await _uow.Bookings.FindAsync(b => b.NurseryId == nursery.Id);

        var bookingsThisMonth = allBookings
            .Where(b => b.CreatedAt >= startOfMonth)
            .ToList();

        var totalBookingsThisMonth = bookingsThisMonth.Count;

        var paidBookingsThisMonth = bookingsThisMonth
            .Count(b => b.Payment != null && b.Payment.Status == PaymentStatus.Completed);

        var pendingBookingsThisMonth = bookingsThisMonth
            .Count(b => b.Status == BookingStatus.Pending);

        var totalRevenueThisMonth = bookingsThisMonth
            .Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Completed)
            .Sum(b => b.Payment!.Amount);

        var reviews = await _uow.Reviews.FindAsync(r => r.NurseryId == nursery.Id);
        var totalReviewsCount = reviews.Count();

        var data = new NurseryPerformanceData(
            nursery.Name,
            totalBookingsThisMonth,
            paidBookingsThisMonth,
            pendingBookingsThisMonth,
            totalRevenueThisMonth,
            nursery.AvgRating,
            totalReviewsCount
        );

        // ✅ نرجع البيانات كـ JSON منظم — الموديل هو اللي هيصيغها كنص بشري بعد كده
        return System.Text.Json.JsonSerializer.Serialize(data);
    }

    // ===========================
    // ✅ البحث الذكي في الحجوزات — "عايز أعرف الحجوزات اللي لسه Pending ومدفوعة"
    // ===========================
    public async Task<string> SearchMyBookingsAsync(
        string adminUserId, AdminBookingSearchFilters filters)
    {
        var nursery = (await _uow.Nurseries.GetAllAsync())
            .FirstOrDefault(n => n.OwnerId == adminUserId);

        if (nursery == null)
            return "معذرة، لم يتم العثور على حضانة مرتبطة بحسابك.";

        var bookings = (await _uow.Bookings.FindAsync(b => b.NurseryId == nursery.Id)).ToList();

        // فلترة بالحالة (Pending, Confirmed, Cancelled, Completed)
        if (!string.IsNullOrWhiteSpace(filters.BookingStatus) &&
            Enum.TryParse<BookingStatus>(filters.BookingStatus, true, out var bookingStatusEnum))
        {
            bookings = bookings.Where(b => b.Status == bookingStatusEnum).ToList();
        }

        // فلترة بحالة الدفع (Pending, Paid, Failed, Refunded)
        if (!string.IsNullOrWhiteSpace(filters.PaymentStatus) &&
            Enum.TryParse<PaymentStatus>(filters.PaymentStatus, true, out var paymentStatusEnum))
        {
            bookings = bookings
                .Where(b => b.Payment != null && b.Payment.Status == paymentStatusEnum)
                .ToList();
        }

        // فلترة بعمر الطفل (بالشهور) وقت بدء الحجز
        if (filters.MaxChildAgeMonths.HasValue || filters.MinChildAgeMonths.HasValue)
        {
            bookings = bookings.Where(b =>
            {
                if (b.Child == null) return false;

                var ageInMonths = (b.StartDate.Year - b.Child.DateOfBirth.Year) * 12 +
                                   b.StartDate.Month - b.Child.DateOfBirth.Month;

                if (filters.MaxChildAgeMonths.HasValue && ageInMonths > filters.MaxChildAgeMonths.Value)
                    return false;

                if (filters.MinChildAgeMonths.HasValue && ageInMonths < filters.MinChildAgeMonths.Value)
                    return false;

                return true;
            }).ToList();
        }

        // فلترة بآخر X يوم (مثلاً "الأسبوع ده" = 7)
        if (filters.WithinLastDays.HasValue)
        {
            var cutoff = DateTime.UtcNow.AddDays(-filters.WithinLastDays.Value);
            bookings = bookings.Where(b => b.CreatedAt >= cutoff).ToList();
        }

        if (!bookings.Any())
            return "مفيش حجوزات مطابقة للمعايير دي حاليًا.";

        var result = bookings.Take(20).Select(b => new
        {
            BookingId = b.Id,
            ChildName = b.Child?.FullName,
            ChildAgeMonths = b.Child != null
                ? (b.StartDate.Year - b.Child.DateOfBirth.Year) * 12 + b.StartDate.Month - b.Child.DateOfBirth.Month
                : (int?)null,
            StartDate = b.StartDate.ToString("yyyy-MM-dd"),
            BookingStatus = b.Status.ToString(),
            PaymentStatus = b.Payment?.Status.ToString() ?? "NoPayment",
            TotalPrice = b.TotalPrice
        });

        return System.Text.Json.JsonSerializer.Serialize(result);
    }
}