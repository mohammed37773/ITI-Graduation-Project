using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Auth
{
    public record RegisterDto(
    string FullName,
    string Email,
    string Password,
    string Role = "Parent"
);
}
