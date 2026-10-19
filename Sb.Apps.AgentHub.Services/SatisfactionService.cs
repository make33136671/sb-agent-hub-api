using Microsoft.AspNetCore.Http;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 满意度评价服务
    /// </summary>
    [AutoDependencyInjection]
    public class SatisfactionService
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly QuotaService _quotaService;
        private readonly MemberService _memberService;

        public SatisfactionService(IFreeSql fsql, IHttpContextAccessor httpContextAccessor,
            QuotaService quotaService, MemberService memberService)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _quotaService = quotaService;
            _memberService = memberService;
        }

        /// <summary>
        /// 提交评价（访客端）
        /// </summary>
        public ResultData SubmitRating(int ownerId, int platformId, string chatUserId,
            int agentId, int rating, string tags, string comment, string conversationId)
        {
            if (!_quotaService.CheckFeature(ownerId, "feature_satisfaction"))
                return ResultData.No("评价功能未启用!");

            if (rating < 1 || rating > 5)
                return ResultData.No("评分范围 1-5!");

            _fsql.Insert(new SatisfactionRating
            {
                OwnerId = ownerId,
                PlatformId = platformId,
                ChatUserId = chatUserId,
                AgentId = agentId,
                Rating = rating,
                Tags = tags ?? "",
                Comment = comment ?? "",
                ConversationId = conversationId ?? "",
                CreateTime = DateTime.Now
            }).ExecuteAffrows();

            return ResultData.Ok("评价提交成功!");
        }

        /// <summary>
        /// 获取评价列表（管理端）
        /// </summary>
        public ResultData GetRatingList(int? agentId = null, int page = 1, int pageSize = 20)
        {
            int ownerId = ResolveOwnerId();
            if (ownerId < 0) return ResultData.No("授权失效!");

            var query = _fsql.Select<SatisfactionRating>()
                .WhereIf(ownerId > 0, r => r.OwnerId == ownerId);

            if (agentId.HasValue)
                query = query.Where(r => r.AgentId == agentId.Value);

            var data = query.OrderByDescending(r => r.CreateTime)
                .Count(out long total)
                .Page(page, pageSize)
                .ToList(r => new
                {
                    r.Id, r.ChatUserId, r.AgentId, r.Rating,
                    r.Tags, r.Comment, r.ConversationId, CreateTime = r.CreateTime
                });

            return ResultPage.Ok(data, total, page, pageSize, "查询成功");
        }

        /// <summary>
        /// 获取评价统计（管理端）
        /// </summary>
        public ResultData GetStatistics(int? agentId = null)
        {
            int ownerId = ResolveOwnerId();
            if (ownerId < 0) return ResultData.No("授权失效!");

            var query = _fsql.Select<SatisfactionRating>()
                .WhereIf(ownerId > 0, r => r.OwnerId == ownerId);

            if (agentId.HasValue)
                query = query.Where(r => r.AgentId == agentId.Value);

            var allRatings = query.ToList(r => r.Rating);
            if (!allRatings.Any())
                return ResultData.Ok(new { total = 0, average = 0.0, distribution = new int[5] });

            var distribution = new int[5];
            foreach (var r in allRatings)
            {
                if (r >= 1 && r <= 5)
                    distribution[r - 1]++;
            }

            return ResultData.Ok(new
            {
                total = allRatings.Count,
                average = Math.Round(allRatings.Average(), 1),
                distribution
            });
        }

        #region 仓储访问入口（供其他 Service 调用）

        /// <summary>
        /// 获取站长的评价记录（供 DataExportService 导出用）
        /// </summary>
        public List<SatisfactionRating> GetRatingsForExport(int ownerId, DateTime? start, DateTime? end)
        {
            var query = _fsql.Select<SatisfactionRating>().Where(r => r.OwnerId == ownerId);

            if (start.HasValue)
                query = query.Where(r => r.CreateTime >= start.Value);
            if (end.HasValue)
                query = query.Where(r => r.CreateTime < end.Value.AddDays(1));

            return query.OrderByDescending(r => r.CreateTime).ToList();
        }

        #endregion

        /// <summary>
        /// 解析当前 ownerId：Admin 返回 0（不限制），Member 根据角色解析，否则返回 -1
        /// </summary>
        private int ResolveOwnerId()
        {
            var user = _httpContextAccessor.HttpContext!.User;
            string userType = user.UserType();
            if (userType == "Admin") return 0;

            int currentId = user.Id();
            if (currentId <= 0) return -1;

            var member = _memberService.GetById(currentId);
            if (member == null) return -1;

            return member.Role == MemberRole.Owner ? member.Id : (member.OwnerId ?? 0);
        }
    }
}
