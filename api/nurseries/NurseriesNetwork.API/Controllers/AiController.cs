using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using NurseriesNetwork.Core.DTOs.AI;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Core.Interfaces.Services;
using NurseriesNetwork.AI.Agents;
using NurseriesNetwork.AI.Services;

namespace NurseriesNetwork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly IUnitOfWork _uow;
    private readonly NurseryAgentPlugin _agent;
    private readonly GeminiService _gemini;
    private readonly ILogger<AiController> _logger;

    public AiController(
        IAiService aiService,
        IUnitOfWork uow,
        NurseryAgentPlugin agent,
        GeminiService gemini,
        ILogger<AiController> logger)
    {
        _aiService = aiService;
        _uow = uow;
        _agent = agent;
        _gemini = gemini;
        _logger = logger;
    }

    [HttpPost("chat")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> Chat(ChatRequestDto dto)
    {
        var response = await _aiService.GetRecommendationAsync(
            dto.Message, dto.Latitude, dto.Longitude);

        return Ok(new { response });
    }

    [HttpPost("recommend")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> Recommend(ChatRequestDto dto)
    {
        var nearbyNurseries = dto.Latitude.HasValue && dto.Longitude.HasValue
            ? await _uow.Nurseries.GetNearbyAsync(
                dto.Latitude.Value, dto.Longitude.Value, 10)
            : await _uow.Nurseries.GetAllAsync();

        if (!nearbyNurseries.Any())
            return Ok(new { response = "مفيش حضانات قريبة منك دلوقتي" });

        var response = await _aiService.GetRecommendationAsync(
            dto.Message, dto.Latitude, dto.Longitude);

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

    // ===========================
    // POST: api/ai/agent
    // الـ Agent الحقيقي — يفهم كلام طبيعي ويقرر بنفسه
    // ===========================
    [HttpPost("agent")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> AgentMessage([FromBody] ChatRequestDto dto)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var parentEmail = User.FindFirstValue(ClaimTypes.Email);

        _logger.LogInformation(
            "Agent: Received message: {Message}", dto.Message);

        // الخطوة 1: اسأل Gemini يقرر إيه الـ Function المناسبة
        var decision = await _gemini.GetFunctionCallAsync(dto.Message);

        // لو الموديل رد بنص مباشر (مش محتاج Function)
        if (!decision.ShouldCallFunction)
        {
            return Ok(new
            {
                response = decision.DirectTextResponse,
                actionTaken = (string?)null
            });
        }

        // الخطوة 2: نفّذ الـ Function اللي الموديل طلبها
        string functionResult;
        using var argsDoc = JsonDocument.Parse(decision.ArgumentsJson!);

        switch (decision.FunctionName)
        {
            case "find_nearby_nurseries":
                var city = argsDoc.RootElement.TryGetProperty("city", out var cityEl)
                    ? cityEl.GetString() : null;
                var maxPrice = argsDoc.RootElement.TryGetProperty("max_price", out var priceEl)
                    ? (decimal?)priceEl.GetDecimal() : null;

                functionResult = await _agent.FindNurseriesAsync(city, maxPrice);
                break;

            case "create_booking":
                var nurseryName = argsDoc.RootElement
                    .GetProperty("nursery_name").GetString()!;
                var childName = argsDoc.RootElement
                    .GetProperty("child_name").GetString()!;
                var startDateStr = argsDoc.RootElement
                    .GetProperty("start_date").GetString()!;

                if (!DateOnly.TryParse(startDateStr, out var startDate))
                {
                    functionResult = "معذرة، التاريخ المطلوب غير واضح";
                    break;
                }

                functionResult = await _agent.CreateBookingByNameAsync(
                    nurseryName, parentId, childName, startDate, parentEmail);
                break;

            default:
                functionResult = "معذرة، مش قادر أنفذ الطلب ده دلوقتي";
                break;
        }

        _logger.LogInformation(
            "Agent: Function {FunctionName} executed. Result: {Result}",
            decision.FunctionName, functionResult);

        // الخطوة 3: اطلب من Gemini يصيغ النتيجة كرد طبيعي
        var finalResponse = await _gemini.GetFinalResponseAfterFunctionAsync(
            dto.Message, decision.FunctionName!, functionResult);

        return Ok(new
        {
            response = finalResponse,
            actionTaken = decision.FunctionName
        });
    }
}