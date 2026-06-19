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

    public BookingsController(
        IUnitOfWork uow,
        IEmailService emailService)
    {
        _uow = uow;
        _emailService = emailService;
    }

    // ===========================
    // POST: api/bookings
    // ===========================
    [HttpPost]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> CreateBooking(CreateBookingDto dto)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

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

        // بعت إيميل تأكيد الحجز
        var parent = await _uow.Children.GetByIdAsync(dto.ChildId);
        await _emailService.SendBookingConfirmationAsync(
            User.FindFirstValue(ClaimTypes.Email)!,
            booking.Id);

        return CreatedAtAction(nameof(GetBookingById),
            new { id = booking.Id }, booking);
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

        return Ok(bookings);
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

        return Ok(booking);
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
    [HttpPut("{id}/confirm")]
    [Authorize(Roles = "NurseryAdmin")]
    public async Task<IActionResult> ConfirmBooking(int id)
    {
        var booking = await _uow.Bookings.GetByIdAsync(id);
        if (booking == null)
            return NotFound("الحجز مش موجود");

        if (booking.Status != BookingStatus.Pending)
            return BadRequest("الحجز مش في انتظار التأكيد");

        booking.Status = BookingStatus.Confirmed;
        _uow.Bookings.Update(booking);
        await _uow.SaveChangesAsync();

        return Ok("تم تأكيد الحجز");
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