using NurseriesNetwork.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Payment
{
    public record CreatePaymentDto(
    [Required] int BookingId,
    [Required][Range(1, 100000)] decimal Amount,
    [Required] PaymentMethod Method,
    [Required] string PhoneOrAccountNumber
);
}
