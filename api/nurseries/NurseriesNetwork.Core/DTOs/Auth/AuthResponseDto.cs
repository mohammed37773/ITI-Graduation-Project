using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Auth
{
    public record AuthResponseDto(
    string Token,
    string FullName,
    string Email,
    string Role
);
}
