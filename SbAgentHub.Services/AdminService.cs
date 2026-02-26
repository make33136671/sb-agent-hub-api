using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Authentication.Jwt;
using SbAgentHub.Core.Expand;
using SbAgentHub.Core.FreeSql;
using SbAgentHub.Core.Util;
using SbAgentHub.Models;
using SbAgentHub.Models.Dto;
using System.Data;
using System.Security.Claims;


namespace SbAgentHub.Services
{
    [AutoDependencyInjection]
    public class AdminService : BaseCrudService<Admin>
    {
        IFreeSql _fsql;
        IHttpContextAccessor _httpContextAccessor;
        JwtToken _jwtToken;
        MenuService _menuService;
        ICacheService _cacheService;

        public AdminService(IFreeSql fsql, ILogger<AdminService> logger, IHttpContextAccessor httpContextAccessor, JwtToken jwtToken, MenuService menuService, ICacheService cacheService) : base(fsql, logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _jwtToken = jwtToken;
            _menuService = menuService;
            _cacheService = cacheService;
        }


        /// <summary>
        /// 登陆管理系统
        /// </summary>
        /// <param name="username">账号</param>
        /// <param name="password">密码</param>
        public async Task<ResultData> LoginAsync(string username, string password)
        {
            Admin admin = this.Select.Include(c => c.AdminRole).Where(c => c.Username == username).First();
            var ctx = _httpContextAccessor.HttpContext;

            if (admin == null)
            {
                return ResultData.No("账号密码错误！");
            }

            if (!PasswordUtil.VerifyPassword(password, admin.Password))
            {
                _fsql.Insert(new LoginLog
                {
                    UserId = admin.Id,
                    UserType = 1,
                    Username = username,
                    IpAddress = ctx?.Connection?.RemoteIpAddress?.ToString(),
                    UserAgent = ctx?.Request?.Headers["User-Agent"].ToString(),
                    Status = 1,
                    CreateTime = DateTime.Now
                }).ExecuteAffrows();
                return ResultData.No("账号密码错误！");
            }

            if (admin.Status != AdminStatus.Active)
            {
                return ResultData.No("您的账号已被封锁！");
            }

            admin.LastOnlineTime = DateTime.Now;
            UpdateDiy.UpdateColumns(c => c.LastOnlineTime).SetSource(admin).ExecuteAffrows();

            var claimList = new List<Claim>()
            {
                new Claim("Id", admin.Id.ToString()),
                new Claim("Username", admin.Username),
                new Claim("UserType", "Admin"),
            };

            string accessToken = _jwtToken.CreateToken("JwtAdmin", claimList);
            _httpContextAccessor.HttpContext.Response.Headers["accessToken"] = accessToken;

            _fsql.Insert(new LoginLog
            {
                UserId = admin.Id,
                UserType = 1,
                Username = admin.Username,
                IpAddress = ctx?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = ctx?.Request?.Headers["User-Agent"].ToString(),
                Status = 0,
                CreateTime = DateTime.Now
            }).ExecuteAffrows();

            var userInfo = GetAdminInfo(admin);

            return ResultData.Ok(new
            {
                accessToken,
                userInfo
            });
        }

        /// <summary>
        /// 退出管理系统
        /// </summary>
        public ResultData Logout()
        {
            return ResultData.Ok("退出系统成功！");
        }

        public async Task<ResultData> GetInfoAsync()
        {
            int userId = _httpContextAccessor.HttpContext.User.Id();
            if (userId <= 0) return ResultData.No("授权失效!");

            Admin admin = Select.Include(c => c.AdminRole).Where(c => c.Id == userId).First();

            if (admin == null)
            {
                return ResultData.No("授权失效!");
            }

            string homePath = _menuService.GetDefaultMenupathByUserId(admin.Id);

            return ResultData.Ok("获取管理员信息成功!", new
            {
                userId = admin.Id,
                admin.Id,
                admin.Username,
                RoleKey = admin.AdminRole?.RoleKey,
                RoleName = admin.AdminRole?.Name,
                Name = admin.Name,
                admin.Avatar,
                parentId = admin.ParentId,
                homePath = homePath,
                UserType = "Admin"
            });
        }

