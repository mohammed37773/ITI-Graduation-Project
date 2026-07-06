using Microsoft.EntityFrameworkCore;
using NurseriesNetwork.Core.DTOs.Booking;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Infrastructure.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context) : base(context) { }

        public async Task<List<ReadBookingDto>?> GetWithDetailsAsync(string parentId)
        {
            return await _context.Bookings
                .Where(b => b.ParentId == parentId)
                .Select(b => new ReadBookingDto
                {
                    Id = b.Id,
                    NurseryId = b.NurseryId,
                    NurseryName = b.Nursery.Name,
                    ChildId = b.ChildId,
                    ChildName = b.Child.FullName,
                    StartDate = b.StartDate,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt,
                }).ToListAsync();
        }

        public async Task<List<ReadBookingDto>?> GetNurseryBookingsWithDetailsAsync(string ownerId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

            return await _context.Bookings
                .AsNoTracking()
                .Where(b => b.Nursery.OwnerId == ownerId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new ReadBookingDto
                {
                    Id = b.Id,
                    NurseryId = b.NurseryId,
                    NurseryName = b.Nursery.Name,
                    ChildId = b.ChildId,
                    ChildName = b.Child.FullName,
                    StartDate = b.StartDate,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();
        }
        public async Task<decimal?> CompleteBookingAsync(int bookingId, string ownerId)
        {
            // 1. جلب الحجز مع بيانات الحضانة
            var booking = await _context.Bookings
                .Include(b => b.Nursery)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return null;

            // قفل الأمان: التأكد من الملكية والحالة
            if (booking.Nursery.OwnerId != ownerId)
                return null;

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                return null;

            // 2. تسجيل تاريخ الانتهاء باللحظة الحالية
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            booking.EndDate = today;

            // 3. حساب عدد الأيام الكلي (بما فيهم أول يوم)
            int totalDays = today.DayNumber - booking.StartDate.DayNumber;
            if (totalDays <= 0) totalDays = 1; // حد أدنى يوم واحد إذا غادر في نفس اليوم

            // 4. حساب السعر الإجمالي الكلي وحفظه في الداتا بيز
            decimal oneDayPrice = booking.Nursery.DailyPrice; // تأكد من مسمى الحقل في كلاس الحضانة
            booking.TotalPrice = totalDays * oneDayPrice;

            booking.Status = BookingStatus.Completed;

            // 6. زيادة الأماكن المتاحة (+1) لأن الطفل غادر المكان (السعة الإجمالية تظل ثابتة)
            booking.Nursery.AvailablePlaces += 1;

            // حفظ التغييرات في الداتا بيز
            var saved = await _context.SaveChangesAsync() > 0;
            if (!saved) return null;

            // 6. حساب المبلغ المراد إرجاعه للفرونت إند (إجمالي المبلغ - سعر أول يوم)
            decimal amountToReturn = booking.TotalPrice - oneDayPrice;

            return amountToReturn;
        }

    }
}
