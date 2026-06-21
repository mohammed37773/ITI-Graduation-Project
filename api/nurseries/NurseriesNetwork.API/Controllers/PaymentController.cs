using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NurseriesNetwork.Core.DTOs.Payment;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Core.Interfaces.Services;
using NurseriesNetwork.Infrastructure.Services.Payment;
using Microsoft.Extensions.Logging;

namespace NurseriesNetwork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IPaymentFactory _paymentFactory;
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IUnitOfWork uow,
        IPaymentFactory paymentFactory,
        IEmailService emailService,
        ILogger<PaymentController> logger)
    {
        _uow = uow;
        _paymentFactory = paymentFactory;
        _emailService = emailService;
        _logger = logger;
    }

    // ===========================
    // POST: api/payment/initiate
    // ===========================
    [HttpPost("initiate")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> InitiatePayment(CreatePaymentDto dto)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        _logger.LogInformation(
            "Payment initiation request for BookingId: {BookingId}, Method: {Method}",
            dto.BookingId, dto.Method);

        // التحقق من الحجز
        var booking = await _uow.Bookings.GetByIdAsync(dto.BookingId);
        if (booking == null)
            return NotFound("الحجز مش موجود");

        if (booking.ParentId != parentId)
            return Forbid();

        if (booking.Status == BookingStatus.Cancelled)
            return BadRequest("مش ممكن تدفع على حجز ملغي");

        // التحقق إن مفيش دفعة ناجحة قبل كده
        var existingPayments = await _uow.Payments
            .FindAsync(p => p.BookingId == dto.BookingId);

        if (existingPayments.Any(p => p.Status == PaymentStatus.Completed))
            return BadRequest("الحجز ده اتدفع بالفعل");

        // اختيار الـ Payment Service
        var paymentService = _paymentFactory.GetPaymentService(dto.Method);

        // إنشاء الدفع
        var result = await paymentService.InitiatePaymentAsync(dto);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Payment initiation failed for BookingId: {BookingId}", dto.BookingId);
            return BadRequest(result.Message);
        }

        // حفظ الـ Payment في DB بحالة Pending
        var payment = new Payment
        {
            BookingId = dto.BookingId,
            ParentId = parentId,
            Amount = dto.Amount,
            Method = dto.Method,
            Status = PaymentStatus.Pending,
            GatewayOrderId = result.GatewayOrderId,
            PaymentUrl = result.PaymentUrl
        };
        booking.Payment = payment;
        await _uow.Payments.AddAsync(payment);
        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Payment created with Id: {PaymentId} for BookingId: {BookingId}",
            payment.Id, dto.BookingId);

        // رجّع الرابط للـ Frontend
        return Ok(new PaymentInitResponseDto(
            true,
            payment.Id,
            result.PaymentUrl,
            result.GatewayOrderId,
            "تم إنشاء طلب الدفع، اكمل على الصفحة التالية"
        ));
    }

    // ===========================
    // POST: api/payment/paypal/capture
    // ===========================
    [HttpPost("paypal/capture")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> CapturePayPal(PayPalCaptureDto dto)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var payment = await _uow.Payments.GetByIdAsync(dto.PaymentId);
        if (payment == null)
            return NotFound("الدفعة مش موجودة");

        if (payment.ParentId != parentId)
            return Forbid();

        if (payment.Status != PaymentStatus.Pending)
            return BadRequest("الدفعة دي مش في انتظار التأكيد");

        // Capture الدفع من PayPal
        var payPalService = _paymentFactory
            .GetPaymentService(PaymentMethod.PayPal) as PayPalService;

        var captured = await payPalService!
            .CapturePaymentAsync(dto.PayPalOrderId);

        if (!captured)
            return BadRequest("فشل تأكيد الدفع من PayPal");

        // حدّث الـ Payment
        payment.Status = PaymentStatus.Completed;
        payment.TransactionId = dto.PayPalOrderId;
        payment.PaidAt = DateTime.UtcNow;
        _uow.Payments.Update(payment);

        // حدّث الـ Booking
        var booking = await _uow.Bookings.GetByIdAsync(payment.BookingId);
        if (booking != null)
        {
            booking.Status = BookingStatus.Confirmed;
            _uow.Bookings.Update(booking);
        }

        await _uow.SaveChangesAsync();

        await _emailService.SendPaymentConfirmationAsync(
            parentId, payment.Id);

        return Ok("تم تأكيد الدفع بنجاح ✅");
    }

    // ===========================
    // GET: api/payment/{id}/status
    // ===========================
    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetPaymentStatus(int id)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var payment = await _uow.Payments.GetByIdAsync(id);
        if (payment == null)
            return NotFound("الدفعة مش موجودة");

        if (payment.ParentId != parentId && !User.IsInRole("Admin"))
            return Forbid();

        return Ok(new PaymentStatusDto(
            payment.Id,
            payment.Status,
            payment.Amount,
            payment.Method.ToString(),
            payment.PaidAt
        ));
    }

    // ===========================
    // GET: api/payment/my
    // ===========================
    [HttpGet("my")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> GetMyPayments()
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var payments = await _uow.Payments
            .FindAsync(p => p.ParentId == parentId);
        return Ok(payments);
    }

    // ===========================
    // POST: api/payment/{id}/refund
    // ===========================
    [HttpPost("{id}/refund")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> Refund(int id)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var payment = await _uow.Payments.GetByIdAsync(id);
        if (payment == null)
            return NotFound("الدفعة مش موجودة");

        if (payment.ParentId != parentId)
            return Forbid();

        if (payment.Status != PaymentStatus.Completed)
            return BadRequest("مش ممكن ترجع دفعة مش مكتملة");

        var paymentService = _paymentFactory
            .GetPaymentService(payment.Method);

        var refunded = await paymentService
            .RefundAsync(payment.TransactionId!);

        if (!refunded)
            return BadRequest("فشل الاسترداد، حاول تاني");

        payment.Status = PaymentStatus.Refunded;
        _uow.Payments.Update(payment);

        var booking = await _uow.Bookings.GetByIdAsync(payment.BookingId);
        if (booking != null)
        {
            booking.Status = BookingStatus.Cancelled;
            _uow.Bookings.Update(booking);
        }

        await _uow.SaveChangesAsync();

        return Ok("تم استرداد المبلغ بنجاح");
    }
}