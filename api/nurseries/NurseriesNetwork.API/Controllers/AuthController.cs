using Microsoft.AspNetCore.Mvc;
using NurseriesNetwork.Core.DTOs;
using NurseriesNetwork.Core.DTOs.Auth;
using NurseriesNetwork.Core.Interfaces.Services;

namespace NurseriesNetwork.API.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync(RegisterDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);
            if (!response.IsSuccess)
            {
                return BadRequest(response.Errors);
            }
            return Ok(response);
        }


        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            if (!response.IsSuccess)
                return Unauthorized(response.Errors);

            return Ok(response);
        }


        [HttpPost("resend-OTP")]
        public async Task<IActionResult> ResendOtpAsync([FromBody]string email)
        {
            var isSent = await _authService.SendOtpAsync(email);
            if (!isSent)
                return BadRequest("Email not found");

            return Ok(new { Message = "A new OTP has been sent to your email." });
        }


        [HttpPost("verify-OTP")]
        public async Task<IActionResult> VerifyOtpAsync(VerifyOtpDto verifyOtpDto)
        {
            var response = await _authService.VerifyOtpAsync(verifyOtpDto.Email, verifyOtpDto.Otp);
            if (!response.IsSuccess)
            {
                if (response.Errors!.Contains("User not found"))
                    return NotFound(new { Errors = response.Errors });
                
                return BadRequest(new { Errors = response.Errors });
            }

            return Ok(response);
        }
    }
}
