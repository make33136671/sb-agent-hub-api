using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using SbAgentHub.Models;
using SbAgentHub.Services;
using System.Collections.Generic;

namespace SbAgentHub.API.Controllers
{
    [ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
    public class ManagerController : Controller
    {
        private readonly ILogger<ManagerController> _logger;
        private readonly AdminService _adminService;
        private readonly AdminRoleService _adminRoleService;
        private readonly AdminRoleMenuService _roleMenuService;

        public ManagerController(ILogger<ManagerController> logger,
            AdminService adminService,
            AdminRoleService adminRoleService,
            AdminRoleMenuService roleMenuService)
        {
            _logger = logger;
            _adminService = adminService;
            _adminRoleService = adminRoleService;
            _roleMenuService = roleMenuService;
        }

        #region 管理员

        [HttpGet]
        public ResultData List(string username = "", string name = "", int? roleId = null,
            AdminStatus? status = null, string field = "", string order = "", int page = 1, int pageSize = 10)
        {
            return _adminService.GetAdminList(username, name, roleId, status, field, order, page, pageSize);
        }

        [HttpPost]
        public ResultData Add([FromForm] Admin admin) => _adminService.CreateAdmin(admin);

        [HttpPost]
        public ResultData Edit([FromForm] Admin admin) => _adminService.EditAdmin(admin);

        [HttpPost]
        public ResultData Del([FromForm] int id) => _adminService.DelAdmin(id);

        [HttpGet]
        public ResultData Detail(int id) => _adminService.GetAdminDetail(id);

        [HttpPost]
        public ResultData ResetPassword([FromForm] int id, [FromForm] string newPassword)
            => _adminService.ResetAdminPassword(id, newPassword);

        [HttpPost]
        public ResultData ToggleStatus([FromForm] int id) => _adminService.ToggleAdminStatus(id);

        #endregion

        #region 角色

        [HttpGet]
        public ResultData RoleList(string name = "", string roleKey = "", string field = "", string order = "", int page = 1, int pageSize = 10)
        {
            return _adminRoleService.GetList(name, roleKey, field, order, page, pageSize);
        }

        [HttpGet]
        public ResultData QueryAdminRole(string key) => _adminRoleService.QueryAdminRole(key);

        [HttpPost]
        public ResultData RoleAdd([FromForm] AdminRole adminRole) => _adminRoleService.Create(adminRole);

        [HttpPost]
        public ResultData RoleEdit([FromForm] AdminRole adminRole) => _adminRoleService.Edit(adminRole);

        [HttpPost]
        public ResultData RoleDel([FromForm] int id) => _adminRoleService.Del(id);

        [HttpGet]
        public ResultData RoleDetail(int id) => _adminRoleService.Detail(id);

        #endregion

        #region 角色菜单权限

        [HttpGet]
        public ResultData GetMenuIds(int roleId) => _roleMenuService.GetMenuIdsByRoleId(roleId);

        [HttpGet]
        public ResultData GetMenusDetail(int roleId) => _roleMenuService.GetRoleMenusWithDetail(roleId);

        [HttpPost]
        public ResultData SetMenus([FromForm] int roleId, [FromForm] string menuIds)
        {
            var menuIdList = new List<int>();
            if (!string.IsNullOrEmpty(menuIds))
            {
                menuIdList = menuIds.Split(',')
                    .Select(id => int.TryParse(id.Trim(), out int menuId) ? menuId : 0)
                    .Where(id => id > 0)
                    .ToList();
            }
            return _roleMenuService.SetMenusForRole(roleId, menuIdList);
        }

        [HttpPost]
        public ResultData AddMenu([FromForm] int roleId, [FromForm] int menuId)
            => _roleMenuService.AddMenuToRole(roleId, menuId);

        [HttpPost]
        public ResultData RemoveMenu([FromForm] int roleId, [FromForm] int menuId)
            => _roleMenuService.RemoveMenuFromRole(roleId, menuId);

        [HttpPost]
        public ResultData CopyMenus([FromForm] int fromRoleId, [FromForm] int toRoleId)
            => _roleMenuService.CopyMenuPermissions(fromRoleId, toRoleId);

        [HttpGet]
        public ResultData GetRolesByMenu(int menuId) => _roleMenuService.GetRolesByMenuId(menuId);

        #endregion
    }
}
