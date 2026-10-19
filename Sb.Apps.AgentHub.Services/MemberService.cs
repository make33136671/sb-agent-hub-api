using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FreeSql;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Core.Util;
using Sb.Apps.AgentHub.Models;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text.Json;

namespace Sb.Apps.AgentHub.Services
{
    [AutoDependencyInjection]
    public class MemberService : BaseCrudService<Member>
    {
        IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly QuotaPackageService _packageService;
        private readonly IServiceProvider _serviceProvider;

        // 懒加载打破循环依赖：MemberService→QuotaService/OrderService（两者亦反向注入 MemberService）
        private readonly Lazy<QuotaService> _quotaService;
        private readonly Lazy<OrderService> _orderService;

        public MemberService(IFreeSql fsql, ILogger<MemberService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            QuotaPackageService packageService,
            IServiceProvider serviceProvider) : base(fsql, logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _packageService = packageService;
            _serviceProvider = serviceProvider;
            _quotaService = new Lazy<QuotaService>(() => _serviceProvider.GetRequiredService<QuotaService>());
            _orderService = new Lazy<OrderService>(() => _serviceProvider.GetRequiredService<OrderService>());
        }

        public Member GetMemberById(int memberId)
        {
            return Select.Where(c => c.Id == memberId).First();
        }

        /// <summary>
        /// 按主键获取（仓储访问入口，供其他 Service 调用）
        /// </summary>
        public Member GetById(int id)
        {
            return Select.Where(c => c.Id == id).First();
        }

        /// <summary>
        /// 按用户名获取 Member（登录校验用）
        /// </summary>
        public Member GetByUsername(string username)
        {
            return Select.Where(c => c.Username == username).First();
        }

        /// <summary>
        /// 获取站长（校验为 Owner 角色），不存在返回 null
        /// </summary>
        public Member GetOwner(int memberId)
        {
            return Select.Where(c => c.Id == memberId && c.Role == MemberRole.Owner && !c.IsDeleted).First();
        }

        /// <summary>
        /// 获取第一个活跃站长（Admin 解析默认 Owner 用）
        /// </summary>
        public Member GetFirstActiveOwner()
        {
            return Select.Where(c => c.Role == MemberRole.Owner && c.Status == UserStatus.Active && !c.IsDeleted)
                .OrderBy(c => c.Id).First();
        }

        /// <summary>
        /// 获取所有活跃站长（供配额/平台下拉用）
        /// </summary>
        public List<Member> GetActiveOwners()
        {
            return Select.Where(c => c.Role == MemberRole.Owner && c.Status == UserStatus.Active && !c.IsDeleted)
                .ToList();
        }

        /// <summary>
        /// 按 Id 批量获取 Member 名称映射（Name 优先，回退 Username）
        /// </summary>
        public Dictionary<int, string> GetMemberNamesByIds(List<int> ids)
        {
            if (ids == null || !ids.Any())
                return new Dictionary<int, string>();

            return Select.Where(c => ids.Contains(c.Id))
                .ToDictionary(c => c.Id, c => c.Name ?? c.Username ?? "");
        }

        /// <summary>
        /// 统计站长下属客服数量（配额 max_agents 用）
        /// </summary>
        public int CountActiveAgentsByOwner(int ownerId)
        {
            return (int)Select.Where(c => c.OwnerId == ownerId && c.Role == MemberRole.Agent && !c.IsDeleted).Count();
        }

        /// <summary>
        /// 获取活跃客服（按 Id，校验为 Agent 角色 + Active 状态）
        /// </summary>
        public Member GetActiveAgent(int agentId)
        {
            return Select.Where(c => c.Id == agentId && c.Role == MemberRole.Agent && c.Status == UserStatus.Active)
                .First();
        }

        /// <summary>
        /// 按主键获取活跃 Member（不限角色，校验 Active 状态）
        /// </summary>
        public Member GetActiveById(int id)
        {
            return Select.Where(c => c.Id == id && c.Status == UserStatus.Active && !c.IsDeleted).First();
        }

