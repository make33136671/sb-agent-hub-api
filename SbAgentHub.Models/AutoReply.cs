using FreeSql.DataAnnotations;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 自动回复规则
    /// </summary>
    [Table(Name = "SB_AutoReply")]
    public class AutoReply : IEntity, ISoftDelete
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        [Column(Name = "OwnerId")]
        public int OwnerId { get; set; }

        [Column(Name = "PlatformId")]
        public int PlatformId { get; set; }

        /// <summary>
        /// 触发类型: 0=关键词 1=首次接入 2=无客服在线
        /// </summary>
        [Column(Name = "TriggerType")]
        public int TriggerType { get; set; }

        /// <summary>
        /// 关键词(逗号分隔，TriggerType=0时)
        /// </summary>
        [Column(Name = "Keywords", StringLength = 500)]
        public string Keywords { get; set; } = "";

        /// <summary>
        /// 匹配类型: 0=包含 1=完全匹配 2=正则
        /// </summary>
        [Column(Name = "MatchType")]
        public int MatchType { get; set; }

        /// <summary>
        /// 回复内容
        /// </summary>
        [Column(Name = "ReplyContent", DbType = "TEXT")]
        public string ReplyContent { get; set; } = "";

        /// <summary>
        /// 回复类型: 0=文本 1=图片 2=富文本
        /// </summary>
        [Column(Name = "ReplyType")]
        public int ReplyType { get; set; }

        /// <summary>
        /// 优先级(数字越大越优先)
        /// </summary>
        [Column(Name = "Priority")]
        public int Priority { get; set; }

        [Column(Name = "IsEnabled")]
        public bool IsEnabled { get; set; } = true;

        [Column(Name = "CreateTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column(Name = "UpdateTime")]
        public DateTime? UpdateTime { get; set; }

        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "0")]
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// 触发类型枚举
    /// </summary>
    public static class TriggerType
    {
        public const int Keyword = 0;
        public const int FirstVisit = 1;
        public const int NoAgentOnline = 2;
    }

    /// <summary>
    /// 匹配类型枚举
    /// </summary>
    public static class MatchType
    {
        public const int Contains = 0;
        public const int Exact = 1;
        public const int Regex = 2;
    }
}
