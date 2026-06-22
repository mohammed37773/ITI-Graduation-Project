using NurseriesNetwork.Core.DTOs.Payment;
using NurseriesNetwork.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Interfaces.Services
{
    public interface IPaymentService
    {
        PaymentMethod PaymentMethod { get; }
        Task<PaymentResponseDto> ProcessPaymentAsync(CreatePaymentDto dto);
        Task<bool> RefundAsync(string transactionId);
    }
}
