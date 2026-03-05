using FreeSql;
using Microsoft.AspNetCore.Http;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Models;
using System.Text;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 数据导出服务
    /// </summary>
    [AutoDependencyInjection]
    public class DataExportService
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly QuotaService _quotaService;

        public DataExportService(IFreeSql fsql, IHttpContextAccessor httpContextAccessor,
            QuotaService quotaService)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _quotaService = quotaService;
        }

        /// <summary>
        /// 导出聊天记录为 CSV
        /// </summary>
        public (byte[] data, string fileName) ExportChatRecords(
            DateTime? startDate, DateTime? endDate, int? platformId)
        {
            int currentId = _httpContextAccessor.HttpContext.User.Id();
            var member = _fsql.Select<Member>().Where(m => m.Id == currentId).First();
            int ownerId = member?.Role == MemberRole.Owner ? member.Id : (member?.OwnerId ?? 0);

            var query = _fsql.Select<ChatUserLoginRecord>()
                .Where(r => true);

            // 通过平台过滤 owner 的数据
            if (platformId.HasValue)
            {
                query = query.Where(r => r.PlatformId == platformId.Value);
            }
            else
            {
                var platformIds = _fsql.Select<ChatPlatform>()
                    .Where(p => p.OwnerId == ownerId)
                    .ToList(p => p.Id);
                if (platformIds.Any())
                    query = query.Where(r => platformIds.Contains(r.PlatformId));
            }

            if (startDate.HasValue)
                query = query.Where(r => r.LoginTime >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(r => r.LoginTime < endDate.Value.AddDays(1));

            var records = query.OrderByDescending(r => r.LoginTime)
                .ToList(r => new
                {
                    r.Id, r.ChatUserId, r.PlatformId,
                    r.IpAddress, r.UserAgent, LoginTime = r.LoginTime
                });

            // 生成 CSV（UTF-8 BOM）
            var sb = new StringBuilder();
            sb.AppendLine("ID,访客ID,平台ID,IP地址,UserAgent,登录时间");
            foreach (var r in records)
            {
                sb.AppendLine($"{r.Id},\"{r.ChatUserId}\",{r.PlatformId},\"{r.IpAddress}\",\"{EscapeCsv(r.UserAgent)}\",\"{r.LoginTime:yyyy-MM-dd HH:mm:ss}\"");
            }

            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var result = new byte[bom.Length + csvBytes.Length];
            bom.CopyTo(result, 0);
            csvBytes.CopyTo(result, bom.Length);

            var fileName = $"chat_records_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return (result, fileName);
        }

        /// <summary>
        /// 导出满意度评价为 CSV
        /// </summary>
        public (byte[] data, string fileName) ExportSatisfactionRatings(
            DateTime? startDate, DateTime? endDate)
        {
            int currentId = _httpContextAccessor.HttpContext.User.Id();
            var member = _fsql.Select<Member>().Where(m => m.Id == currentId).First();
            int ownerId = member?.Role == MemberRole.Owner ? member.Id : (member?.OwnerId ?? 0);

            var query = _fsql.Select<SatisfactionRating>()
                .Where(r => r.OwnerId == ownerId);

            if (startDate.HasValue)
                query = query.Where(r => r.CreateTime >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(r => r.CreateTime < endDate.Value.AddDays(1));

            var records = query.OrderByDescending(r => r.CreateTime).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("ID,访客ID,客服ID,评分,标签,评价内容,时间");
            foreach (var r in records)
            {
                sb.AppendLine($"{r.Id},\"{r.ChatUserId}\",{r.AgentId},{r.Rating},\"{EscapeCsv(r.Tags)}\",\"{EscapeCsv(r.Comment)}\",\"{r.CreateTime:yyyy-MM-dd HH:mm:ss}\"");
            }

            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var result = new byte[bom.Length + csvBytes.Length];
            bom.CopyTo(result, 0);
            csvBytes.CopyTo(result, bom.Length);

            return (result, $"satisfaction_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        /// <summary>
        /// 检查导出权限
        /// </summary>
        public bool CanExport()
        {
            int currentId = _httpContextAccessor.HttpContext.User.Id();
            var member = _fsql.Select<Member>().Where(m => m.Id == currentId).First();
            int ownerId = member?.Role == MemberRole.Owner ? member.Id : (member?.OwnerId ?? 0);
            return _quotaService.CheckFeature(ownerId, "feature_data_export");
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
        }
    }
}
