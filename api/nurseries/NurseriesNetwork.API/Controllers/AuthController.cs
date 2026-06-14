using Microsoft.AspNetCore.Mvc;
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
    }
}
