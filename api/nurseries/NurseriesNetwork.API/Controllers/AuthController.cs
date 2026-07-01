using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.DTOs.Auth;
using NurseriesNetwork.Core.Interfaces.Services;

namespace NurseriesNetwork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    // ===========================
    // POST: api/auth/register
    // ===========================
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        // التحقق إن الإيميل مش موجود
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest("الإيميل ده موجود بالفعل");

        // التحقق من الـ Role
        if (dto.Role != "Parent" && dto.Role != "NurseryAdmin")
            return BadRequest("الـ Role غير صحيح");

        // إنشاء المستخدم
        var user = new ApplicationUser
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        // إضافة الـ Role
        await _userManager.AddToRoleAsync(user, dto.Role);

        // بعت إيميل التأكيد
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendConfirmationEmailAsync(user.Email, token);

        return Ok("تم التسجيل بنجاح، تفقد إيميلك لتأكيد الحساب");
    }

    // ===========================
    // POST: api/auth/login
    // ===========================
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        // التحقق من المستخدم
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized("بيانات خاطئة");

        // التحقق إن الإيميل اتأكد
        if (!user.EmailConfirmed)
            return Unauthorized("لازم تأكد إيميلك الأول");

        // التحقق من الباسورد
        var result = await _signInManager.CheckPasswordSignInAsync(
            user, dto.Password, false);
        if (!result.Succeeded)
            return Unauthorized("بيانات خاطئة");

        // توليد الـ Token
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        return Ok(new AuthResponseDto(
            token,
            user.Id,
            user.FullName,
            user.Email!,
            roles.FirstOrDefault() ?? "Parent"
        ));
    }

    // ===========================
    // GET: api/auth/confirm-email
    // ===========================
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string email,
        [FromQuery] string token)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound("المستخدم مش موجود");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return BadRequest("الكود غير صحيح أو انتهت صلاحيته");

        return Ok("تم تأكيد الإيميل بنجاح، يمكنك تسجيل الدخول الآن");
    }

    // ===========================
    // POST: api/auth/resend-email
    // ===========================
    [HttpPost("resend-email")]
    public async Task<IActionResult> ResendConfirmationEmail(
        [FromBody] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound("المستخدم مش موجود");

        if (user.EmailConfirmed)
            return BadRequest("الإيميل اتأكد بالفعل");

        var token = await _userManager
            .GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendConfirmationEmailAsync(user.Email!, token);

        return Ok("تم إرسال الإيميل تاني");
    }
}