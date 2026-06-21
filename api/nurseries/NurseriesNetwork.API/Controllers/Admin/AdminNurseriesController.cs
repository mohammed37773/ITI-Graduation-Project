using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseriesNetwork.Core.Interfaces.Repositories;

namespace NurseriesNetwork.API.Controllers.Admin;

[ApiController]
[Route("api/admin/nurseries")]
[Authorize(Roles = "Admin")]
public class AdminNurseriesController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public AdminNurseriesController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // ===========================
    // GET: api/admin/nurseries
    // ===========================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var nurseries = await _uow.Nurseries.GetAllAsync();
        return Ok(nurseries);
    }

    // ===========================
    // PUT: api/admin/nurseries/{id}/verify
    // ===========================
    [HttpPut("{id}/verify")]
    public async Task<IActionResult> VerifyNursery(int id)
    {
        var nursery = await _uow.Nurseries.GetByIdAsync(id);
        if (nursery == null)
            return NotFound("الحضانة مش موجودة");

        nursery.IsVerified = true;
        _uow.Nurseries.Update(nursery);
        await _uow.SaveChangesAsync();

        return Ok("تم التحقق من الحضانة");
    }

    // ===========================
    // DELETE: api/admin/nurseries/{id}
    // ===========================
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNursery(int id)
    {
        var nursery = await _uow.Nurseries.GetByIdAsync(id);
        if (nursery == null)
            return NotFound("الحضانة مش موجودة");

        _uow.Nurseries.Delete(nursery);
        await _uow.SaveChangesAsync();

        return Ok("تم حذف الحضانة");
    }

    // ===========================
    // GET: api/admin/nurseries/stats
    // ===========================
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var all = await _uow.Nurseries.GetAllAsync();

        return Ok(new
        {
            Total = all.Count(),
            Verified = all.Count(n => n.IsVerified),
            Pending = all.Count(n => !n.IsVerified),
            AvgPrice = all.Average(n => n.DailyPrice),
            AvgRating = all.Average(n => n.AvgRating)
        });
    }
}