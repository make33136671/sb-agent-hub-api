using Microsoft.AspNetCore.Http;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;
using System.Text;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 数据导出服务
    /// </summary>
    [AutoDependencyInjection]
    public class DataExportService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly QuotaService _quotaService;
        private readonly MemberService _memberService;
        private readonly ChatPlatformService _platformService;
        private readonly ChatUserService _chatUserService;
        private readonly SatisfactionService _satisfactionService;

        public DataExportService(IHttpContextAccessor httpContextAccessor,
            QuotaService quotaService,
            MemberService memberService,
            ChatPlatformService platformService,
            ChatUserService chatUserService,
            SatisfactionService satisfactionService)
        {
            _httpContextAccessor = httpContextAccessor;
            _quotaService = quotaService;
            _memberService = memberService;
            _platformService = platformService;
            _chatUserService = chatUserService;
            _satisfactionService = satisfactionService;
        }

        /// <summary>
        /// 导出聊天记录为 CSV
        /// </summary>
        public (byte[] data, string fileName) ExportChatRecords(
            DateTime? startDate, DateTime? endDate, int? platformId)
        {
            int currentId = _httpContextAccessor.HttpContext!.User.Id();
            var member = _memberService.GetById(currentId);
            int ownerId = member?.Role == MemberRole.Owner ? member.Id : (member?.OwnerId ?? 0);

            List<int>? filterPlatformIds = null;
            if (platformId.HasValue)
            {
                filterPlatformIds = new List<int> { platformId.Value };
            }
            else
            {
                filterPlatformIds = _platformService.GetPlatformIdsByOwner(ownerId);
            }

            var records = _chatUserService.GetLoginRecords(filterPlatformIds, startDate, endDate)
                .Select(r => new
                {
                    r.Id, r.ChatUserDbId, r.PlatformId,
                    r.IpAddress, r.UserAgent, LoginTime = r.LoginTime
                });

            // 生成 CSV（UTF-8 BOM）
            var sb = new StringBuilder();
            sb.AppendLine("ID,访客ID,平台ID,IP地址,UserAgent,登录时间");
            foreach (var r in records)
            {
                sb.AppendLine($"{r.Id},\"{r.ChatUserDbId}\",{r.PlatformId},\"{r.IpAddress}\",\"{EscapeCsv(r.UserAgent)}\",\"{r.LoginTime:yyyy-MM-dd HH:mm:ss}\"");
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
            int currentId = _httpContextAccessor.HttpContext!.User.Id();
            var member = _memberService.GetById(currentId);
            int ownerId = member?.Role == MemberRole.Owner ? member.Id : (member?.OwnerId ?? 0);

            var records = _satisfactionService.GetRatingsForExport(ownerId, startDate, endDate);

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
            int currentId = _httpContextAccessor.HttpContext!.User.Id();
            var member = _memberService.GetById(currentId);
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
