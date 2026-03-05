using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 自动回复规则管理（站长端）
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class AutoReplyController : Controller
{
    private readonly AutoReplyService _service;

    public AutoReplyController(AutoReplyService service)
    {
        _service = service;
    }

    [HttpGet]
    public ResultData List(int? platformId = null, int? triggerType = null)
        => _service.GetRuleList(platformId, triggerType);

    [HttpPost]
    public ResultData Create([FromBody] CreateAutoReplyRequest request)
        => _service.CreateRule(request.PlatformId, request.TriggerType, request.Keywords,
            request.MatchType, request.ReplyContent, request.ReplyType, request.Priority);

    [HttpPost]
    public ResultData Update([FromBody] UpdateAutoReplyRequest request)
        => _service.UpdateRule(request.Id, request.TriggerType, request.Keywords,
            request.MatchType, request.ReplyContent, request.ReplyType, request.Priority, request.IsEnabled);

    [HttpPost]
    public ResultData Delete([FromForm] int id)
        => _service.DeleteRule(id);
}

public class CreateAutoReplyRequest
{
    public int PlatformId { get; set; }
    public int TriggerType { get; set; }
    public string Keywords { get; set; } = "";
    public int MatchType { get; set; }
    public string ReplyContent { get; set; } = "";
    public int ReplyType { get; set; }
    public int Priority { get; set; }
}

public class UpdateAutoReplyRequest
{
    public int Id { get; set; }
    public int TriggerType { get; set; }
    public string Keywords { get; set; } = "";
    public int MatchType { get; set; }
    public string ReplyContent { get; set; } = "";
    public int ReplyType { get; set; }
    public int Priority { get; set; }
    public bool IsEnabled { get; set; } = true;
}
