using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;
using System.Text.RegularExpressions;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 自动回复服务
    /// </summary>
    [AutoDependencyInjection]
    public class AutoReplyService
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly QuotaService _quotaService;

        public AutoReplyService(IFreeSql fsql, IHttpContextAccessor httpContextAccessor,
            QuotaService quotaService)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _quotaService = quotaService;
        }

        /// <summary>
        /// 获取规则列表（站长端）
        /// </summary>
        public ResultData GetRuleList(int? platformId = null, TriggerType? triggerType = null)
        {
            int ownerId = ResolveOwnerId();
            if (ownerId < 0) return ResultData.No("授权失效!");

            var query = _fsql.Select<AutoReply>()
                .WhereIf(ownerId > 0, r => r.OwnerId == ownerId);

            if (platformId.HasValue)
                query = query.Where(r => r.PlatformId == platformId.Value);
            if (triggerType.HasValue)
                query = query.Where(r => r.TriggerType == triggerType.Value);

            var list = query.OrderByDescending(r => r.Priority)
                .OrderByDescending(r => r.CreateTime)
                .ToList(r => new
                {
                    r.Id, r.OwnerId, r.PlatformId, r.TriggerType,
                    r.Keywords, r.MatchType, r.ReplyContent, r.ReplyType,
                    r.Priority, r.IsEnabled, CreateTime = r.CreateTime
                });

            return ResultData.Ok(list);
        }

        /// <summary>
        /// 创建规则
        /// </summary>
        public ResultData CreateRule(int platformId, TriggerType triggerType, string keywords,
            Models.MatchType matchType, string replyContent, ReplyType replyType, int priority)
        {
            int ownerId = ResolveOwnerId();
            if (ownerId <= 0) return ResultData.No("授权失效!");

            if (!_quotaService.CheckFeature(ownerId, "feature_auto_reply"))
                return ResultData.No("未开通「自动回复」功能!");

            _fsql.Insert(new AutoReply
            {
                OwnerId = ownerId,
                PlatformId = platformId,
                TriggerType = triggerType,
                Keywords = keywords,
                MatchType = matchType,
                ReplyContent = replyContent,
                ReplyType = replyType,
                Priority = priority,
                IsEnabled = true,
                CreateTime = DateTime.Now
            }).ExecuteAffrows();

            return ResultData.Ok("创建成功!");
        }

        /// <summary>
        /// 更新规则
        /// </summary>
        public ResultData UpdateRule(int id, TriggerType triggerType, string keywords,
            Models.MatchType matchType, string replyContent, ReplyType replyType, int priority, bool isEnabled)
        {
            int ownerId = ResolveOwnerId();
            if (ownerId < 0) return ResultData.No("授权失效!");

            _fsql.Update<AutoReply>()
                .Where(r => r.Id == id)
                .WhereIf(ownerId > 0, r => r.OwnerId == ownerId)
                .Set(r => r.TriggerType, triggerType)
                .Set(r => r.Keywords, keywords)
                .Set(r => r.MatchType, matchType)
                .Set(r => r.ReplyContent, replyContent)
                .Set(r => r.ReplyType, replyType)
                .Set(r => r.Priority, priority)
                .Set(r => r.IsEnabled, isEnabled)
                .Set(r => r.UpdateTime, DateTime.Now)
                .ExecuteAffrows();

            return ResultData.Ok("更新成功!");
        }

        /// <summary>
        /// 删除规则
        /// </summary>
        public ResultData DeleteRule(int id)
        {
            int ownerId = ResolveOwnerId();
            if (ownerId < 0) return ResultData.No("授权失效!");

            _fsql.Update<AutoReply>()
                .Where(r => r.Id == id)
                .WhereIf(ownerId > 0, r => r.OwnerId == ownerId)
                .Set(r => r.IsDeleted, true)
                .ExecuteAffrows();

            return ResultData.Ok("删除成功!");
        }

        /// <summary>
        /// 匹配自动回复（核心逻辑）
        /// </summary>
        /// <param name="ownerId">站长ID</param>
        /// <param name="platformId">平台ID</param>
        /// <param name="triggerType">触发类型</param>
        /// <param name="messageText">消息文本（关键词匹配时使用）</param>
        /// <returns>匹配到的回复内容，null 表示无匹配</returns>
        public AutoReply? MatchReply(int ownerId, int platformId, TriggerType triggerType, string messageText = "")
        {
            if (!_quotaService.CheckFeature(ownerId, "feature_auto_reply"))
                return null;

            // 查找启用的规则，优先平台级，再全局
            var rules = _fsql.Select<AutoReply>()
                .Where(r => r.OwnerId == ownerId && r.IsEnabled
                    && r.TriggerType == triggerType
                    && (r.PlatformId == platformId || r.PlatformId == 0))
                .OrderByDescending(r => r.Priority)
                .ToList();

            foreach (var rule in rules)
            {
                if (triggerType == Models.TriggerType.Keyword)
                {
                    if (MatchKeyword(rule, messageText))
                        return rule;
                }
                else
                {
                    // 首次接入、无客服在线 直接返回第一条
                    return rule;
                }
            }

            return null;
        }

        /// <summary>
        /// 解析当前 ownerId：Admin 返回 0（不限制），Owner 返回自身 Id，否则返回 -1
        /// </summary>
        private int ResolveOwnerId()
        {
            var user = _httpContextAccessor.HttpContext!.User;
            string userType = user.UserType();
            if (userType == "Admin") return 0;
            int id = user.Id();
            return id > 0 ? id : -1;
        }

        /// <summary>
        /// 关键词匹配
        /// </summary>
        private bool MatchKeyword(AutoReply rule, string messageText)
        {
            if (string.IsNullOrEmpty(messageText) || string.IsNullOrEmpty(rule.Keywords))
                return false;

            var keywords = rule.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var keyword in keywords)
            {
                var kw = keyword.Trim();
                if (string.IsNullOrEmpty(kw)) continue;

                switch (rule.MatchType)
                {
                    case Models.MatchType.Contains:
                        if (messageText.Contains(kw, StringComparison.OrdinalIgnoreCase))
                            return true;
                        break;
                    case Models.MatchType.Exact:
                        if (messageText.Equals(kw, StringComparison.OrdinalIgnoreCase))
                            return true;
                        break;
                    case Models.MatchType.Regex:
                        try
                        {
                            if (Regex.IsMatch(messageText, kw, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1)))
                                return true;
                        }
                        catch { /* ignore invalid regex */ }
                        break;
                }
            }

            return false;
        }
    }
}
