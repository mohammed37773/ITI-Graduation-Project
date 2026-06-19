using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseriesNetwork.Core.DTOs.Child;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Repositories;
using System.Security.Claims;

namespace NurseriesNetwork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Parent")]
public class ChildrenController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public ChildrenController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // ===========================
    // GET: api/children
    // ===========================
    [HttpGet]
    public async Task<IActionResult> GetMyChildren()
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var children = await _uow.Children
            .FindAsync(c => c.ParentId == parentId);

        return Ok(children);
    }

    // ===========================
    // GET: api/children/{id}
    // ===========================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var child = await _uow.Children.GetByIdAsync(id);
        if (child == null)
            return NotFound("الطفل مش موجود");

        if (child.ParentId != parentId)
            return Forbid();

        return Ok(child);
    }

    // ===========================
    // POST: api/children
    // ===========================
    [HttpPost]
    public async Task<IActionResult> AddChild(CreateChildDto dto)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var child = new Child
        {
            ParentId = parentId,
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
            SpecialNeeds = dto.SpecialNeeds
        };

        await _uow.Children.AddAsync(child);
        await _uow.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById),
            new { id = child.Id }, child);
    }

    // ===========================
    // PUT: api/children/{id}
    // ===========================
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateChild(
        int id, CreateChildDto dto)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var child = await _uow.Children.GetByIdAsync(id);
        if (child == null)
            return NotFound("الطفل مش موجود");

        if (child.ParentId != parentId)
            return Forbid();

        child.FullName = dto.FullName;
        child.DateOfBirth = dto.DateOfBirth;
        child.SpecialNeeds = dto.SpecialNeeds;

        _uow.Children.Update(child);
        await _uow.SaveChangesAsync();

        return Ok(child);
    }

    // ===========================
    // DELETE: api/children/{id}
    // ===========================
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChild(int id)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var child = await _uow.Children.GetByIdAsync(id);
        if (child == null)
            return NotFound("الطفل مش موجود");

        if (child.ParentId != parentId)
            return Forbid();

        // التحقق إن مفيش حجز نشط للطفل
        var activeBookings = await _uow.Bookings.FindAsync(b =>
            b.ChildId == id &&
            b.Status != BookingStatus.Cancelled);

        if (activeBookings.Any())
            return BadRequest("مش ممكن تحذف طفل عنده حجز نشط");

        _uow.Children.Delete(child);
        await _uow.SaveChangesAsync();

        return Ok("تم حذف الطفل");
    }
}