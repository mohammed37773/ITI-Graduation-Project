using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Core.Interfaces.Services;

namespace NurseriesNetwork.API.Controllers.Admin;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = "Admin")]
public class AdminPaymentsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IPaymentFactory _paymentFactory;

    public AdminPaymentsController(
        IUnitOfWork uow,
        IPaymentFactory paymentFactory)
    {
        _uow = uow;
        _paymentFactory = paymentFactory;
    }

    // ===========================
    // GET: api/admin/payments
    // ===========================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var payments = await _uow.Payments.GetAllAsync();
        return Ok(payments);
    }

    // ===========================
    // GET: api/admin/payments/stats
    // ===========================
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var payments = await _uow.Payments.GetAllAsync();

        return Ok(new
        {
            Total = payments.Count(),
            Completed = payments.Count(p =>
                p.Status == PaymentStatus.Completed),
            Failed = payments.Count(p =>
                p.Status == PaymentStatus.Failed),
            Refunded = payments.Count(p =>
                p.Status == PaymentStatus.Refunded),
            TotalRevenue = payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount),
            ByMethod = new
            {
                VodafoneCash = payments.Count(p =>
                    p.Method == PaymentMethod.VodafoneCash),
                Meeza = payments.Count(p =>
                    p.Method == PaymentMethod.Meeza),
                Card = payments.Count(p =>
                    p.Method == PaymentMethod.Card),
                PayPal = payments.Count(p =>
                    p.Method == PaymentMethod.PayPal)
            }
        });
    }

    // ===========================
    // POST: api/admin/payments/{id}/refund
    // ===========================
    // غيّر الـ Refund method بس — الباقي زي ما هو

    [HttpPost("{id}/refund")]
    public async Task<IActionResult> Refund(int id)
    {
        var payment = await _uow.Payments.GetByIdAsync(id);
        if (payment == null)
            return NotFound(new { msg = "الدفعة مش موجودة" });

        if (payment.Status != PaymentStatus.Completed)
            return BadRequest(new { msg = "مش ممكن ترجع دفعة مش مكتملة" });

        // ✅ بدل ProcessPaymentAsync استخدم RefundAsync
        var paymentService = _paymentFactory
            .GetPaymentService(payment.Method);

        var refunded = await paymentService
            .RefundAsync(payment.TransactionId!);

        if (!refunded)
            return BadRequest(new { msg = "فشل الاسترداد" });

        payment.Status = PaymentStatus.Refunded;
        _uow.Payments.Update(payment);

        var booking = await _uow.Bookings
            .GetByIdAsync(payment.BookingId);
        if (booking != null)
        {
            booking.Status = BookingStatus.Cancelled;
            _uow.Bookings.Update(booking);
        }

        await _uow.SaveChangesAsync();

        return Ok(new { msg = "تم استرداد المبلغ بنجاح" });
    }
    // ===========================
    // GET: api/admin/reports/summary
    // ===========================
    [HttpGet("/api/admin/reports/summary")]
    public async Task<IActionResult> GetSummary()
    {
        var payments = await _uow.Payments.GetAllAsync();
        var bookings = await _uow.Bookings.GetAllAsync();
        var nurseries = await _uow.Nurseries.GetAllAsync();

        return Ok(new
        {
            TotalRevenue = payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount),
            TotalBookings = bookings.Count(),
            TotalNurseries = nurseries.Count(),
            VerifiedNurseries = nurseries
                .Count(n => n.IsVerified),
            ThisMonth = new
            {
                Revenue = payments
                    .Where(p =>
                        p.Status == PaymentStatus.Completed &&
                        p.CreatedAt.Month == DateTime.Now.Month)
                    .Sum(p => p.Amount),
                Bookings = bookings
                    .Count(b =>
                        b.CreatedAt.Month == DateTime.Now.Month)
            }
        });
    }
}