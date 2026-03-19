using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FreeSql;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Core.Util;
using SbAgentHub.Models;
using System.Text.Json;

namespace SbAgentHub.Services
{
    [AutoDependencyInjection]
    public class MemberService : BaseCrudService<Member>
    {
        IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public MemberService(IFreeSql fsql, ILogger<MemberService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration) : base(fsql, logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public Member GetMemberById(int memberId)
        {
            return Select.Where(c => c.Id == memberId).First();
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
                    ownerId = user.Id();
                else if (userRole == "Agent")
                {
                    var agent = Select.Where(c => c.Id == user.Id()).First();
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
                m.Status, m.CreatedTime, m.LastOnlineTime
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
                    c.Status, c.CreatedTime, c.LastOnlineTime
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

            // 客服角色：自动设置 OwnerId 为当前登录站长的 ID
            if (member.Role == MemberRole.Agent && (member.OwnerId == null || member.OwnerId <= 0))
            {
                var user = _httpContextAccessor.HttpContext?.User;
                string userType = user?.UserType() ?? "";
                if (userType == "Member")
                {
                    string userRole = user!.Role();
                    if (userRole == "Owner")
                        member.OwnerId = user.Id();
                    else if (userRole == "Agent")
                    {
                        var agent = Select.Where(c => c.Id == user.Id()).First();
                        member.OwnerId = agent?.OwnerId;
                    }
                }
            }

            member.CreatedTime = DateTime.Now;
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
                agent.Avatar
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
            UpdateDiy.UpdateColumns(c => c.Status).SetSource(member).ExecuteAffrows();
            return ResultData.Ok("状态切换成功", new { member.Status });
        }

        /// <summary>
        /// 为新站长分配试用套餐
        /// </summary>
        private void AssignTrialPackage(int memberId)
        {
            try
            {
                int trialDays = int.TryParse(_configuration["Trial:Days"], out var d) ? d : 7;
                string trialPackageKey = _configuration["Trial:PackageKey"] ?? "basic";

                var package = _fsql.Select<QuotaPackage>()
                    .Where(p => p.PackageKey == trialPackageKey && p.IsEnabled)
                    .First();
                if (package == null) return;

                // 解析套餐配额
                Dictionary<string, int> settings;
                try
                {
                    settings = JsonSerializer.Deserialize<Dictionary<string, int>>(package.QuotaSettings)
                        ?? new Dictionary<string, int>();
                }
                catch { return; }

                var expiresAt = DateTime.Now.AddDays(trialDays);
                var remark = $"试用套餐: {package.PackageName} ({trialDays}天)";

                foreach (var kv in settings)
                {
                    _fsql.Insert(new MemberQuota
                    {
                        MemberId = memberId,
                        QuotaKey = kv.Key,
                        QuotaValue = kv.Value,
                        GrantedByType = "System",
                        ExpiresAt = expiresAt,
                        Remark = remark,
                        CreateTime = DateTime.Now,
                        IsDeleted = false
                    }).ExecuteAffrows();
                }

                // 创建试用订单记录
                _fsql.Insert(new Order
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
                }).ExecuteAffrows();
            }
            catch
            {
                // 试用分配失败不影响创建
            }
        }
    }
}
