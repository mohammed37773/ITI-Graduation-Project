using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Auth
{
    public record ConfirmEmailDto(
    [Required][EmailAddress] string Email,
    [Required] string Token
);
}
