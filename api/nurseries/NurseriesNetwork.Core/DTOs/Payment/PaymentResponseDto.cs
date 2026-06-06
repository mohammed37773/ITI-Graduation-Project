using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Payment
{
    public record PaymentResponseDto(
     bool IsSuccess,
     string TransactionId,
     string Message,
     decimal Amount,
     string Method
 );
}
