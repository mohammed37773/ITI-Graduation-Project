using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NurseriesNetwork.Core.DTOs.Payment;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Services;

namespace NurseriesNetwork.Infrastructure.Services.Payment;

public class PaymobService : IPaymentService, IPaymentGatewayService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymobService> _logger;

    public PaymentMethod PaymentMethod => PaymentMethod.Card;

    public PaymobService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<PaymobService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    // ===========================
    // Step 1: Get Auth Token
    // ===========================
    private async Task<string> GetAuthTokenAsync()
    {
        _logger.LogInformation("Paymob: Getting auth token...");

        var apiKey = _config["Paymob:ApiKey"];
        var url = $"{_config["Paymob:BaseUrl"]}/intention/";

        // الـ New Paymob API بيستخدم Secret Key مباشرة
        // لو Secret Key موجود نستخدمه، لو لأ نستخدم API Key مؤقتاً
        var secretKey = _config["Paymob:SecretKey"];

        if (string.IsNullOrEmpty(secretKey))
        {
            _logger.LogWarning("Paymob: Secret Key not set yet, using API Key");
            return apiKey ?? throw new Exception("Paymob API Key is missing");
        }

        return secretKey;
    }

    // ===========================
    // Step 2: Create Payment Intention
    // ===========================
    public async Task<PaymentInitResponseDto> InitiatePaymentAsync(
        CreatePaymentDto dto)
    {
        try
        {
            _logger.LogInformation(
                "Paymob: Initiating payment for BookingId: {BookingId}, Amount: {Amount}",
                dto.BookingId, dto.Amount);

            var secretKey = await GetAuthTokenAsync();

            // تحديد الـ Integration ID حسب طريقة الدفع
            var integrationId = dto.Method switch
            {
                PaymentMethod.Card or PaymentMethod.Meeza =>
                    _config["Paymob:CardIntegrationId"],
                PaymentMethod.VodafoneCash =>
                    _config["Paymob:MobileWalletIntegrationId"],
                _ => throw new ArgumentException("طريقة دفع غير مدعومة")
            };

            // المبلغ بالقروش (Paymob بيستخدم أصغر وحدة)
            var amountCents = (int)(dto.Amount * 100);

            var requestBody = new
            {
                amount = amountCents,
                currency = "EGP",
                payment_methods = new[] { int.Parse(integrationId!) },
                items = new[]
                {
                    new
                    {
                        name = "Nursery Booking",
                        amount = amountCents,
                        description = $"Booking ID: {dto.BookingId}",
                        quantity = 1
                    }
                },
                billing_data = new
                {
                    first_name = "Customer",
                    last_name = "Name",
                    email = "customer@example.com",
                    phone_number = "01000000000"
                },
                metadata = new
                {
                    booking_id = dto.BookingId
                },
                redirection_url = "http://localhost:4200/payment/callback"
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "Authorization", $"Token {secretKey}");

            var response = await _httpClient.PostAsJsonAsync(
                $"{_config["Paymob:BaseUrl"]}/intention/", requestBody);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Paymob: Failed to create intention. Response: {Content}", content);
                return new PaymentInitResponseDto(
                    false, 0, null, null,
                    "فشل إنشاء طلب الدفع، حاول تاني");
            }

            using var doc = JsonDocument.Parse(content);
            var clientSecret = doc.RootElement
                .GetProperty("client_secret")
                .GetString();

            var iframeId = _config["Paymob:IframeId"];
            var paymentUrl = string.IsNullOrEmpty(iframeId)
                ? $"https://accept.paymob.com/unifiedcheckout/?publicKey={_config["Paymob:ApiKey"]}&clientSecret={clientSecret}"
                : $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={clientSecret}";

            _logger.LogInformation(
                "Paymob: Payment intention created successfully");

            return new PaymentInitResponseDto(
                true, 0, paymentUrl, clientSecret,
                "تم إنشاء طلب الدفع بنجاح");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paymob: Exception during payment initiation");
            return new PaymentInitResponseDto(
                false, 0, null, null, "حصل خطأ غير متوقع");
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
                "Paymob: Refunding transaction: {TransactionId}", transactionId);

            var secretKey = _config["Paymob:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogWarning("Paymob: Secret Key not set, cannot refund");
                return false;
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "Authorization", $"Token {secretKey}");

            var requestBody = new
            {
                transaction_id = int.Parse(transactionId)
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_config["Paymob:BaseUrl"]}/refund/", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Paymob: Refund failed for transaction: {TransactionId}",
                    transactionId);
                return false;
            }

            _logger.LogInformation(
                "Paymob: Refund successful for transaction: {TransactionId}",
                transactionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paymob: Exception during refund");
            return false;
        }
    }

    // ===========================
    // HMAC Signature Verification
    // ===========================
    public bool VerifySignature(string payload, string receivedHmac)
    {
        try
        {
            var hmacSecret = _config["Paymob:HmacSecret"]!;
            var keyBytes = Encoding.UTF8.GetBytes(hmacSecret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA512(keyBytes);
            var computedHash = hmac.ComputeHash(payloadBytes);
            var computedHmac = BitConverter.ToString(computedHash)
                .Replace("-", "").ToLower();

            var isValid = computedHmac == receivedHmac.ToLower();

            if (!isValid)
                _logger.LogWarning(
                    "Paymob: HMAC verification failed. " +
                    "Computed: {Computed}, Received: {Received}",
                    computedHmac, receivedHmac);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paymob: Exception during HMAC verification");
            return false;
        }
    }
}