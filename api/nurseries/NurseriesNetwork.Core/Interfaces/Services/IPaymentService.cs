using NurseriesNetwork.Core.DTOs.Payment;
using NurseriesNetwork.Core.Enums;

namespace NurseriesNetwork.Core.Interfaces.Services;

public interface IPaymentService
{
    PaymentMethod PaymentMethod { get; }

    // بدل ما يرجع "تم الدفع" فوري، يرجع رابط دفع المستخدم يكمل عليه
    Task<PaymentInitResponseDto> InitiatePaymentAsync(CreatePaymentDto dto);

    Task<bool> RefundAsync(string transactionId);
}