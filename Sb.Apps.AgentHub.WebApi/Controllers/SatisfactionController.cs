using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Services;

namespace Sb.Apps.AgentHub.WebApi.Controllers;

/// <summary>
/// 满意度评价管理（管理端）
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class SatisfactionController : Controller
{
    private readonly SatisfactionService _service;

    public SatisfactionController(SatisfactionService service)
    {
        _service = service;
    }

    [HttpGet]
    public ResultData List(int? agentId = null, int page = 1, int pageSize = 20)
        => _service.GetRatingList(agentId, page, pageSize);

    [HttpGet]
    public ResultData Statistics(int? agentId = null)
        => _service.GetStatistics(agentId);
}

/// <summary>
/// 满意度评价提交（访客端，公开接口）
/// </summary>
[ApiController, Route("/api/[controller]/[action]")]
public class RatingController : Controller
{
    private readonly SatisfactionService _service;

    public RatingController(SatisfactionService service)
    {
        _service = service;
    }

    [HttpPost, AllowAnonymous]
    public ResultData Submit([FromBody] SubmitRatingRequest request)
    {
        if (request == null) return ResultData.No("参数无效!");
        return _service.SubmitRating(
            request.OwnerId, request.PlatformId, request.ChatUserId,
            request.AgentId, request.Rating, request.Tags, request.Comment, request.ConversationId);
    }
}

public class SubmitRatingRequest
{
    public int OwnerId { get; set; }
    public int PlatformId { get; set; }
    public string ChatUserId { get; set; } = "";
    public int AgentId { get; set; }
    public int Rating { get; set; }
    public string Tags { get; set; } = "";
    public string Comment { get; set; } = "";
    public string ConversationId { get; set; } = "";
}
