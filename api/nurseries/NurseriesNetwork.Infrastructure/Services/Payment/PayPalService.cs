using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NurseriesNetwork.Core.DTOs.Payment;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Services;

namespace NurseriesNetwork.Infrastructure.Services.Payment;

public class PayPalService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<PayPalService> _logger;

    public PaymentMethod PaymentMethod => PaymentMethod.PayPal;

    public PayPalService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<PayPalService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    // ===========================
    // Step 1: Get Access Token
    // ===========================
    private async Task<string> GetAccessTokenAsync()
    {
        var clientId = _config["PayPal:ClientId"]!;
        var clientSecret = _config["PayPal:ClientSecret"]!;
        var baseUrl = _config["PayPal:BaseUrl"]!;

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add(
            "Authorization", $"Basic {credentials}");

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>(
                "grant_type", "client_credentials")
        });

        var response = await _httpClient.PostAsync(
            $"{baseUrl}/v1/oauth2/token", content);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement
            .GetProperty("access_token")
            .GetString()!;
    }

    // ===========================
    // Step 2: Create PayPal Order
    // ===========================
    public async Task<PaymentInitResponseDto> InitiatePaymentAsync(
        CreatePaymentDto dto)
    {
        try
        {
            _logger.LogInformation(
                "PayPal: Creating order for BookingId: {BookingId}",
                dto.BookingId);

            var accessToken = await GetAccessTokenAsync();
            var baseUrl = _config["PayPal:BaseUrl"]!;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "Authorization", $"Bearer {accessToken}");

            var requestBody = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = dto.BookingId.ToString(),
                        amount = new
                        {
                            currency_code = "USD",
                            value = dto.Amount.ToString("F2")
                        },
                        description = $"Nursery Booking #{dto.BookingId}"
                    }
                },
                application_context = new
                {
                    return_url = "http://localhost:4200/payment/paypal-success",
                    cancel_url = "http://localhost:4200/payment/paypal-cancel"
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{baseUrl}/v2/checkout/orders", requestBody);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "PayPal: Failed to create order. Response: {Content}", content);
                return new PaymentInitResponseDto(
                    false, 0, null, null,
                    "فشل إنشاء طلب PayPal");
            }

            using var doc = JsonDocument.Parse(content);
            var orderId = doc.RootElement.GetProperty("id").GetString();

            // استخرج الـ Approval URL
            var approvalUrl = doc.RootElement
                .GetProperty("links")
                .EnumerateArray()
                .FirstOrDefault(l =>
                    l.GetProperty("rel").GetString() == "approve")
                .GetProperty("href")
                .GetString();

            _logger.LogInformation(
                "PayPal: Order created successfully. OrderId: {OrderId}", orderId);

            return new PaymentInitResponseDto(
                true, 0, approvalUrl, orderId,
                "تم إنشاء طلب PayPal بنجاح");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayPal: Exception during order creation");
            return new PaymentInitResponseDto(
                false, 0, null, null, "حصل خطأ غير متوقع");
        }
    }

    // ===========================
    // Step 3: Capture Payment
    // ===========================
    public async Task<bool> CapturePaymentAsync(string paypalOrderId)
    {
        try
        {
            _logger.LogInformation(
                "PayPal: Capturing payment for OrderId: {OrderId}", paypalOrderId);

            var accessToken = await GetAccessTokenAsync();
            var baseUrl = _config["PayPal:BaseUrl"]!;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "Authorization", $"Bearer {accessToken}");
            // ❌ شيل السطر ده:
            // _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

            // ✅ الـ Content-Type بقى متحدد هنا تلقائياً
            var requestContent = new StringContent(
                "{}", Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{baseUrl}/v2/checkout/orders/{paypalOrderId}/capture",
                requestContent);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "PayPal: Capture failed for OrderId: {OrderId}. " +
                    "StatusCode: {StatusCode}. Response: {Content}",
                    paypalOrderId, response.StatusCode, content);
                return false;
            }

            _logger.LogInformation(
                "PayPal: Payment captured successfully for OrderId: {OrderId}",
                paypalOrderId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayPal: Exception during capture");
            return false;
        }
    }
    // ===========================
    // Refund
    // ===========================
    public async Task<bool> RefundAsync(string transactionId)
    {
        try
        {
            _logger.LogInformation(
                "PayPal: Refunding transaction: {TransactionId}", transactionId);

            var accessToken = await GetAccessTokenAsync();
            var baseUrl = _config["PayPal:BaseUrl"]!;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.PostAsync(
                $"{baseUrl}/v2/payments/captures/{transactionId}/refund",
                new StringContent("{}", Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "PayPal: Refund failed for transaction: {TransactionId}",
                    transactionId);
                return false;
            }

            _logger.LogInformation(
                "PayPal: Refund successful for transaction: {TransactionId}",
                transactionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayPal: Exception during refund");
            return false;
        }
    }
}