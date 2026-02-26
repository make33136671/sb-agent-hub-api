using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers
{
    [ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
    public class MemberController : Controller
    {
        private readonly MemberService _service;

        public MemberController(MemberService service)
        {
            _service = service;
        }

        [HttpGet]
        public ResultData List(int? id = null, string? keyword = null, UserStatus? status = null,
            MemberRole? role = null, int? ownerId = null,
            string field = "", string order = "", int page = 1, int pageSize = 20)
        {
            return _service.GetList(id, keyword, status, role, ownerId, field, order, page, pageSize);
        }

        [HttpGet]
        public ResultData Detail(int id) => _service.Detail(id);

        [HttpPost]
        public ResultData Add([FromForm] Member member) => _service.Create(member);

        [HttpPost]
        public ResultData Edit([FromForm] Member member) => _service.Edit(member);

        [HttpPost]
        public ResultData Del([FromForm] int id) => _service.Del(id);

        [HttpPost]
        public ResultData ResetPassword([FromForm] int id, [FromForm] string newPassword)
            => _service.ResetPassword(id, newPassword);

        [HttpPost]
        public ResultData ToggleStatus([FromForm] int id) => _service.ToggleStatus(id);

        [HttpGet]
        public ResultData AgentsByOwner(int ownerId) => _service.GetAgentsByOwner(ownerId);
    }
}
