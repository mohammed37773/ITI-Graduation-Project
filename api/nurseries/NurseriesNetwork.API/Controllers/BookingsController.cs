using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NurseriesNetwork.Core.DTOs.Booking;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Core.Interfaces.Services;

namespace NurseriesNetwork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;
    private readonly ILogger<BookingsController> _logger;


    public BookingsController(
        IUnitOfWork uow,
        IEmailService emailService,
         ILogger<BookingsController> logger)
    {
        _uow = uow;
        _emailService = emailService;
        _logger = logger;
    }

    // ===========================
    // POST: api/bookings
    // ===========================
    [HttpPost]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> CreateBooking(CreateBookingDto dto)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ✅ فحص جديد — تاريخ البداية لازم يكون في المستقبل
        if (dto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            return BadRequest("تاريخ البداية لازم يكون في المستقبل");
        // التحقق من الحضانة
        var nursery = await _uow.Nurseries.GetByIdAsync(dto.NurseryId);
        if (nursery == null)
            return NotFound("الحضانة مش موجودة");

        if (!nursery.IsVerified)
            return BadRequest("الحضانة دي مش متأكدة من الإدارة لسه");

        // التحقق من الطفل
        var child = await _uow.Children.GetByIdAsync(dto.ChildId);
        if (child == null || child.ParentId != parentId)
            return NotFound("الطفل مش موجود");

        // التحقق من السن
        var ageInMonths = CalculateAgeInMonths(child.DateOfBirth);
        if (ageInMonths < nursery.AgeRangeMin ||
            ageInMonths > nursery.AgeRangeMax)
            return BadRequest("سن الطفل مش مناسب للحضانة دي");

        // ✅ فحص جديد — منع الحجز المكرر لنفس الطفل في نفس الحضانة
        var existingBookings = await _uow.Bookings.FindAsync(b =>
            b.NurseryId == dto.NurseryId &&
            b.ChildId == dto.ChildId &&
            b.Status != BookingStatus.Cancelled);

        if (existingBookings.Any())
            return BadRequest("الطفل ده عنده حجز قائم بالفعل في الحضانة دي");

        // حساب السعر
        var totalPrice = nursery.DailyPrice;

        var booking = new Booking
        {
            ParentId = parentId,
            NurseryId = dto.NurseryId,
            ChildId = dto.ChildId,
            StartDate = dto.StartDate,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending
        };

        await _uow.Bookings.AddAsync(booking);
        await _uow.SaveChangesAsync();

        // بعت إيميل تأكيد الحجز - لو فشل، لا يمنع نجاح الحجز نفسه
        try
        {
            await _emailService.SendBookingConfirmationAsync(
                User.FindFirstValue(ClaimTypes.Email)!,
                booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send booking confirmation email for BookingId: {BookingId}",
                booking.Id);
        }

        return CreatedAtAction(nameof(GetBookingById),
            new { id = booking.Id }, new
            {
                booking.Id,
                booking.NurseryId,
                booking.ChildId,
                booking.StartDate,
                booking.TotalPrice,
                Status = booking.Status.ToString(),
                booking.CreatedAt
            });
    }


    // ===========================
    // GET: api/bookings/my
    // ===========================

    [HttpGet("my")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> GetMyBookings()
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var bookings = await _uow.Bookings
            .FindAsync(b => b.ParentId == parentId);

        return Ok(bookings.Select(b => new
        {
            b.Id,
            b.NurseryId,
            b.ChildId,
            b.StartDate,
            b.TotalPrice,
            Status = b.Status.ToString(),
            b.CreatedAt
        }));
    }

    // ===========================
    // GET: api/bookings/{id}
    // ===========================

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookingById(int id)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var booking = await _uow.Bookings.GetByIdAsync(id);
        if (booking == null)
            return NotFound("الحجز مش موجود");

        // Parent يشوف حجوزاته بس
        if (booking.ParentId != parentId &&
            !User.IsInRole("Admin"))
            return Forbid();

        return Ok(new
        {
            booking.Id,
            booking.NurseryId,
            booking.ChildId,
            booking.StartDate,
            booking.TotalPrice,
            Status = booking.Status.ToString(),
            booking.CreatedAt
        });
    }

    // ===========================
    // PUT: api/bookings/{id}/cancel
    // ===========================
    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var booking = await _uow.Bookings.GetByIdAsync(id);
        if (booking == null)
            return NotFound("الحجز مش موجود");

        if (booking.ParentId != parentId)
            return Forbid();

        if (booking.Status == BookingStatus.Cancelled)
            return BadRequest("الحجز ده ملغي بالفعل");

        if (booking.Status == BookingStatus.Confirmed)
            return BadRequest("مش ممكن تلغي حجز مؤكد");

        booking.Status = BookingStatus.Cancelled;
        _uow.Bookings.Update(booking);
        await _uow.SaveChangesAsync();

        return Ok("تم إلغاء الحجز");
    }

    // ===========================
    // PUT: api/bookings/{id}/confirm
    // NurseryAdmin بيأكد الحجز
    // ===========================
    //[HttpPut("{id}/confirm")]
    //[Authorize(Roles = "NurseryAdmin")]
    //public async Task<IActionResult> ConfirmBooking(int id)
    //{
    //    var booking = await _uow.Bookings.GetByIdAsync(id);
    //    if (booking == null)
    //        return NotFound("الحجز مش موجود");

    //    if (booking.Status != BookingStatus.Pending)
    //        return BadRequest("الحجز مش في انتظار التأكيد");

    //    booking.Status = BookingStatus.Confirmed;
    //    _uow.Bookings.Update(booking);
    //    await _uow.SaveChangesAsync();

    //    return Ok("تم تأكيد الحجز");
    //}


    // ===========================
    // GET: api/bookings/nursery
    // NurseryAdmin يشوف الحجوزات المؤكدة (المدفوعة) بتاعت حضانته
    // ===========================
    [HttpGet("nursery/{nurseryId}")]
    [Authorize(Roles = "NurseryAdmin")]
    public async Task<IActionResult> GetNurseryBookings(int nurseryId)
    {
        var bookings = await _uow.Bookings
            .FindAsync(b => b.NurseryId == nurseryId);

        return Ok(bookings.Select(b => new
        {
            b.Id,
            b.ChildId,
            b.StartDate,
            b.TotalPrice,
            Status = b.Status.ToString(),
            b.CreatedAt
        }));
    }
    // ===========================
    // Helper
    // ===========================
    private static int CalculateAgeInMonths(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return (today.Year - dateOfBirth.Year) * 12 +
               today.Month - dateOfBirth.Month;
    }
}