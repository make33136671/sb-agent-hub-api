using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 待审核域名服务
    /// </summary>
    [AutoDependencyInjection]
    public class PendingDomainService
    {
        private readonly IFreeSql _fsql;
        private readonly ILogger<PendingDomainService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        // 懒加载打破循环依赖：PendingDomainService→ChatPlatformService（ChatPlatformService 反向注入 PendingDomainService）
        private readonly Lazy<ChatPlatformService> _platformService;

        public PendingDomainService(IFreeSql fsql, ILogger<PendingDomainService> logger,
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider)
        {
            _fsql = fsql;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
            _platformService = new Lazy<ChatPlatformService>(() => _serviceProvider.GetRequiredService<ChatPlatformService>());
        }

        /// <summary>
        /// 记录待审核域名（域名不匹配时调用）
        /// </summary>
        public void AddOrUpdate(int platformId, int ownerId, string domain, string ip = "", string ua = "")
        {
            try
            {
                domain = NormalizeDomain(domain);
                if (string.IsNullOrEmpty(domain)) return;

                var existing = _fsql.Select<PendingDomain>()
                    .Where(p => p.PlatformId == platformId && p.Domain == domain)
                    .First();

                if (existing != null)
                {
                    // 已存在：更新计数和时间（已拒绝的也更新，让管理员知道还有请求）
                    existing.RequestCount++;
                    existing.LastSeenTime = DateTime.Now;
                    existing.LastIpAddress = ip ?? "";
                    existing.LastUserAgent = ua?.Length > 500 ? ua.Substring(0, 500) : ua ?? "";

                    // 如果之前已批准，不再改为 Pending
                    if (existing.Status != PendingDomainStatus.Approved)
                        existing.Status = PendingDomainStatus.Pending;

                    _fsql.Update<PendingDomain>().SetSource(existing)
                        .UpdateColumns(c => new { c.RequestCount, c.LastSeenTime, c.LastIpAddress, c.LastUserAgent, c.Status })
                        .ExecuteAffrows();
                }
                else
                {
                    _fsql.Insert(new PendingDomain
                    {
                        PlatformId = platformId,
                        OwnerId = ownerId,
                        Domain = domain,
                        RequestCount = 1,
                        LastIpAddress = ip ?? "",
                        LastUserAgent = ua?.Length > 500 ? ua.Substring(0, 500) : ua ?? "",
                        Status = PendingDomainStatus.Pending,
                        FirstSeenTime = DateTime.Now,
                        LastSeenTime = DateTime.Now
                    }).ExecuteAffrows();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录待审核域名失败: PlatformId={PlatformId}, Domain={Domain}", platformId, domain);
            }
        }

        /// <summary>
        /// 获取指定平台的待审核域名列表
        /// </summary>
        public ResultData GetListByPlatform(int platformId)
        {
            var data = _fsql.Select<PendingDomain>()
                .Where(p => p.PlatformId == platformId && p.Status == PendingDomainStatus.Pending)
                .OrderByDescending(p => p.LastSeenTime)
                .ToList(p => new
                {
                    p.Id,
                    p.Domain,
                    p.RequestCount,
                    p.LastIpAddress,
                    p.FirstSeenTime,
                    p.LastSeenTime
                });

            return ResultData.Ok(data);
        }

        /// <summary>
        /// 获取当前用户所有平台的待审核域名（含平台信息）
        /// </summary>
        public ResultData GetPendingList(int? platformId = null, string field = "", string order = "",
            int page = 1, int pageSize = 20)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            string? userType = user?.UserType();
            string? role = user?.Role();

            var query = _fsql.Select<PendingDomain>()
                .Where(p => p.Status == PendingDomainStatus.Pending);

            if (userType == "Member" && role == "Owner" && user != null)
                query = query.Where(p => p.OwnerId == user.Id());

            query = query.WhereIf(platformId.HasValue, p => p.PlatformId == platformId!.Value);

            if (!string.IsNullOrEmpty(field))
                query = query.OrderByPropertyName(field, order == "ascend");
            else
                query = query.OrderByDescending(p => p.LastSeenTime);

            var data = query.Count(out long dataCount).Page(page, pageSize).ToList(p => new
            {
                p.Id,
                p.PlatformId,
                p.OwnerId,
                p.Domain,
                p.RequestCount,
                p.LastIpAddress,
                p.FirstSeenTime,
                p.LastSeenTime
            });

            // 批量查询平台名称
            var platformIds = data.Select(d => d.PlatformId).Distinct().ToList();
            var platformNames = _platformService.Value.GetPlatformNamesByIds(platformIds);

            var result = data.Select(d => new
            {
                d.Id,
                d.PlatformId,
                PlatformName = platformNames.ContainsKey(d.PlatformId) ? platformNames[d.PlatformId] : "",
                d.Domain,
                d.RequestCount,
                d.LastIpAddress,
                d.FirstSeenTime,
                d.LastSeenTime
            });

            return ResultPage.Ok(result, dataCount, page, pageSize, "查询成功");
        }

        /// <summary>
        /// 批准域名（加入平台白名单）
        /// </summary>
        public ResultData Approve(int id)
        {
            var pending = _fsql.Select<PendingDomain>().Where(p => p.Id == id).First();
            if (pending == null) return ResultData.No("记录不存在!");

            if (!HasOwnership(pending))
                return ResultData.No("无权操作!");

            var platform = _platformService.Value.GetById(pending.PlatformId);
            if (platform == null) return ResultData.No("平台不存在!");

            // 将域名加入白名单
            var domains = ParseAllowedDomains(platform.AllowedDomains);
            var normalizedDomain = NormalizeDomain(pending.Domain);

            if (!domains.Any(d => d.Equals(normalizedDomain, StringComparison.OrdinalIgnoreCase)))
            {
                domains.Add(normalizedDomain);
                var newAllowedDomains = JsonSerializer.Serialize(domains);
                _platformService.Value.UpdateAllowedDomains(platform.Id, newAllowedDomains);
            }

            // 更新状态
            pending.Status = PendingDomainStatus.Approved;
            _fsql.Update<PendingDomain>().SetSource(pending)
                .UpdateColumns(c => c.Status)
                .ExecuteAffrows();

            _logger.LogInformation("域名已批准: PlatformId={PlatformId}, Domain={Domain}", pending.PlatformId, pending.Domain);

            return ResultData.Ok("域名已加入白名单!");
        }

        /// <summary>
        /// 批量批准域名
        /// </summary>
        public ResultData ApproveBatch(int[] ids)
        {
            int successCount = 0;
            foreach (var id in ids)
            {
                var result = Approve(id);
                if (result.Code == 0) successCount++;
            }
            return ResultData.Ok($"已批准 {successCount} 个域名");
        }

        /// <summary>
        /// 拒绝域名
        /// </summary>
        public ResultData Reject(int id)
        {
            var pending = _fsql.Select<PendingDomain>().Where(p => p.Id == id).First();
            if (pending == null) return ResultData.No("记录不存在!");

            if (!HasOwnership(pending))
                return ResultData.No("无权操作!");

            pending.Status = PendingDomainStatus.Rejected;
            _fsql.Update<PendingDomain>().SetSource(pending)
                .UpdateColumns(c => c.Status)
                .ExecuteAffrows();

            return ResultData.Ok("域名已拒绝!");
        }

        /// <summary>
        /// 批量拒绝域名
        /// </summary>
        public ResultData RejectBatch(int[] ids)
        {
            int successCount = 0;
            foreach (var id in ids)
            {
                var result = Reject(id);
                if (result.Code == 0) successCount++;
            }
            return ResultData.Ok($"已拒绝 {successCount} 个域名");
        }

        /// <summary>
        /// 获取指定平台的待审核域名数量
        /// </summary>
        public int GetPendingCount(int platformId)
        {
            return (int)_fsql.Select<PendingDomain>()
                .Where(p => p.PlatformId == platformId && p.Status == PendingDomainStatus.Pending)
                .Count();
        }

        /// <summary>
        /// 批量获取多个平台的待审核域名数量映射（供 ChatPlatformService 调用）
        /// </summary>
        public Dictionary<int, int> GetPendingCountsByPlatforms(List<int> platformIds)
        {
            if (platformIds == null || !platformIds.Any())
                return new Dictionary<int, int>();

            return _fsql.Select<PendingDomain>()
                .Where(p => platformIds.Contains(p.PlatformId) && p.Status == PendingDomainStatus.Pending)
                .GroupBy(p => p.PlatformId)
                .ToList(g => new { PlatformId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.PlatformId, x => x.Count);
        }

        /// <summary>
        /// 格式化域名列表（统一格式）
        /// </summary>
        public static string FormatDomains(string allowedDomainsJson)
        {
            var domains = ParseAllowedDomains(allowedDomainsJson);
            var formatted = domains
                .Select(NormalizeDomain)
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(d => d)
                .ToList();

            return formatted.Count > 0 ? JsonSerializer.Serialize(formatted) : "";
        }

        /// <summary>
        /// 域名标准化：小写、去除协议前缀、去除路径、去除端口（80/443）、去除尾部斜杠
        /// </summary>
        public static string NormalizeDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain)) return "";

            domain = domain.Trim().ToLower();

            // 去除协议前缀
            if (domain.StartsWith("http://"))
                domain = domain.Substring(7);
            else if (domain.StartsWith("https://"))
                domain = domain.Substring(8);

            // 去除路径部分
            var slashIndex = domain.IndexOf('/');
            if (slashIndex >= 0)
                domain = domain.Substring(0, slashIndex);

            // 去除默认端口
            if (domain.EndsWith(":80"))
                domain = domain.Substring(0, domain.Length - 3);
            else if (domain.EndsWith(":443"))
                domain = domain.Substring(0, domain.Length - 4);

            return domain.Trim();
        }

        #region Private

        private static List<string> ParseAllowedDomains(string json)
        {
            if (string.IsNullOrEmpty(json)) return new List<string>();

            try
            {
                var domains = JsonSerializer.Deserialize<string[]>(json);
                return domains?.ToList() ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private bool HasOwnership(PendingDomain pending)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            string? userType = user?.UserType();

            if (userType == "Admin") return true;
            if (user == null) return false;
            return pending.OwnerId == user.Id();
        }

        #endregion
    }
}
