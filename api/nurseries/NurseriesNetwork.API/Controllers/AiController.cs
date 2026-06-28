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
    private readonly ILlmService _llm;
    private readonly ILogger<AiController> _logger;
    private readonly NurseryAdminAgentPlugin _adminAgent;

    public AiController(
        IAiService aiService,
        IUnitOfWork uow,
        NurseryAgentPlugin agent,
        ILlmService llm,
        ILogger<AiController> logger,
        NurseryAdminAgentPlugin adminAgent)
    {
        _aiService = aiService;
        _uow = uow;
        _agent = agent;
        _llm = llm;
        _logger = logger;
        _adminAgent = adminAgent;
    }

    // ===========================
    // POST: api/ai/chat
    // ===========================
    [HttpPost("chat")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> Chat(ChatRequestDto dto)
    {
        // ✅ لو الرسالة فيها نية حجز واضحة، حوّلها لمنطق الـ Agent مباشرة
        var intent = await _llm.ClassifyIntentAsync(dto.Message);

        if (intent.Intent == "BOOKING")
        {
            return await AgentMessage(dto);
        }

        var result = await _aiService.GetRecommendationAsync(
            dto.Message, dto.Latitude, dto.Longitude, intent);

        return Ok(new
        {
            response = result.ResponseText,
            nurseries = result.Nurseries
        });
    }

    // ===========================
    // POST: api/ai/recommend
    // ✅ بقت تستخدم نفس مصدر النتائج بتاع GetRecommendationAsync،
    //    مش استعلام جغرافي مستقل بالـ lat/lng بس
    // ===========================
    [HttpPost("recommend")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> Recommend(ChatRequestDto dto)
    {
        var intent = await _llm.ClassifyIntentAsync(dto.Message);

        if (intent.Intent == "BOOKING")
        {
            return await AgentMessage(dto);
        }

        var result = await _aiService.GetRecommendationAsync(
            dto.Message, dto.Latitude, dto.Longitude, intent);

        return Ok(new
        {
            response = result.ResponseText,
            nurseries = result.Nurseries
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

        // الخطوة 1: اسأل الموديل يقرر إيه الـ Function المناسبة
        var decision = await _llm.GetFunctionCallAsync(dto.Message);

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

        // الخطوة 3: اطلب من الموديل يصيغ النتيجة كرد طبيعي
        var finalResponse = await _llm.GetFinalResponseAfterFunctionAsync(
            dto.Message, decision.FunctionName!, functionResult);

        return Ok(new
        {
            response = finalResponse,
            actionTaken = decision.FunctionName
        });
    }



    [HttpPost("admin-agent")]
    [Authorize(Roles = "NurseryAdmin")]
    public async Task<IActionResult> AdminAgentMessage([FromBody] ChatRequestDto dto)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        _logger.LogInformation(
            "AdminAgent: Received message from {AdminId}: {Message}", adminUserId, dto.Message);

        // الخطوة 1: اسأل الموديل يقرر إيه الـ Function المناسبة
        var decision = await _llm.GetAdminFunctionCallAsync(dto.Message);

        // لو الموديل رد بنص مباشر (تحية/شكر/غير متعلق بالإدارة)
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
            case "get_nursery_performance":
                functionResult = await _adminAgent.GetNurseryPerformanceSummaryAsync(adminUserId);
                break;

            case "search_my_bookings":
                var bookingStatus = argsDoc.RootElement.TryGetProperty("booking_status", out var bsEl)
                    ? bsEl.GetString() : null;
                var paymentStatus = argsDoc.RootElement.TryGetProperty("payment_status", out var psEl)
                    ? psEl.GetString() : null;
                var maxAge = argsDoc.RootElement.TryGetProperty("max_child_age_months", out var maxAgeEl)
                    ? (int?)maxAgeEl.GetInt32() : null;
                var minAge = argsDoc.RootElement.TryGetProperty("min_child_age_months", out var minAgeEl)
                    ? (int?)minAgeEl.GetInt32() : null;
                var withinDays = argsDoc.RootElement.TryGetProperty("within_last_days", out var daysEl)
                    ? (int?)daysEl.GetInt32() : null;

                var searchFilters = new AdminBookingSearchFilters(
                    bookingStatus, paymentStatus, maxAge, minAge, withinDays);

                functionResult = await _adminAgent.SearchMyBookingsAsync(adminUserId, searchFilters);
                break;

            default:
                functionResult = "معذرة، مش قادر أنفذ الطلب ده دلوقتي";
                break;
        }

        _logger.LogInformation(
            "AdminAgent: Function {FunctionName} executed. Result: {Result}",
            decision.FunctionName, functionResult);

        // الخطوة 3: اطلب من الموديل يصيغ النتيجة كرد طبيعي بشري مفهوم
        var finalResponse = await _llm.GetFinalResponseAfterFunctionAsync(
            dto.Message, decision.FunctionName!, functionResult);

        return Ok(new
        {
            response = finalResponse,
            actionTaken = decision.FunctionName
        });
    }

}