using System.ComponentModel.DataAnnotations;
using NurseriesNetwork.Core.Enums;

namespace NurseriesNetwork.Core.DTOs.Payment;

public class CreatePaymentDto
{
    [Required]
    public int BookingId { get; set; }

    [Required]
    [Range(1, 100000)]
    public decimal Amount { get; set; }

    [Required]
    public PaymentMethod Method { get; set; }
}

