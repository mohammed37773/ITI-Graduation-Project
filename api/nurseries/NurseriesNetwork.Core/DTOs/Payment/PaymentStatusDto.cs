using NurseriesNetwork.Core.Enums;

namespace NurseriesNetwork.Core.DTOs.Payment;

public record PaymentStatusDto(
    int PaymentId,
    PaymentStatus Status,
    decimal Amount,
    string Method,
    DateTime? PaidAt
);