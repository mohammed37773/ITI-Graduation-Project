using NurseriesNetwork.Core.Enums;

namespace NurseriesNetwork.Core.Entities;

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string ParentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? TransactionId { get; set; }
    public string? GatewayOrderId { get; set; }   // ← جديد: رقم الطلب عند Paymob/PayPal
    public string? PaymentUrl { get; set; }        // ← جديد: رابط صفحة الدفع للمستخدم
    public DateTime? PaidAt { get; set; }           // ← جديد: تاريخ الدفع الفعلي

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Booking Booking { get; set; } = null!;
    public ApplicationUser Parent { get; set; } = null!;
}