using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseriesNetwork.Core.Entities;

namespace NurseriesNetwork.API.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUsersController(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // ===========================
    // GET: api/admin/users
    // ===========================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManager.Users.ToListAsync();

        var result = new List<object>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.EmailConfirmed,
                user.LockoutEnabled,
                Roles = roles
            });
        }

        return Ok(result);
    }

    // ===========================
    // GET: api/admin/users/{id}
    // ===========================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("المستخدم مش موجود");

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.EmailConfirmed,
            user.LockoutEnabled,
            Roles = roles
        });
    }

    // ===========================
    // PUT: api/admin/users/{id}/ban
    // ===========================
    [HttpPut("{id}/ban")]
    public async Task<IActionResult> BanUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("المستخدم مش موجود");

        // منع المستخدم من الدخول
        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(
            user, DateTimeOffset.MaxValue);

        return Ok("تم حظر المستخدم");
    }

    // ===========================
    // PUT: api/admin/users/{id}/unban
    // ===========================
    [HttpPut("{id}/unban")]
    public async Task<IActionResult> UnbanUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("المستخدم مش موجود");

        await _userManager.SetLockoutEndDateAsync(user, null);

        return Ok("تم رفع الحظر عن المستخدم");
    }

    // ===========================
    // GET: api/admin/users/stats
    // ===========================
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var users = await _userManager.Users.ToListAsync();

        var parents = await _userManager
            .GetUsersInRoleAsync("Parent");
        var admins = await _userManager
            .GetUsersInRoleAsync("NurseryAdmin");

        return Ok(new
        {
            TotalUsers = users.Count,
            Parents = parents.Count,
            NurseryAdmins = admins.Count,
            Banned = users.Count(u => u.LockoutEnd != null &&
                u.LockoutEnd > DateTimeOffset.UtcNow)
        });
    }
}