        /// <summary>
        /// 修改头像
        /// </summary>
        public async Task<ResultData> ChangeAvatarAsync(string avatar)
        {
            if (string.IsNullOrEmpty(avatar))
            {
                return ResultData.No("头像地址不能为空!");
            }

            int userId = _httpContextAccessor.HttpContext.User.Id();
            if (userId <= 0) return ResultData.No("授权失效!");

            Admin admin = Select.Include(c => c.AdminRole).Where(c => c.Id == userId).First();

            if (admin == null || admin.Status != AdminStatus.Active)
            {
                return ResultData.No("授权失效!");
            }

            admin.Avatar = avatar;
            UpdateDiy.UpdateColumns(c => c.Avatar).SetSource(admin).ExecuteAffrows();

            return ResultData.Ok("修改头像成功!", GetAdminInfo(admin));
        }

        /// <summary>
        /// 修改个人资料
        /// </summary>
        public async Task<ResultData> UpdateProfileAsync(string name)
        {
            int userId = _httpContextAccessor.HttpContext.User.Id();
            if (userId <= 0) return ResultData.No("授权失效!");

            Admin admin = Select.Include(c => c.AdminRole).Where(c => c.Id == userId).First();

            if (admin == null || admin.Status != AdminStatus.Active)
            {
                return ResultData.No("授权失效!");
            }

            if (!string.IsNullOrEmpty(name) && name.Length > 50)
            {
                return ResultData.No("姓名长度不能超过50个字符!");
            }

            admin.Name = name;
            UpdateDiy.UpdateColumns(c => c.Name).SetSource(admin).ExecuteAffrows();

            return ResultData.Ok("修改资料成功!", GetAdminInfo(admin));
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public ResultData ChangePassword(string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(oldPassword))
            {
                return ResultData.No("数据异常,修改失败!");
            }

            if (newPassword.Length < 6)
            {
                return ResultData.No("请输入6位以上新密码!");
            }

            int userId = _httpContextAccessor.HttpContext.User.Id();
            if (userId <= 0) return ResultData.No("授权失效!");

            Admin admin = Select.Include(c => c.AdminRole).Where(c => c.Id == userId).First();

            if (admin == null || admin.Status != AdminStatus.Active)
            {
                return ResultData.No("授权失效!");
            }

            if (!PasswordUtil.VerifyPassword(oldPassword, admin.Password))
            {
                return ResultData.No("旧密码错误!");
            }

            admin.Password = PasswordUtil.EncryptPassword(newPassword);
            UpdateDiy.UpdateColumns(c => c.Password).SetSource(admin).ExecuteAffrows();

            return ResultData.Ok("修改密码成功!", GetAdminInfo(admin));
        }

        /// <summary>
        /// 获取管理员信息
        /// </summary>
        private object GetAdminInfo(Admin admin)
        {
            string homePath = _menuService.GetDefaultMenupathByUserId(admin.Id);
            return new
            {
                userId = admin.Id,
                admin.Id,
                admin.Username,
                RoleKey = admin.AdminRole?.RoleKey,
                RoleName = admin.AdminRole?.Name,
                Name = admin.Name,
                admin.Avatar,
                parentId = admin.ParentId,
                homePath = homePath,
                UserType = "Admin"
            };
        }

        /// <summary>
        /// 获取当前登录用户的权限菜单
        /// </summary>
        public object MenuAll()
        {
            var currentUser = GetCurrentAdmin();
            if (currentUser == null)
            {
                return ResultData.No("未登录或登录已过期");
            }

            int roleId = currentUser.RoleId;
            string cacheKey = $"menu:user:{currentUser.Id}:role:{roleId}";

            var cachedMenus = _cacheService.Get<List<VbenMenu>>(cacheKey);
            if (cachedMenus != null)
            {
                return ResultData.Ok(cachedMenus);
            }

            var allMenus = _fsql.Select<AdminRoleMenu>()
                .Where(jm => jm.RoleId == roleId)
                .ToList(jm => jm.MenuId);

            if (!allMenus.Any())
            {
                var emptyMenus = new List<VbenMenu>();
                _cacheService.Set(cacheKey, emptyMenus, TimeSpan.FromMinutes(30));
                return ResultData.Ok(emptyMenus);
            }

