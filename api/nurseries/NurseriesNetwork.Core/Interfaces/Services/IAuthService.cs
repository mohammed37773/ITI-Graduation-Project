using NurseriesNetwork.Core.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

        Task<bool> SendOtpAsync(string email);
        Task<AuthResponseDto> VerifyOtpAsync(string email, string otpCode);
    }
}
