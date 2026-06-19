using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseriesNetwork.Core.DTOs.AI;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Core.Interfaces.Services;
using System.Security.Claims;

namespace NurseriesNetwork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly IUnitOfWork _uow;

    public AiController(
        IAiService aiService,
        IUnitOfWork uow)
    {
        _aiService = aiService;
        _uow = uow;
    }

    // ===========================
    // POST: api/ai/chat
    // ===========================
    [HttpPost("chat")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> Chat(ChatRequestDto dto)
    {
        var response = await _aiService.GetRecommendationAsync(
            dto.Message,
            dto.Latitude,
            dto.Longitude);

        return Ok(new { response });
    }

    // ===========================
    // POST: api/ai/recommend
    // ===========================
    [HttpPost("recommend")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> Recommend(
        ChatRequestDto dto)
    {
        // جيب الحضانات القريبة الأول
        var nearbyNurseries = dto.Latitude.HasValue &&
                              dto.Longitude.HasValue
            ? await _uow.Nurseries.GetNearbyAsync(
                dto.Latitude.Value,
                dto.Longitude.Value,
                radiusKm: 10)
            : await _uow.Nurseries.GetAllAsync();

        // لو مفيش حضانات
        if (!nearbyNurseries.Any())
            return Ok(new
            {
                response = "مفيش حضانات قريبة منك دلوقتي"
            });

        // بعت للـ AI مع context الحضانات
        var response = await _aiService.GetRecommendationAsync(
            dto.Message,
            dto.Latitude,
            dto.Longitude);

        return Ok(new
        {
            response,
            nurseries = nearbyNurseries.Take(5).Select(n => new
            {
                n.Id,
                n.Name,
                n.DailyPrice,
                n.AvgRating,
                City = n.Location?.City,
                Address = n.Location?.Address
            })
        });
    }
}