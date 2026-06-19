using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Repositories;

namespace NurseriesNetwork.API.Controllers.Admin;

[ApiController]
[Route("api/admin/bookings")]
[Authorize(Roles = "Admin")]
public class AdminBookingsController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public AdminBookingsController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // ===========================
    // GET: api/admin/bookings
    // ===========================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bookings = await _uow.Bookings.GetAllAsync();
        return Ok(bookings);
    }

    // ===========================
    // GET: api/admin/bookings/stats
    // ===========================
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var bookings = await _uow.Bookings.GetAllAsync();

        return Ok(new
        {
            Total = bookings.Count(),
            Pending = bookings.Count(b =>
                b.Status == BookingStatus.Pending),
            Confirmed = bookings.Count(b =>
                b.Status == BookingStatus.Confirmed),
            Cancelled = bookings.Count(b =>
                b.Status == BookingStatus.Cancelled),
            TotalRevenue = bookings
                .Where(b => b.Status == BookingStatus.Confirmed)
                .Sum(b => b.TotalPrice)
        });
    }
}