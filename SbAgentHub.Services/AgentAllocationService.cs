using FreeSql;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Models;
using SbAgentHub.Models.Dto;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 客服分配服务 — 根据配置规则分配客服
    /// </summary>
    [AutoDependencyInjection]
    public class AgentAllocationService
    {
        private readonly IFreeSql _fsql;
        private readonly ILogger<AgentAllocationService> _logger;
        private readonly CustomerServiceConfigService _configService;

        // 轮询计数器（内存级别，重启后重置）
        private static int _roundRobinIndex = 0;
        private static readonly object _lockObj = new();

        public AgentAllocationService(
            IFreeSql fsql,
            ILogger<AgentAllocationService> logger,
            CustomerServiceConfigService configService)
        {
            _fsql = fsql;
            _logger = logger;
            _configService = configService;
        }

        /// <summary>
        /// 为访客分配客服
        /// </summary>
        public AgentAllocationResult AllocateAgent(int ownerId, int platformId = 0)
        {
            // 检查平台是否开启管理员接管
            if (platformId > 0)
            {
                var platform = _fsql.Select<ChatPlatform>()
                    .Where(c => c.Id == platformId)
                    .First();

                if (platform != null && platform.IsAdminTakeover && platform.TakeoverAdminId.HasValue)
                {
                    var admin = _fsql.Select<Admin>()
                        .Where(a => a.Id == platform.TakeoverAdminId.Value && a.Status == AdminStatus.Active)
                        .First();

                    if (admin != null)
                    {
                        _logger.LogInformation("管理员接管分配: PlatformId={PlatformId}, AdminId={AdminId}",
                            platformId, admin.Id);

                        return new AgentAllocationResult
                        {
                            Success = true,
                            AgentId = admin.Id,
                            AgentName = admin.Name ?? admin.Username,
                            AgentAvatar = admin.Avatar,
                            WelcomeMessage = null
                        };
                    }
                }
            }

            var config = _configService.GetEffectiveConfig(ownerId);

            // 检查工作时间
            if (config.EnableWorkTime && !IsWithinWorkingHours(config))
            {
                return new AgentAllocationResult
                {
                    Success = false,
                    Message = config.NonWorkTimeMessage ?? "当前为非工作时间"
                };
            }

            // 获取可用客服列表（在线 + 未满载）
            var availableAgents = _fsql.Select<Member>()
                .Where(c => c.OwnerId == ownerId
                    && c.Role == MemberRole.Agent
                    && c.Status == UserStatus.Active
                    && c.OnlineStatus == OnlineStatus.Online
                    && c.CurrentCount < c.MaxConcurrent)
                .OrderBy(c => c.CurrentCount)
                .ToList();

            // 站长自己也可作为客服
            var owner = _fsql.Select<Member>()
                .Where(c => c.Id == ownerId && c.Status == UserStatus.Active
                    && c.OnlineStatus == OnlineStatus.Online
                    && c.CurrentCount < c.MaxConcurrent)
                .First();

            if (owner != null)
            {
                availableAgents.Add(owner);
            }

            if (!availableAgents.Any())
            {
                // 没有在线客服，按离线处理策略
                return HandleOffline(config, ownerId);
            }

            // 按分配规则选择客服
            Member selectedAgent = config.AllocationRule switch
            {
                AllocationRule.LeastBusy => AllocateLeastBusy(availableAgents),
                AllocationRule.Random => AllocateRandom(availableAgents),
                AllocationRule.Fixed => AllocateFixed(config, availableAgents, ownerId),
                _ => AllocateRoundRobin(availableAgents)  // 默认轮询
            };

            if (selectedAgent == null)
            {
                return HandleOffline(config, ownerId);
            }

            _logger.LogInformation("客服分配成功: OwnerId={OwnerId}, AgentId={AgentId}, Rule={Rule}",
                ownerId, selectedAgent.Id, config.AllocationRule);

            return new AgentAllocationResult
            {
                Success = true,
                AgentOnline = true,
                AgentId = selectedAgent.Id,
                AgentName = selectedAgent.Name ?? selectedAgent.Username,
                AgentAvatar = selectedAgent.Avatar,
                OpenImUserId = selectedAgent.OpenImUserId,
                WelcomeMessage = selectedAgent.WelcomeMessage
            };
        }

        /// <summary>
        /// 检查是否在工作时间内
        /// </summary>
        public bool IsWithinWorkingHours(CustomerServiceConfig config)
        {
            if (!config.EnableWorkTime) return true;

            var now = DateTime.Now.TimeOfDay;

            if (TimeSpan.TryParse(config.WorkTimeStart, out var start)
                && TimeSpan.TryParse(config.WorkTimeEnd, out var end))
            {
                return now >= start && now <= end;
            }

            return true; // 解析失败默认不限制
        }

        #region 分配策略

        private Member AllocateRoundRobin(List<Member> agents)
        {
            lock (_lockObj)
            {
                _roundRobinIndex = _roundRobinIndex % agents.Count;
                var agent = agents[_roundRobinIndex];
                _roundRobinIndex++;
                return agent;
            }
        }

        private Member AllocateLeastBusy(List<Member> agents)
        {
            return agents.OrderBy(a => a.CurrentCount).First();
        }

        private Member AllocateRandom(List<Member> agents)
        {
            var random = new Random();
            return agents[random.Next(agents.Count)];
        }

        private Member AllocateFixed(CustomerServiceConfig config, List<Member> agents, int ownerId)
        {
            // 固定分配：优先分配给站长自己
            var owner = agents.FirstOrDefault(a => a.Id == ownerId);
            return owner ?? agents.FirstOrDefault();
        }

        #endregion

        #region 离线处理 — 消息滑落

        /// <summary>
        /// 没有在线客服时的滑落逻辑：
        /// 1. 站长在线且未满载 → 分配给站长
        /// 2. 站长也不在线 → 仍分配给站长（离线也分配，消息会缓存）
        /// 永不自动滑落到超级管理员（Admin），除非 IsAdminTakeover
        /// </summary>
        private AgentAllocationResult HandleOffline(CustomerServiceConfig config, int ownerId)
        {
            // 尝试滑落到站长
            var owner = _fsql.Select<Member>()
                .Where(c => c.Id == ownerId && c.Status == UserStatus.Active)
                .First();

            if (owner != null)
            {
                bool ownerOnline = owner.OnlineStatus != OnlineStatus.Offline
                    && owner.CurrentCount < owner.MaxConcurrent;

                if (ownerOnline)
                {
                    _logger.LogInformation("客服全部离线，滑落到站长(在线): OwnerId={OwnerId}", ownerId);
                    return new AgentAllocationResult
                    {
                        Success = true,
                        AgentOnline = true,
                        AgentId = owner.Id,
                        AgentName = owner.Name ?? owner.Username,
                        AgentAvatar = owner.Avatar,
                        OpenImUserId = owner.OpenImUserId,
                        WelcomeMessage = owner.WelcomeMessage
                    };
                }

                // 站长也不在线 → 仍然分配给站长，标记离线
                _logger.LogInformation("客服和站长均离线，分配给站长(离线): OwnerId={OwnerId}", ownerId);
                return new AgentAllocationResult
                {
                    Success = true,
                    AgentOnline = false,
                    AgentId = owner.Id,
                    AgentName = owner.Name ?? owner.Username,
                    AgentAvatar = owner.Avatar,
                    OpenImUserId = owner.OpenImUserId,
                    WelcomeMessage = owner.WelcomeMessage,
                    Message = config.OfflineAutoReply ?? "客服暂时不在线，消息已送达，我们会尽快回复。"
                };
            }

            // 兜底：找不到站长
            return new AgentAllocationResult
            {
                Success = false,
                AgentOnline = false,
                Message = "当前没有可用的客服，请稍后再试。"
            };
        }

        #endregion
    }
}
