using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Core.Interfaces.Services;
using NurseriesNetwork.Infrastructure.Services.Payment;
using Microsoft.Extensions.Logging;

namespace NurseriesNetwork.API.Controllers.Webhooks;

[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
public class PaymobWebhookController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly PaymobService _paymobService;
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymobWebhookController> _logger;

    public PaymobWebhookController(
        IUnitOfWork uow,
        PaymobService paymobService,
        IEmailService emailService,
        ILogger<PaymobWebhookController> logger)
    {
        _uow = uow;
        _paymobService = paymobService;
        _emailService = emailService;
        _logger = logger;
    }

    // ===========================
    // POST: api/webhooks/paymob
    // ===========================
    [HttpPost("paymob")]
    public async Task<IActionResult> HandlePaymobWebhook()
    {
        try
        {
            // اقرأ الـ Request Body كاملاً
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            _logger.LogInformation(
                "Paymob Webhook received: {Payload}", payload);

            // استخرج الـ HMAC من الـ Query String
            var hmac = Request.Query["hmac"].ToString();

            if (string.IsNullOrEmpty(hmac))
            {
                _logger.LogWarning("Paymob Webhook: No HMAC provided");
                return BadRequest("No HMAC");
            }

            // تحقق من الـ HMAC
            // Paymob بيحسب HMAC على قيم محددة بترتيب معين
            using var doc = JsonDocument.Parse(payload);
            var obj = doc.RootElement.GetProperty("obj");

            var hmacString = BuildPaymobHmacString(obj);

            if (!_paymobService.VerifySignature(hmacString, hmac))
            {
                _logger.LogWarning("Paymob Webhook: Invalid HMAC signature");
                return Unauthorized("Invalid HMAC");
            }

            // استخرج بيانات الـ Transaction
            var success = obj.GetProperty("success").GetBoolean();
            var transactionId = obj.GetProperty("id").GetInt64().ToString();
            var amountCents = obj.GetProperty("amount_cents").GetInt64();

            // استخرج الـ Booking ID من الـ metadata
            var metadata = obj
                .GetProperty("order")
                .GetProperty("merchant_order_id")
                .GetString();

            if (!int.TryParse(metadata, out var bookingId))
            {
                _logger.LogWarning(
                    "Paymob Webhook: Invalid booking ID in metadata");
                return Ok(); // نرجع OK عشان Paymob ما يعيدش الإرسال
            }

            // جيب الـ Payment من DB
            var payments = await _uow.Payments
                .FindAsync(p => p.BookingId == bookingId &&
                           p.Status == PaymentStatus.Pending);

            var payment = payments.FirstOrDefault();
            if (payment == null)
            {
                _logger.LogWarning(
                    "Paymob Webhook: No pending payment found for BookingId: {BookingId}",
                    bookingId);
                return Ok();
            }

            if (success)
            {
                // الدفع نجح — حدّث الـ Payment والـ Booking
                payment.Status = PaymentStatus.Completed;
                payment.TransactionId = transactionId;
                payment.PaidAt = DateTime.UtcNow;
                _uow.Payments.Update(payment);

                var booking = await _uow.Bookings.GetByIdAsync(bookingId);
                if (booking != null)
                {
                    booking.Status = BookingStatus.Confirmed;
                    _uow.Bookings.Update(booking);
                }

                await _uow.SaveChangesAsync();

                // بعت إيميل تأكيد
                var parent = await _uow.Bookings.GetByIdAsync(bookingId);
                // بنجيب الإيميل من الـ User
                await _emailService.SendPaymentConfirmationAsync(
                    payment.ParentId, payment.Id);

                _logger.LogInformation(
                    "Paymob Webhook: Payment completed for BookingId: {BookingId}",
                    bookingId);
            }
            else
            {
                // الدفع فشل
                payment.Status = PaymentStatus.Failed;
                _uow.Payments.Update(payment);
                await _uow.SaveChangesAsync();

                _logger.LogWarning(
                    "Paymob Webhook: Payment failed for BookingId: {BookingId}",
                    bookingId);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paymob Webhook: Unhandled exception");
            return Ok(); // دايماً نرجع OK عشان Paymob ما يعيدش الإرسال
        }
    }

    // ===========================
    // بناء الـ HMAC String بالترتيب الصحيح
    // ===========================
    private static string BuildPaymobHmacString(JsonElement obj)
    {
        // ترتيب الحقول ده مهم جداً ومحدد من Paymob
        var fields = new[]
        {
            obj.GetProperty("amount_cents").ToString(),
            obj.GetProperty("created_at").GetString() ?? "",
            obj.GetProperty("currency").GetString() ?? "",
            obj.GetProperty("error_occured").GetBoolean().ToString().ToLower(),
            obj.GetProperty("has_parent_transaction").GetBoolean().ToString().ToLower(),
            obj.GetProperty("id").ToString(),
            obj.GetProperty("integration_id").ToString(),
            obj.GetProperty("is_3d_secure").GetBoolean().ToString().ToLower(),
            obj.GetProperty("is_auth").GetBoolean().ToString().ToLower(),
            obj.GetProperty("is_capture").GetBoolean().ToString().ToLower(),
            obj.GetProperty("is_refunded").GetBoolean().ToString().ToLower(),
            obj.GetProperty("is_standalone_payment").GetBoolean().ToString().ToLower(),
            obj.GetProperty("is_voided").GetBoolean().ToString().ToLower(),
            obj.GetProperty("order").GetProperty("id").ToString(),
            obj.GetProperty("owner").ToString(),
            obj.GetProperty("pending").GetBoolean().ToString().ToLower(),
            obj.GetProperty("source_data").GetProperty("pan").GetString() ?? "",
            obj.GetProperty("source_data").GetProperty("sub_type").GetString() ?? "",
            obj.GetProperty("source_data").GetProperty("type").GetString() ?? "",
            obj.GetProperty("success").GetBoolean().ToString().ToLower()
        };

        return string.Concat(fields);
    }
}