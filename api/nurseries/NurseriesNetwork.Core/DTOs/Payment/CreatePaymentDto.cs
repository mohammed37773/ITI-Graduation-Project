using System.ComponentModel.DataAnnotations;
using NurseriesNetwork.Core.Enums;

namespace NurseriesNetwork.Core.DTOs.Payment;

public record CreatePaymentDto(
    [Required] int BookingId,
    [Required][Range(1, 100000)] decimal Amount,
    [Required] PaymentMethod Method
);