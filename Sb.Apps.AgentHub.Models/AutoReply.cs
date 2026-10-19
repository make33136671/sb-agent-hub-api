using FreeSql.DataAnnotations;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 自动回复规则
    /// </summary>
    [Table(Name = "sb_agenthub_auto_reply")]
    [Index("ix_autoreply_ownerid_platformid", "OwnerId,PlatformId")]
    [Index("ix_autoreply_isenabled", "IsEnabled")]
    public class AutoReply : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>所属站长Id</summary>
        [Column(Name = "OwnerId")]
        public int OwnerId { get; set; }

        /// <summary>所属平台Id</summary>
        [Column(Name = "PlatformId")]
        public int PlatformId { get; set; }

        /// <summary>
        /// 触发类型
        /// </summary>
        [Column(Name = "TriggerType", MapType = typeof(string))]
        public TriggerType TriggerType { get; set; }

        /// <summary>
        /// 关键词(逗号分隔，TriggerType=0时)
        /// </summary>
        [Column(Name = "Keywords", StringLength = 500)]
        public string Keywords { get; set; } = "";

        /// <summary>
        /// 匹配类型
        /// </summary>
        [Column(Name = "MatchType", MapType = typeof(string))]
        public MatchType MatchType { get; set; }

        /// <summary>
        /// 回复内容
        /// </summary>
        [Column(Name = "ReplyContent", DbType = "TEXT")]
        public string ReplyContent { get; set; } = "";

        /// <summary>
        /// 回复类型
        /// </summary>
        [Column(Name = "ReplyType", MapType = typeof(string))]
        public ReplyType ReplyType { get; set; }

        /// <summary>
        /// 优先级(数字越大越优先)
        /// </summary>
        [Column(Name = "Priority")]
        public int Priority { get; set; }

        /// <summary>是否启用</summary>
        [Column(Name = "IsEnabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>创建时间</summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }

        /// <summary>更新时间</summary>
        [Column(Name = "UpdateTime")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>软删除标记</summary>
        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "false")]
        public bool IsDeleted { get; set; }

        /// <summary>删除时间</summary>
        [Column(Name = "DeletedAt", IsNullable = true)]
        public DateTime? DeletedAt { get; set; }

        /// <summary>删除人Id</summary>
        [Column(Name = "DeletedBy", IsNullable = true)]
        public int? DeletedBy { get; set; }
    }

    /// <summary>
    /// 触发类型
    /// </summary>
    public enum TriggerType
    {
        /// <summary>关键词触发</summary>
        Keyword = 0,
        /// <summary>首次访问触发</summary>
        FirstVisit = 1,
        /// <summary>无客服在线触发</summary>
        NoAgentOnline = 2
    }

    /// <summary>
    /// 匹配类型
    /// </summary>
    public enum MatchType
    {
        /// <summary>包含匹配</summary>
        Contains = 0,
        /// <summary>精确匹配</summary>
        Exact = 1,
        /// <summary>正则匹配</summary>
        Regex = 2
    }

    /// <summary>
    /// 回复类型
    /// </summary>
    public enum ReplyType
    {
        /// <summary>文本</summary>
        Text = 0,
        /// <summary>图片</summary>
        Image = 1,
        /// <summary>富文本</summary>
        RichText = 2
    }
}
