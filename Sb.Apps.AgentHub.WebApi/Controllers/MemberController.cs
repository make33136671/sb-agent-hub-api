using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Services;

namespace Sb.Apps.AgentHub.WebApi.Controllers
{
    [ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
    public class MemberController : Controller
    {
        private readonly MemberService _service;

        public MemberController(MemberService service)
        {
            _service = service;
        }

        private bool IsMember => User.UserType() == "Member";

        private bool IsTargetOwner(int id)
        {
            var member = _service.GetById(id);
            return member?.Role == MemberRole.Owner;
        }

        [HttpGet]
        public ResultData List(int? id = null, string? keyword = null, UserStatus? status = null,
            MemberRole? role = null, int? ownerId = null,
            string field = "", string order = "", int page = 1, int pageSize = 20)
        {
            // 站长管理仅 Admin 可访问：Member 用户查询站长列表时返回空
            if (role == MemberRole.Owner && IsMember)
                return ResultData.Ok(new { list = new List<object>(), totalCount = 0 });

            return _service.GetList(id, keyword, status, role, ownerId, field, order, page, pageSize);
        }

        [HttpGet]
        public ResultData Detail(int id)
        {
            if (IsTargetOwner(id) && IsMember)
                return ResultData.No("无权查看站长信息!");

            return _service.Detail(id);
        }

        [HttpPost]
        public ResultData Add([FromForm] Member member) => _service.Create(member);

        [HttpPost]
        public ResultData Edit([FromForm] Member member)
        {
            if (IsTargetOwner(member.Id) && IsMember)
                return ResultData.No("无权修改站长信息!");

            return _service.Edit(member);
        }

        [HttpPost]
        public ResultData Del([FromForm] int id)
        {
            if (IsTargetOwner(id) && IsMember)
                return ResultData.No("无权删除站长账号!");

            return _service.Del(id);
        }

        [HttpPost]
        public ResultData ResetPassword([FromForm] int id, [FromForm] string newPassword)
        {
            if (IsTargetOwner(id) && IsMember)
                return ResultData.No("无权重置站长密码!");

            return _service.ResetPassword(id, newPassword);
        }

        [HttpPost]
        public ResultData ToggleStatus([FromForm] int id)
        {
            if (IsTargetOwner(id) && IsMember)
                return ResultData.No("无权修改站长状态!");

            return _service.ToggleStatus(id);
        }

        [HttpGet]
        public ResultData AgentsByOwner(int ownerId) => _service.GetAgentsByOwner(ownerId);
    }
}