        /// <summary>
        /// 在线 + 未满载的可用客服列表（分配服务用）
        /// </summary>
        public List<Member> GetAvailableAgents(int ownerId)
        {
            return Select.Where(c => c.OwnerId == ownerId
                    && c.Role == MemberRole.Agent
                    && c.Status == UserStatus.Active
                    && c.OnlineStatus == OnlineStatus.Online
                    && c.CurrentCount < c.MaxConcurrent
                    && !c.IsDeleted)
                .OrderBy(c => c.CurrentCount)
                .ToList();
        }

        /// <summary>
        /// 在线 + 未满载的站长（可作客服兜底）
        /// </summary>
        public Member GetOnlineOwnerUnderCapacity(int ownerId)
        {
            return Select.Where(c => c.Id == ownerId && c.Status == UserStatus.Active
                    && c.OnlineStatus == OnlineStatus.Online
                    && c.CurrentCount < c.MaxConcurrent && !c.IsDeleted).First();
        }

        /// <summary>
        /// 活跃站长（不论是否在线，离线兜底用）
        /// </summary>
        public Member GetActiveOwner(int ownerId)
        {
            return Select.Where(c => c.Id == ownerId && c.Status == UserStatus.Active && !c.IsDeleted).First();
        }

        /// <summary>
        /// 可转接的客服列表（同 owner，活跃且在线，排除自己）
        /// </summary>
        public List<Member> GetTransferableAgents(int ownerId, int excludeAgentId)
        {
            return Select.Where(c => c.Id != excludeAgentId
                    && (c.OwnerId == ownerId || c.Id == ownerId)
                    && c.Status == UserStatus.Active
                    && c.OnlineStatus != OnlineStatus.Offline
                    && !c.IsDeleted)
                .ToList();
        }

        /// <summary>
        /// 广播模式：在线活跃且有 RealtimeUserId 的客服（排除主客服）
        /// </summary>
        public List<string> GetOnlineAgentRealtimeUserIds(int ownerId, int excludeAgentId)
        {
            return Select.Where(c => (c.OwnerId == ownerId || c.Id == ownerId)
                    && c.Status == UserStatus.Active
                    && c.OnlineStatus != OnlineStatus.Offline
                    && !string.IsNullOrEmpty(c.RealtimeUserId)
                    && c.Id != excludeAgentId
                    && !c.IsDeleted)
                .ToList(c => c.RealtimeUserId);
        }

        /// <summary>
        /// 更新在线状态与最后在线时间
        /// </summary>
        public void UpdateOnlineStatus(int memberId, OnlineStatus status, DateTime lastOnlineTime)
        {
            UpdateDiy.Where(c => c.Id == memberId)
                .Set(c => c.OnlineStatus, status)
                .Set(c => c.LastOnlineTime, lastOnlineTime)
                .ExecuteAffrows();
        }

        /// <summary>
        /// 客服当前服务数 +1
        /// </summary>
        public void IncrementCurrentCount(int memberId)
        {
            UpdateDiy.Where(c => c.Id == memberId)
                .SetRaw("CurrentCount = CurrentCount + 1")
                .ExecuteAffrows();
        }

        /// <summary>
        /// 客服当前服务数 -1（不低于 0）
        /// </summary>
        public void DecrementCurrentCount(int memberId)
        {
            UpdateDiy.Where(c => c.Id == memberId && c.CurrentCount > 0)
                .SetRaw("CurrentCount = CurrentCount - 1")
                .ExecuteAffrows();
        }

        /// <summary>
        /// 更新 Realtime 用户ID
        /// </summary>
        public void UpdateRealtimeUserId(int memberId, string realtimeUserId)
        {
            UpdateDiy.Where(c => c.Id == memberId)
                .Set(c => c.RealtimeUserId, realtimeUserId)
                .ExecuteAffrows();
        }

        /// <summary>
        /// 通用列更新（供其他 Service 按需更新 Member 字段）
        /// </summary>
        public void UpdateColumns(Member source, Expression<Func<Member, object>> columns)
        {
            UpdateDiy.UpdateColumns(columns).SetSource(source).ExecuteAffrows();
        }

