using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 聊天用户管理
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class ChatUserController : Controller
{
    private readonly ChatUserService _service;

    public ChatUserController(ChatUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public ResultData List(string keyword = "", int? ownerId = null, int? platformId = null,
        ChatUserStatus? status = null, ChatUserType? userType = null,
        string field = "", string order = "", int page = 1, int pageSize = 20)
        => _service.GetList(keyword, ownerId, platformId, status, userType, field, order, page, pageSize);

    [HttpGet]
    public ResultData Detail(int id) => _service.Detail(id);

    [HttpPost]
    public ResultData Edit([FromForm] ChatUser chatUser) => _service.Edit(chatUser);

    [HttpPost]
    public ResultData Del([FromForm] int id) => _service.Del(id);

    [HttpPost]
    public ResultData UpdateStatus([FromForm] int id, [FromForm] ChatUserStatus status)
        => _service.UpdateStatus(id, status);

    [HttpPost]
    public ResultData AddToBlacklist([FromForm] int id) => _service.AddToBlacklist(id);

    [HttpPost]
    public ResultData RemoveFromBlacklist([FromForm] int id) => _service.RemoveFromBlacklist(id);

    [HttpPost]
    public ResultData AssignAgent([FromForm] int chatUserId, [FromForm] int agentId)
        => _service.AssignAgent(chatUserId, agentId);

    [HttpGet]
    public ResultData OnlineUsers(int? ownerId = null) => _service.OnlineUsers(ownerId);

    [HttpPost]
    public ResultData UpdateRemark([FromForm] int id, [FromForm] string nicknameRemark, [FromForm] string remark)
        => _service.UpdateRemark(id, nicknameRemark, remark);
}
