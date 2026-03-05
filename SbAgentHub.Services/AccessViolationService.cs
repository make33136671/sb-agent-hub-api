using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Models;
using System;
using System.Linq;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 非法接入日志服务
    /// </summary>
    [AutoDependencyInjection]
    public class AccessViolationService
    {
        private readonly IFreeSql _fsql;
        private readonly ILogger<AccessViolationService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessViolationService(IFreeSql fsql, ILogger<AccessViolationService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _fsql = fsql;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 记录非法接入
        /// </summary>
        public void Log(int platformId, string platformKey, int ownerId,
            string requestDomain, string requestSource, string expectedSource,
            string reason, string ip = "", string ua = "", string requestUrl = "")
        {
            try
            {
                _fsql.Insert(new AccessViolationLog
                {
                    PlatformId = platformId,
                    PlatformKey = platformKey,
                    OwnerId = ownerId,
                    RequestDomain = requestDomain ?? "",
                    RequestSource = requestSource ?? "",
                    ExpectedSource = expectedSource ?? "",
                    Reason = reason,
                    IpAddress = ip,
                    UserAgent = ua?.Length > 500 ? ua.Substring(0, 500) : ua ?? "",
                    RequestUrl = requestUrl?.Length > 500 ? requestUrl.Substring(0, 500) : requestUrl ?? "",
                    CreateTime = DateTime.Now
                }).ExecuteAffrows();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录非法接入日志失败");
            }
        }

        /// <summary>
        /// 查询非法接入列表
        /// </summary>
        public ResultData GetList(int? ownerId = null, int? platformId = null,
            string reason = "", DateTime? startDate = null, DateTime? endDate = null,
            string field = "", string order = "", int page = 1, int pageSize = 20)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            string userType = user?.UserType();
            string role = user?.Role();

            var query = _fsql.Select<AccessViolationLog>();

            // Owner 只能看自己平台的记录
            if (userType == "Member" && role == "Owner")
            {
                int currentUserId = user.Id();
                query = query.Where(c => c.OwnerId == currentUserId);
            }

            query = query.WhereIf(ownerId.HasValue, c => c.OwnerId == ownerId.Value);
            query = query.WhereIf(platformId.HasValue, c => c.PlatformId == platformId.Value);
            query = query.WhereIf(!string.IsNullOrEmpty(reason), c => c.Reason == reason);
            query = query.WhereIf(startDate.HasValue, c => c.CreateTime >= startDate.Value);
            query = query.WhereIf(endDate.HasValue, c => c.CreateTime <= endDate.Value);

            if (!string.IsNullOrEmpty(field))
                query = query.OrderByPropertyName(field, order == "ascend");
            else
                query = query.OrderByDescending(c => c.Id);

            var data = query.Count(out long dataCount).Page(page, pageSize).ToList(c => new
            {
                c.Id,
                c.PlatformId,
                c.PlatformKey,
                c.OwnerId,
                c.RequestDomain,
                c.RequestSource,
                c.ExpectedSource,
                c.Reason,
                c.IpAddress,
                c.RequestUrl,
                c.CreateTime
            });

            // 批量查询平台名称
            var platformIds = data.Select(d => d.PlatformId).Distinct().ToList();
            var platformNames = platformIds.Any()
                ? _fsql.Select<ChatPlatform>().Where(p => platformIds.Contains(p.Id))
                    .ToDictionary(p => p.Id, p => p.PlatformName)
                : new System.Collections.Generic.Dictionary<int, string>();

            var result = data.Select(d => new
            {
                d.Id,
                d.PlatformId,
                PlatformName = platformNames.ContainsKey(d.PlatformId) ? platformNames[d.PlatformId] : "",
                d.PlatformKey,
                d.OwnerId,
                d.RequestDomain,
                d.RequestSource,
                d.ExpectedSource,
                d.Reason,
                ReasonText = GetReasonText(d.Reason),
                d.IpAddress,
                d.RequestUrl,
                d.CreateTime
            });

            return ResultPage.Ok(result, dataCount, page, pageSize, "查询成功");
        }

        /// <summary>
        /// 统计概览
        /// </summary>
        public ResultData GetStats(int? ownerId = null, int days = 7)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            string userType = user?.UserType();
            string role = user?.Role();

            var startDate = DateTime.Now.AddDays(-days);

            var query = _fsql.Select<AccessViolationLog>()
                .Where(c => c.CreateTime >= startDate);

            if (userType == "Member" && role == "Owner")
                query = query.Where(c => c.OwnerId == user.Id());
            else if (ownerId.HasValue)
                query = query.Where(c => c.OwnerId == ownerId.Value);

            var stats = query.GroupBy(c => c.Reason)
                .ToList(g => new
                {
                    Reason = g.Key,
                    Count = g.Count()
                });

            var total = stats.Sum(s => s.Count);

            return ResultData.Ok(new
            {
                Days = days,
                Total = total,
                Details = stats.Select(s => new
                {
                    s.Reason,
                    ReasonText = GetReasonText(s.Reason),
                    s.Count
                })
            });
        }

        private static string GetReasonText(string reason)
        {
            return reason switch
            {
                "domain_mismatch" => "域名不匹配",
                "type_mismatch" => "接入类型不匹配",
                "platform_disabled" => "平台已停用",
                "quota_exceeded" => "配额超限",
                _ => reason
            };
        }
    }
}