        public ResultData GetList(int? id = null, string? keyword = null, UserStatus? status = null,
            MemberRole? role = null, int? ownerId = null,
            string field = "", string order = "", int page = 1, int pageSize = 20)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            string userType = user?.UserType() ?? "";

            // Member 用户强制按 ownerId 过滤，防止跨站长查看
            if (userType == "Member")
            {
                string userRole = user!.Role();
                if (userRole == "Owner")
                    ownerId = user!.Id();
                else if (userRole == "Agent")
                {
                    var agent = Select.Where(c => c.Id == user!.Id()).First();
                    ownerId = agent?.OwnerId ?? 0;
                }
            }

            var query = Select;
            query = query.WhereIf(id.HasValue, c => c.Id == id);
            query = query.WhereIf(!string.IsNullOrEmpty(keyword),
                c => c.Username.Contains(keyword!) || c.Name.Contains(keyword!));
            query = query.WhereIf(status.HasValue, c => c.Status == status!.Value);
            query = query.WhereIf(role.HasValue, c => c.Role == role!.Value);
            query = query.WhereIf(ownerId.HasValue, c => c.OwnerId == ownerId!.Value);
            query = ApplySorting(query, field, order);

            var data = query.Count(out long dataCount).Page(page, pageSize).ToList(m => new
            {
                m.Id, m.Username, m.Name, m.Avatar, m.AgentNo, m.Role, m.OwnerId,
                m.OnlineStatus, m.MaxConcurrent, m.CurrentCount,
                m.Status, m.CreateTime, m.LastOnlineTime
            });

            return ResultPage.Ok(data, dataCount, page, pageSize, "数据查询成功");
        }

        /// <summary>
        /// 获取站长下属的客服列表
        /// </summary>
        public ResultData GetAgentsByOwner(int ownerId)
        {
            var agents = Select.Where(c => c.OwnerId == ownerId && c.Role == MemberRole.Agent)
                .ToList(c => new
                {
                    c.Id, c.Username, c.Name, c.Avatar, c.AgentNo,
                    c.OnlineStatus, c.MaxConcurrent, c.CurrentCount,
                    c.Status, c.CreateTime, c.LastOnlineTime
                });

            return ResultData.Ok(agents);
        }

        public ResultData Create(Member member)
        {
            var existing = Select.Where(c => c.Username == member.Username).First();
            if (existing != null)
                return ResultData.No($"用户名 {member.Username} 已存在");

            if (!string.IsNullOrEmpty(member.Password))
                member.Password = PasswordUtil.EncryptPassword(member.Password);

            // 防御性编程：OwnerId 由后端根据当前登录用户自动设置，前端不可操作
            var user = _httpContextAccessor.HttpContext?.User;
            string userType = user?.UserType() ?? "";
            string userRole = user?.Role() ?? "";

            if (userType == "Member")
            {
                // 站长/客服不能创建站长，只能创建客服
                if (member.Role == MemberRole.Owner)
                    return ResultData.No("无权创建站长账号!");

                // 强制创建的客服角色
                member.Role = MemberRole.Agent;

                // OwnerId 强制设为当前用户的 OwnerId
                if (userRole == "Owner")
                    member.OwnerId = user!.Id();
                else if (userRole == "Agent")
                {
                    var agent = Select.Where(c => c.Id == user!.Id()).First();
                    member.OwnerId = agent?.OwnerId;
                }
            }
            // Admin 操作：允许前端指定 Role 和 OwnerId

            member.CreateTime = DateTime.Now;
            member = Insert(member);

            // 站长创建时自动分配试用套餐
            if (member.Role == MemberRole.Owner)
            {
                AssignTrialPackage(member.Id);
            }

            return ResultData.Ok("创建成功", member);
        }

        public ResultData Edit(Member member)
        {
            var existing = Select.Where(c => c.Id == member.Id).First();
            if (existing == null)
                return ResultData.No("用户不存在");

            existing.Name = member.Name;
            existing.Avatar = member.Avatar;
            existing.AgentNo = member.AgentNo;
            existing.Status = member.Status;
            existing.MaxConcurrent = member.MaxConcurrent;
            existing.WelcomeMessage = member.WelcomeMessage;
            // OwnerId 不可通过编辑修改（防御性编程）

            Update(existing);
            return ResultData.Ok("更新成功");
        }

