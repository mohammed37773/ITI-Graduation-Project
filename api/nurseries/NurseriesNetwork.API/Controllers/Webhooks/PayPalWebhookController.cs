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
public class PayPalWebhookController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly PayPalService _payPalService;
    private readonly IEmailService _emailService;
    private readonly ILogger<PayPalWebhookController> _logger;

    public PayPalWebhookController(
        IUnitOfWork uow,
        PayPalService payPalService,
        IEmailService emailService,
        ILogger<PayPalWebhookController> logger)
    {
        _uow = uow;
        _payPalService = payPalService;
        _emailService = emailService;
        _logger = logger;
    }

    // ===========================
    // POST: api/webhooks/paypal
    // ===========================
    [HttpPost("paypal")]
    public async Task<IActionResult> HandlePayPalWebhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            _logger.LogInformation(
                "PayPal Webhook received: {Payload}", payload);

            using var doc = JsonDocument.Parse(payload);
            var eventType = doc.RootElement
                .GetProperty("event_type").GetString();

            _logger.LogInformation(
                "PayPal Webhook event type: {EventType}", eventType);

            // نتعامل مع حدث "الدفع اكتمل"
            if (eventType == "PAYMENT.CAPTURE.COMPLETED")
            {
                var resource = doc.RootElement.GetProperty("resource");
                var captureId = resource.GetProperty("id").GetString();

                // استخرج الـ Booking ID من الـ custom_id
                var customId = resource
                    .GetProperty("supplementary_data")
                    .GetProperty("related_ids")
                    .GetProperty("order_id")
                    .GetString();

                // جيب الـ Payment من DB عن طريق الـ GatewayOrderId
                var payments = await _uow.Payments
                    .FindAsync(p => p.GatewayOrderId == customId &&
                               p.Status == PaymentStatus.Pending);

                var payment = payments.FirstOrDefault();
                if (payment == null)
                {
                    _logger.LogWarning(
                        "PayPal Webhook: No pending payment found for OrderId: {OrderId}",
                        customId);
                    return Ok();
                }

                // حدّث الـ Payment والـ Booking
                payment.Status = PaymentStatus.Completed;
                payment.TransactionId = captureId;
                payment.PaidAt = DateTime.UtcNow;
                _uow.Payments.Update(payment);

                var booking = await _uow.Bookings
                    .GetByIdAsync(payment.BookingId);
                if (booking != null)
                {
                    booking.Status = BookingStatus.Confirmed;
                    _uow.Bookings.Update(booking);
                }

                await _uow.SaveChangesAsync();

                await _emailService.SendPaymentConfirmationAsync(
                    payment.ParentId, payment.Id);

                _logger.LogInformation(
                    "PayPal Webhook: Payment completed for PaymentId: {PaymentId}",
                    payment.Id);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayPal Webhook: Unhandled exception");
            return Ok();
        }
    }
}