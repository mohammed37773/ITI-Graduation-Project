using Microsoft.AspNetCore.Identity;
using NurseriesNetwork.Core.DTOs.Auth;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces.Services;
using NurseriesNetwork.Infrastructure.Consts;

namespace NurseriesNetwork.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtProvider _jwtProvider;
        private readonly IEmailService _emailService;


        public AuthService(UserManager<ApplicationUser> userManager,
                           JwtProvider jwtProvider,
                           IEmailService emailService)
        {
            _userManager = userManager;
            _jwtProvider = jwtProvider;
            _emailService = emailService;
        }


        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var appUser = MappRegisterDtoToAppUser(registerDto);
            var identityResult = await _userManager.CreateAsync(appUser, registerDto.Password);
            if (!identityResult.Succeeded)
                return new AuthResponseDto { IsSuccess = false, Errors = identityResult.Errors.Select(e => e.Description) };

            await _userManager.AddToRoleAsync(appUser, AppRoles.Parent);

            var otpSent = await SendOtpAsync(appUser.Email!);
            if (!otpSent)
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new[] { "User registered, but failed to send OTP." } };
            }

            return new AuthResponseDto { IsSuccess = true };

        }



        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var appUser = await _userManager.FindByEmailAsync(loginDto.Email);
            if (appUser is null || !await _userManager.CheckPasswordAsync(appUser, loginDto.Password))
                return new AuthResponseDto { IsSuccess = false, Errors = new List<string> { "Invalid Authentication" } };

            if (!appUser.EmailConfirmed)
            {
                return new AuthResponseDto 
                { 
                    IsSuccess = false, 
                    Errors = new List<string> { "Please confirm your email first by entering the OTP." } 
                };
            }

            var roles = await _userManager.GetRolesAsync(appUser);

            var token = _jwtProvider.CreateToken(appUser, roles);
            return new AuthResponseDto { IsSuccess = true, Token = token };

        }


        public async Task<bool> SendOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return false;

            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();

            user.Otp = otpCode;
            user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);

            await _userManager.UpdateAsync(user);

            await _emailService.SendOtpEmailAsync(user.Email!, otpCode);

            return true;
        }


        public async Task<AuthResponseDto> VerifyOtpAsync(string email, string otpCode)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return new AuthResponseDto { IsSuccess = false, Errors = new[] { "User not found" } };

            if (user.Otp != otpCode || user.OtpExpiryTime < DateTime.UtcNow)
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new[] { "Invalid or expired OTP code" } };
            }

            user.Otp = null;
            user.OtpExpiryTime = null;
            user.EmailConfirmed = true; 
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtProvider.CreateToken(user, roles);

            return new AuthResponseDto { IsSuccess = true, Token = token };
        }


        private ApplicationUser MappRegisterDtoToAppUser(RegisterDto registerDto)
        {
            return new ApplicationUser
            {
                FullName = registerDto.FullName,
                UserName = registerDto.Email,
                Email = registerDto.Email,
            };
        }


    }
}