        /// <summary>
        /// 重置登录密码
        /// </summary>
        public ResultData ResetPassword(int id, string newPassword)
        {
            var member = Select.Where(c => c.Id == id).First();
            if (member == null)
                return ResultData.No("用户不存在");

            member.Password = PasswordUtil.EncryptPassword(newPassword);
            UpdateDiy.UpdateColumns(c => c.Password).SetSource(member).ExecuteAffrows();
            return ResultData.Ok("密码已重置");
        }

        /// <summary>
        /// 公开接口：通过站长Id和客服工号查询客服基本信息
        /// </summary>
        public ResultData GetAgentPublicInfo(int ownerId, string agentNo)
        {
            var agent = Select.Where(c =>
                c.AgentNo == agentNo
                && c.Status == UserStatus.Active
                && c.OwnerId == ownerId
                && c.Role == MemberRole.Agent)
                .First();

            if (agent == null)
                return ResultData.No("客服不存在");

            return ResultData.Ok(new
            {
                agent.AgentNo,
                agent.Id,
                agent.Name,
                Avatar = agent.Avatar.ToAbsoluteUrl(_httpContextAccessor.HttpContext!)
            });
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        public ResultData ToggleStatus(int id)
        {
            var member = Select.Where(c => c.Id == id).First();
            if (member == null)
                return ResultData.No("用户不存在");

            member.Status = member.Status == UserStatus.Active ? UserStatus.Banned : UserStatus.Active;

            // 使用原生 SQL 避免 FreeSql MapType=string 在 UpdateColumns 中的兼容问题
            var newStatus = member.Status.ToString();
            var rowsAffected = _fsql.Ado.ExecuteNonQuery(
                $"UPDATE \"sb_agenthub_member\" SET \"Status\" = '{newStatus}' WHERE \"Id\" = {id} AND \"IsDeleted\" = false");

            if (rowsAffected > 0)
                return ResultData.Ok("状态切换成功", new { member.Status });
            return ResultData.No("状态切换失败");
        }

        /// <summary>
        /// 为新站长分配试用套餐 — 事务包裹配额+订单
        /// </summary>
        private void AssignTrialPackage(int memberId)
        {
            try
            {
                int trialDays = int.TryParse(_configuration["Trial:Days"], out var d) ? d : 7;
                string trialPackageKey = _configuration["Trial:PackageKey"] ?? "basic";

                var package = _packageService.GetEnabledByPackageKey(trialPackageKey);
                if (package == null) return;

                Dictionary<string, int> settings;
                try
                {
                    settings = JsonSerializer.Deserialize<Dictionary<string, int>>(package.QuotaSettings)
                        ?? new Dictionary<string, int>();
                }
                catch { return; }

                var expiresAt = DateTime.Now.AddDays(trialDays);
                var remark = $"试用套餐: {package.PackageName} ({trialDays}天)";

                using var uow = _fsql.CreateUnitOfWork();
                var tx = uow.GetOrBeginTransaction();
                try
                {
                    foreach (var kv in settings)
                    {
                        _quotaService.Value.UpsertMemberQuota(memberId, kv.Key, kv.Value,
                            "System", expiresAt, remark, tx);
                    }

                    _orderService.Value.InsertOrder(new Order
                    {
                        OrderNo = $"TRIAL{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
                        MemberId = memberId,
                        PackageId = package.Id,
                        PackageName = package.PackageName,
                        OrderType = Models.OrderType.NewPurchase,
                        Amount = 0,
                        Currency = "CNY",
                        Status = OrderStatus.Paid,
                        PaymentMethod = "trial",
                        PaymentTime = DateTime.Now,
                        EffectiveStartTime = DateTime.Now,
                        EffectiveEndTime = expiresAt,
                        Remark = remark,
                        CreateTime = DateTime.Now
                    }, tx);

                    uow.Commit();
                }
                catch
                {
                    uow.Rollback();
                    throw;
                }
            }
            catch
            {
            }
        }
    }
}
