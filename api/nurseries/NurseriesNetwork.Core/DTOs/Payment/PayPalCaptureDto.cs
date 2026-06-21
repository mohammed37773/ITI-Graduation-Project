namespace NurseriesNetwork.Core.DTOs.Payment;

// بيستقبله الـ Frontend بعد ما المستخدم يوافق على الدفع في PayPal
// وبيبعته للـ Backend عشان يأكد العملية (Capture)
public record PayPalCaptureDto(
    int PaymentId,
    string PayPalOrderId
);