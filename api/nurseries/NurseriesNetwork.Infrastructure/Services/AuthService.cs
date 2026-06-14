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


        public AuthService(UserManager<ApplicationUser> userManager,
                           JwtProvider jwtProvider)
        {
            _userManager = userManager;
            _jwtProvider = jwtProvider;
         
        }


        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var appUser = MappRegisterDtoToAppUser(registerDto);
            var identityResult = await _userManager.CreateAsync(appUser, registerDto.Password);
            if (!identityResult.Succeeded)
                return new AuthResponseDto { IsSuccess = false, Errors = identityResult.Errors.Select(e => e.Description) };
            await _userManager.AddToRoleAsync(appUser, AppRoles.Parent);

            return new AuthResponseDto { IsSuccess = true };

        }



        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var appUser = await _userManager.FindByEmailAsync(loginDto.Email);
            if (appUser is null || !await _userManager.CheckPasswordAsync(appUser, loginDto.Password))
                return new AuthResponseDto { IsSuccess = false, Errors = new List<string> { "Invalid Authentication" } };

            var roles = await _userManager.GetRolesAsync(appUser);

            var token = _jwtProvider.CreateToken(appUser, roles);
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