            var menus = _fsql.Select<Menu>()
                .Where(m => allMenus.Contains(m.Id))
                .ToList();

            var parentIds = menus
                .Where(m => m.ParentId > 0)
                .Select(m => m.ParentId)
                .Distinct()
                .ToList();

            var parentMenus = _fsql.Select<Menu>()
                .Where(m => parentIds.Contains(m.Id))
                .ToList();

            var allMenusWithParents = menus.Union(parentMenus)
                .GroupBy(m => m.Id)
                .Select(g => g.First())
                .OrderBy(m => m.OrderNo)
                .ToList();

            var topMenus = allMenusWithParents
                .Where(m => m.ParentId == 0)
                .OrderBy(m => m.OrderNo)
                .ToList();

            var menuList = topMenus.Select(parent =>
            {
                var children = allMenusWithParents
                    .Where(child => child.ParentId == parent.Id)
                    .OrderBy(child => child.OrderNo)
                    .ToList();

                return new VbenMenu(parent, children);
            }).ToList();

            _cacheService.Set(cacheKey, menuList, TimeSpan.FromMinutes(30));

            return ResultData.Ok(menuList);
        }

        #region 管理员 CRUD

        private Admin GetCurrentAdmin()
        {
            int currentId = _httpContextAccessor.HttpContext.User.Id();
            if (currentId <= 0) return null;
            return Select.Where(c => c.Id == currentId).First();
        }

        public ResultData GetAdminList(
            string username = "",
            string name = "",
            int? roleId = null,
            AdminStatus? status = null,
            string field = "",
            string order = "",
            int page = 1,
            int pageSize = 20)
        {
            var query = Select.Include(c => c.AdminRole);

            query = query.WhereIf(!string.IsNullOrEmpty(username), c => c.Username.Contains(username));
            query = query.WhereIf(!string.IsNullOrEmpty(name), c => c.Name.Contains(name));
            query = query.WhereIf(roleId.HasValue, c => c.RoleId == roleId.Value);
            query = query.WhereIf(status.HasValue, c => c.Status == status.Value);

            if (string.IsNullOrEmpty(field))
            {
                query = query.OrderByDescending(c => c.Id);
            }
            else
            {
                query = query.OrderByPropertyName(field, order == "ascend");
            }

            var data = query.Count(out long dataCount).Page(page, pageSize).ToList(
                c => new
                {
                    c.Id,
                    c.ParentId,
                    c.Username,
                    c.Name,
                    c.Avatar,
                    c.RoleId,
                    Role = c.AdminRole.Name,
                    c.Status,
                    c.CreatedTime,
                    c.LastOnlineTime
                }
            );

            return ResultPage.Ok(data, dataCount, page, pageSize, "数据查询成功");
        }

        public ResultData CreateAdmin(Admin admin)
        {
            var currentUser = GetCurrentAdmin();
            if (currentUser == null) return ResultData.No("授权失效!");

            if (string.IsNullOrEmpty(admin.Username)) return ResultData.No("用户名不能为空!");
            if (string.IsNullOrEmpty(admin.Password)) return ResultData.No("密码不能为空!");
            if (admin.Password.Length < 6) return ResultData.No("密码长度不能少于6位!");

            if (Select.Where(c => c.Username == admin.Username).Any())
            {
                return ResultData.No("用户名已存在!");
            }

            admin.CreatedTime = DateTime.Now;
            admin.LastOnlineTime = null;
            admin.IsDeleted = false;
            admin = this.Insert(admin);

            return ResultData.Ok("管理员添加成功!", new { admin.Id, admin.Username, admin.Name, admin.RoleId, admin.Status, admin.CreatedTime });
        }

