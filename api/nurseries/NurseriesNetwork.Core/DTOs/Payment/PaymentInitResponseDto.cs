namespace NurseriesNetwork.Core.DTOs.Payment;

public record PaymentInitResponseDto(
    bool IsSuccess,
    int PaymentId,           // الـ Id بتاع الـ Payment في DB بتاعنا
    string? PaymentUrl,      // رابط الدفع (Paymob Iframe / PayPal Approval Link)
    string? GatewayOrderId,  // رقم الطلب عند الـ Gateway
    string Message
);