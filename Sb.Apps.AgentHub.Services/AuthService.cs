using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Authentication.Jwt;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Core.Util;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Models.Dto;
using System.Security.Claims;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 统一认证服务 — Admin 和 Member 共用登录入口
    /// </summary>
    [AutoDependencyInjection]
    public class AuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtToken _jwtToken;
        private readonly MenuService _menuService;
        private readonly AdminService _adminService;
        private readonly MemberService _memberService;
        private readonly AdminRoleMenuService _roleMenuService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IHttpContextAccessor httpContextAccessor,
            JwtToken jwtToken,
            MenuService menuService,
            AdminService adminService,
            MemberService memberService,
            AdminRoleMenuService roleMenuService,
            ICacheService cacheService,
            ILogger<AuthService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _jwtToken = jwtToken;
            _menuService = menuService;
            _adminService = adminService;
            _memberService = memberService;
            _roleMenuService = roleMenuService;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// 统一登录 — 先查 Admin 表，再查 Member 表
        /// </summary>
        public async Task<ResultData> UnifiedLoginAsync(string username, string password)
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
                throw new InvalidOperationException("HttpContext not available.");

            // 1. 先查 Admin 表
            var admin = _adminService.GetByUsername(username);

            if (admin != null)
            {
                return await LoginAsAdmin(admin, password, ctx);
            }

            // 2. 再查 Member 表
            var member = _memberService.GetByUsername(username);

            if (member != null)
            {
                return await LoginAsMember(member, password, ctx);
            }

            // 3. 都找不到
            return ResultData.No("账号密码错误！");
        }

        /// <summary>
        /// Admin 登录逻辑
        /// </summary>
        private async Task<ResultData> LoginAsAdmin(Admin admin, string password, HttpContext ctx)
        {
            if (!PasswordUtil.VerifyPassword(password, admin.Password))
            {
                _adminService.RecordLoginLog(admin.Id, "Admin", admin.Username, 1, ctx);
                return ResultData.No("账号密码错误！");
            }

            if (admin.Status != AdminStatus.Active)
            {
                return ResultData.No("您的账号已被封锁！");
            }

            // 更新最后在线时间
            admin.LastOnlineTime = DateTime.Now;
            _adminService.UpdateColumns(admin, c => c.LastOnlineTime!);

            var claimList = new List<Claim>
            {
                new Claim("Id", admin.Id.ToString()),
                new Claim("Username", admin.Username),
                new Claim("UserType", "Admin"),
            };

            string accessToken = _jwtToken.CreateToken("JwtAdmin", claimList);
            ctx.Response.Headers["accessToken"] = accessToken;

            _adminService.RecordLoginLog(admin.Id, "Admin", admin.Username, 0, ctx);

            string homePath = _menuService.GetDefaultMenupathByUserId(admin.Id);

            // Admin 的 roles 始终包含 "admin"，保证前端 roles.includes('admin') 判定正确
            var adminRoles = new List<string> { "admin" };
            var roleKey = admin.AdminRole?.RoleKey;
            if (!string.IsNullOrEmpty(roleKey) && roleKey != "admin")
                adminRoles.Add(roleKey);

            return ResultData.Ok(new
            {
                accessToken,
                userInfo = new
                {
                    userId = admin.Id,
                    admin.Id,
                    admin.Username,
                    realName = admin.Name,
                    roles = adminRoles.ToArray(),
                    RoleName = admin.AdminRole?.Name,
                    admin.Avatar,
                    homePath,
                    UserType = "Admin"
                }
            });
        }

        /// <summary>
        /// Member 登录逻辑
        /// </summary>
        private async Task<ResultData> LoginAsMember(Member member, string password, HttpContext ctx)
        {
            if (!PasswordUtil.VerifyPassword(password, member.Password))
            {
                _adminService.RecordLoginLog(member.Id, "Member", member.Username, 1, ctx);
                return ResultData.No("账号密码错误！");
            }

            if (member.Status != UserStatus.Active)
            {
                return ResultData.No("您的账号已被封锁！");
            }

            // 更新最后在线时间
            member.LastOnlineTime = DateTime.Now;
            _memberService.UpdateColumns(member, c => c.LastOnlineTime);

            var claimList = new List<Claim>
            {
                new Claim("Id", member.Id.ToString()),
                new Claim("Username", member.Username),
                new Claim("UserType", "Member"),
                new Claim("Role", member.Role.ToString()),
            };

            string accessToken = _jwtToken.CreateToken("JwtAdmin", claimList);
            ctx.Response.Headers["accessToken"] = accessToken;

            _adminService.RecordLoginLog(member.Id, "Member", member.Username, 0, ctx);

            return ResultData.Ok(new
            {
                accessToken,
                userInfo = new
                {
                    userId = member.Id,
                    member.Id,
                    member.Username,
                    realName = member.Name,
                    roles = new[] { member.Role.ToString() },
                    member.Avatar,
                    UserType = "Member",
                    Role = member.Role.ToString(),
                    member.OwnerId,
                    homePath = "/workbench/index"
                }
            });
        }

        /// <summary>
        /// 获取当前登录用户信息（Admin 或 Member）
        /// </summary>
        public async Task<ResultData> GetInfoAsync()
        {
            var user = _httpContextAccessor.HttpContext!.User;
            int userId = user.Id();
            if (userId <= 0) return ResultData.No("授权失效!");

            string userType = user.UserType();

            if (userType == "Admin")
            {
                var admin = _adminService.GetByIdWithRole(userId);

                if (admin == null) return ResultData.No("授权失效!");

                string homePath = _menuService.GetDefaultMenupathByUserId(admin.Id);

                // Admin 的 roles 始终包含 "admin"，保证前端 roles.includes('admin') 判定正确
                var adminRoles = new List<string> { "admin" };
                var roleKey = admin.AdminRole?.RoleKey;
                if (!string.IsNullOrEmpty(roleKey) && roleKey != "admin")
                    adminRoles.Add(roleKey);

                return ResultData.Ok(new
                {
                    userId = admin.Id,
                    admin.Id,
                    admin.Username,
                    realName = admin.Name,
                    roles = adminRoles.ToArray(),
                    RoleName = admin.AdminRole?.Name,
                    admin.Avatar,
                    homePath,
                    UserType = "Admin"
                });
            }
            else if (userType == "Member")
            {
                var member = _memberService.GetById(userId);

                if (member == null) return ResultData.No("授权失效!");

                return ResultData.Ok(new
                {
                    userId = member.Id,
                    member.Id,
                    member.Username,
                    realName = member.Name,
                    roles = new[] { member.Role.ToString() },
                    member.Avatar,
                    UserType = "Member",
                    Role = member.Role.ToString(),
                    RoleName = member.Role.ToString() == "Owner" ? "站长" : "客服",
                    member.OwnerId,
                    member.OnlineStatus,
                    member.RealtimeUserId,
                    homePath = "/workbench/index"
                });
            }

            return ResultData.No("授权失效!");
        }

        /// <summary>
        /// 获取当前用户的权限菜单（Admin 按角色加载，Member 按 Role 加载固定菜单）
        /// </summary>
        public ResultData GetMenus()
        {
            var user = _httpContextAccessor.HttpContext!.User;
            int userId = user.Id();
            if (userId <= 0) return ResultData.No("未登录或登录已过期");

            string userType = user.UserType();

            if (userType == "Admin")
            {
                // Admin 走角色-菜单权限
                var admin = _adminService.GetById(userId);
                if (admin == null) return ResultData.No("未登录或登录已过期");

                int roleId = admin.RoleId;
                string cacheKey = $"menu:user:{userId}:role:{roleId}";

                var cachedMenus = _cacheService.Get<List<VbenMenu>>(cacheKey);
                if (cachedMenus != null) return ResultData.Ok(cachedMenus);

                var menuIds = _roleMenuService.GetMenuIdsByRoleIdRaw(roleId);

                if (!menuIds.Any())
                {
                    var emptyMenus = new List<VbenMenu>();
                    _cacheService.Set(cacheKey, emptyMenus, TimeSpan.FromMinutes(30));
                    return ResultData.Ok(emptyMenus);
                }

                var menus = _menuService.GetByIds(menuIds);

                var parentIds = menus.Where(m => m.ParentId > 0).Select(m => m.ParentId).Distinct().ToList();
                var parentMenus = _menuService.GetByIds(parentIds);

                var allMenus = menus.Union(parentMenus).GroupBy(m => m.Id).Select(g => g.First()).OrderBy(m => m.OrderNo).ToList();

                // Admin 也需要访问客服工作台（用于测试和临时接管消息）
                // 将工作台菜单追加进来，hideInMenu 使其不在侧栏显示
                var existingIds = new HashSet<int>(allMenus.Select(m => m.Id));
                var workbenchMenus = _menuService.GetByPathPrefix("/workbench");
                foreach (var wm in workbenchMenus)
                {
                    if (!existingIds.Contains(wm.Id))
                    {
                        wm.HideInMenu = true;
                        allMenus.Add(wm);
                        existingIds.Add(wm.Id);
                    }
                }
                // 补充工作台父菜单（如有）
                var wpIds = workbenchMenus.Where(m => m.ParentId > 0).Select(m => m.ParentId).Distinct().ToList();
                foreach (var wpId in wpIds)
                {
                    if (!existingIds.Contains(wpId))
                    {
                        var pm = _menuService.GetById(wpId);
                        if (pm != null)
                        {
                            pm.HideInMenu = true;
                            allMenus.Add(pm);
                            existingIds.Add(pm.Id);
                        }
                    }
                }

                allMenus = allMenus.OrderBy(m => m.OrderNo).ToList();
                var menuList = BuildVbenMenuTree(allMenus);

                _cacheService.Set(cacheKey, menuList, TimeSpan.FromMinutes(30));
                return ResultData.Ok(menuList);
            }
            else if (userType == "Member")
            {
                // Member 按 Role 加载固定菜单集
                string role = user.Role();
                string cacheKey = $"menu:member:role:{role}";

                var cachedMenus = _cacheService.Get<List<VbenMenu>>(cacheKey);
                if (cachedMenus != null) return ResultData.Ok(cachedMenus);

                List<Menu> menus;

                if (role == "Agent")
                {
                    // Agent 看工作台 + 个人中心
                    var workbenchMenus = _menuService.GetByPathPrefix("/workbench");
                    var profileMenu = _menuService.GetByPath("/profile");
                    menus = workbenchMenus;
                    if (profileMenu != null) menus = menus.Append(profileMenu).ToList();
                    menus = menus.OrderBy(m => m.OrderNo).ToList();
                }
                else
                {
                    // Owner 加载 Owner 菜单（OrderNo >= 10 的一级菜单及其子菜单）
                    var ownerTopMenus = _menuService.GetOwnerTopMenus();

                    // 排除站长管理菜单（MemberMgmt 及子菜单 OwnerList），仅 Admin 可见
                    ownerTopMenus = ownerTopMenus.Where(m => m.Name != "MemberMgmt").ToList();

                    var ownerTopMenuIds = ownerTopMenus.Select(m => m.Id).ToList();
                    var ownerChildMenus = _menuService.GetChildMenus(ownerTopMenuIds);
                    ownerChildMenus = ownerChildMenus
                        .Where(c => c.Name != "OwnerList" && c.Path != "/member-mgmt/owner")
                        .ToList();

                    menus = ownerTopMenus.Concat(ownerChildMenus).OrderBy(m => m.OrderNo).ToList();
                }

                // 追加工作台菜单（HideInMenu=true，仅注册路由不在侧栏显示）
                var existingIds = new HashSet<int>(menus.Select(m => m.Id));
                var wbMenus = _menuService.GetByPathPrefix("/workbench");
                foreach (var wm in wbMenus)
                {
                    if (!existingIds.Contains(wm.Id))
                    {
                        wm.HideInMenu = true;
                        menus.Add(wm);
                        existingIds.Add(wm.Id);
                    }
                }
                var wbParentIds = wbMenus.Where(m => m.ParentId > 0).Select(m => m.ParentId).Distinct().ToList();
                foreach (var pid in wbParentIds)
                {
                    if (!existingIds.Contains(pid))
                    {
                        var pm = _menuService.GetById(pid);
                        if (pm != null)
                        {
                            pm.HideInMenu = true;
                            menus.Add(pm);
                            existingIds.Add(pm.Id);
                        }
                    }
                }

                var menuList = BuildVbenMenuTree(menus);
                _cacheService.Set(cacheKey, menuList, TimeSpan.FromMinutes(30));
                return ResultData.Ok(menuList);
            }

            return ResultData.No("未登录或登录已过期");
        }

        /// <summary>
        /// 修改密码（Admin / Member 通用）
        /// </summary>
        public ResultData ChangePassword(string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(oldPassword)) return ResultData.No("旧密码不能为空!");
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6) return ResultData.No("请输入6位以上新密码!");

            var user = _httpContextAccessor.HttpContext!.User;
            int userId = user.Id();
            if (userId <= 0) return ResultData.No("授权失效!");

            string userType = user.UserType();

            if (userType == "Admin")
            {
                var admin = _adminService.GetById(userId);
                if (admin == null || admin.Status != AdminStatus.Active) return ResultData.No("授权失效!");
                if (!PasswordUtil.VerifyPassword(oldPassword, admin.Password)) return ResultData.No("旧密码错误!");

                admin.Password = PasswordUtil.EncryptPassword(newPassword);
                _adminService.UpdateColumns(admin, c => c.Password);
                return ResultData.Ok("修改密码成功!");
            }
            else if (userType == "Member")
            {
                var member = _memberService.GetById(userId);
                if (member == null || member.Status != UserStatus.Active) return ResultData.No("授权失效!");
                if (!PasswordUtil.VerifyPassword(oldPassword, member.Password)) return ResultData.No("旧密码错误!");

                member.Password = PasswordUtil.EncryptPassword(newPassword);
                _memberService.UpdateColumns(member, c => c.Password);
                return ResultData.Ok("修改密码成功!");
            }

            return ResultData.No("授权失效!");
        }

        /// <summary>
        /// 修改头像（Admin / Member 通用）
        /// </summary>
        public ResultData ChangeAvatar(string avatar)
        {
            if (string.IsNullOrEmpty(avatar)) return ResultData.No("头像地址不能为空!");

            var user = _httpContextAccessor.HttpContext!.User;
            int userId = user.Id();
            if (userId <= 0) return ResultData.No("授权失效!");

            string userType = user.UserType();

            if (userType == "Admin")
            {
                var admin = _adminService.GetById(userId);
                if (admin == null) return ResultData.No("授权失效!");

                admin.Avatar = avatar;
                _adminService.UpdateColumns(admin, c => c.Avatar);
                return ResultData.Ok("修改头像成功!");
            }
            else if (userType == "Member")
            {
                var member = _memberService.GetById(userId);
                if (member == null) return ResultData.No("授权失效!");

                member.Avatar = avatar;
                _memberService.UpdateColumns(member, c => c.Avatar);
                return ResultData.Ok("修改头像成功!");
            }

            return ResultData.No("授权失效!");
        }

        /// <summary>
        /// 修改个人资料（Admin / Member 通用）
        /// </summary>
        public ResultData UpdateProfile(string name)
        {
            if (!string.IsNullOrEmpty(name) && name.Length > 50)
                return ResultData.No("姓名长度不能超过50个字符!");

            var user = _httpContextAccessor.HttpContext!.User;
            int userId = user.Id();
            if (userId <= 0) return ResultData.No("授权失效!");

            string userType = user.UserType();

            if (userType == "Admin")
            {
                var admin = _adminService.GetById(userId);
                if (admin == null) return ResultData.No("授权失效!");

                admin.Name = name;
                _adminService.UpdateColumns(admin, c => c.Name);
                return ResultData.Ok("修改资料成功!");
            }
            else if (userType == "Member")
            {
                var member = _memberService.GetById(userId);
                if (member == null) return ResultData.No("授权失效!");

                member.Name = name;
                _memberService.UpdateColumns(member, c => c.Name);
                return ResultData.Ok("修改资料成功!");
            }

            return ResultData.No("授权失效!");
        }

        #region Private Helpers

        private List<VbenMenu> BuildVbenMenuTree(List<Menu> menus)
        {
            var topMenus = menus.Where(m => m.ParentId == 0).OrderBy(m => m.OrderNo).ToList();
            return topMenus.Select(parent => new VbenMenu(parent, menus)).ToList();
        }

        #endregion
    }
}