        public ResultData EditAdmin(Admin admin)
        {
            var currentUser = GetCurrentAdmin();
            if (currentUser == null) return ResultData.No("授权失效!");

            var targetUser = Select.Where(c => c.Id == admin.Id).First();
            if (targetUser == null) return ResultData.No("管理员不存在!");
            if (targetUser.Id == 1) return ResultData.No("超级管理员不允许修改!");

            admin.ParentId = targetUser.ParentId;

            if (string.IsNullOrEmpty(admin.Username)) return ResultData.No("用户名不能为空!");

            if (Select.Where(c => c.Username == admin.Username && c.Id != admin.Id).Any())
            {
                return ResultData.No("用户名已存在!");
            }

            var updateDiy = UpdateDiy.UpdateColumns(c => new { c.Username, c.Name, c.Avatar, c.RoleId, c.ParentId, c.Status }).SetSource(admin);

            if (!string.IsNullOrEmpty(admin.Password) && admin.Password.Length >= 6)
            {
                admin.Password = PasswordUtil.EncryptPassword(admin.Password);
                updateDiy = UpdateDiy.UpdateColumns(c => new { c.Username, c.Password, c.Name, c.Avatar, c.RoleId, c.ParentId, c.Status }).SetSource(admin);
            }

            int count = updateDiy.ExecuteAffrows();
            if (count <= 0) return ResultData.No("管理员更新失败!");

            return ResultData.Ok("管理员更新成功!");
        }

        public ResultData DelAdmin(int id)
        {
            var currentUser = GetCurrentAdmin();
            if (currentUser == null) return ResultData.No("授权失效!");
            if (id == 1) return ResultData.No("超级管理员不允许删除!");
            if (id == currentUser.Id) return ResultData.No("不能删除自己!");

            try
            {
                return this.ExecuteInTransaction<Admin, ResultData>(uow =>
                {
                    Admin targetUser = this.Select.Where(c => c.Id == id).ForUpdate().First();
                    if (targetUser == null) return ResultData.No("管理员不存在,删除失败!");

                    targetUser.IsDeleted = true;
                    int count = this.UpdateDiy.UpdateColumns(c => c.IsDeleted).SetSource(targetUser).ExecuteAffrows();
                    if (count <= 0) throw new Exception("管理员伪删除更新数据为0条,逻辑异常!");

                    return ResultData.Ok("删除管理员成功!");
                }, _logger);
            }
            catch (Exception)
            {
                return ResultData.No("删除管理员失败，数据操作异常!");
            }
        }

        public ResultData GetAdminDetail(int id)
        {
            var currentUser = GetCurrentAdmin();
            if (currentUser == null) return ResultData.No("授权失效!");

            Admin admin = Select.Include(c => c.AdminRole).Where(c => c.Id == id).First();
            if (admin == null) return ResultData.No("管理员不存在!");

            return ResultData.Ok(new
            {
                admin.Id,
                admin.ParentId,
                admin.Username,
                admin.Name,
                admin.Avatar,
                admin.RoleId,
                Role = admin.AdminRole?.Name,
                admin.Status,
                admin.CreatedTime,
                admin.LastOnlineTime
            });
        }

        public ResultData ResetAdminPassword(int id, string newPassword)
        {
            var currentUser = GetCurrentAdmin();
            if (currentUser == null) return ResultData.No("授权失效!");
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6) return ResultData.No("新密码长度不能少于6位!");
            if (id == 1 && currentUser.Id != 1) return ResultData.No("无权限重置超级管理员密码!");

            var targetUser = Select.Where(c => c.Id == id).First();
            if (targetUser == null) return ResultData.No("管理员不存在!");

            targetUser.Password = PasswordUtil.EncryptPassword(newPassword);
            int count = UpdateDiy.UpdateColumns(c => c.Password).SetSource(targetUser).ExecuteAffrows();
            if (count <= 0) return ResultData.No("密码重置失败!");

            return ResultData.Ok("密码重置成功!");
        }

        public ResultData ToggleAdminStatus(int id)
        {
            var currentUser = GetCurrentAdmin();
            if (currentUser == null) return ResultData.No("授权失效!");
            if (id == 1) return ResultData.No("超级管理员状态不允许修改!");
            if (id == currentUser.Id) return ResultData.No("不能修改自己的状态!");

            var targetUser = Select.Where(c => c.Id == id).First();
            if (targetUser == null) return ResultData.No("管理员不存在!");

            targetUser.Status = targetUser.Status == AdminStatus.Active ? AdminStatus.Disabled : AdminStatus.Active;
            int count = UpdateDiy.UpdateColumns(c => c.Status).SetSource(targetUser).ExecuteAffrows();
            if (count <= 0) return ResultData.No("状态切换失败!");

            return ResultData.Ok("状态切换成功!", new { targetUser.Status });
        }

        #endregion
    }
}
