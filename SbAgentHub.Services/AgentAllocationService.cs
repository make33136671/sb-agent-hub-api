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
        public AgentAllocationResult AllocateAgent(int ownerId)
        {
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

        #region 离线处理

        private AgentAllocationResult HandleOffline(CustomerServiceConfig config, int ownerId)
        {
            return config.OfflineHandling switch
            {
                OfflineHandling.AutoReply => new AgentAllocationResult
                {
                    Success = false,
                    Message = config.OfflineAutoReply ?? "客服暂时不在线，请稍后再试。"
                },
                OfflineHandling.TransferOther => TryTransferToAnyAgent(ownerId),
                _ => new AgentAllocationResult
                {
                    Success = false,
                    Message = "客服暂时不在线，请留言，我们将尽快回复。"
                }
            };
        }

        private AgentAllocationResult TryTransferToAnyAgent(int ownerId)
        {
            // 尝试分配给任何可用的客服（包括忙碌状态的）
            var agent = _fsql.Select<Member>()
                .Where(c => c.OwnerId == ownerId
                    && c.Role == MemberRole.Agent
                    && c.Status == UserStatus.Active
                    && c.OnlineStatus != OnlineStatus.Offline)
                .OrderBy(c => c.CurrentCount)
                .First();

            if (agent != null)
            {
                return new AgentAllocationResult
                {
                    Success = true,
                    AgentId = agent.Id,
                    AgentName = agent.Name ?? agent.Username,
                    AgentAvatar = agent.Avatar,
                    OpenImUserId = agent.OpenImUserId,
                    WelcomeMessage = agent.WelcomeMessage
                };
            }

            return new AgentAllocationResult
            {
                Success = false,
                Message = "当前没有可用的客服，请稍后再试。"
            };
        }

        #endregion
    }
}
