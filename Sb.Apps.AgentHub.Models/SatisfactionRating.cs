using FreeSql.DataAnnotations;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 满意度评价
    /// </summary>
    [Table(Name = "sb_agenthub_satisfaction_rating")]
    [Index("ix_satisfactionrating_chatuserid", "ChatUserId")]
    [Index("ix_satisfactionrating_agentid", "AgentId")]
    [Index("ix_satisfactionrating_platform_owner", "PlatformId,OwnerId")]
    public class SatisfactionRating : IEntity
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

        /// <summary>聊天用户Id</summary>
        [Column(Name = "ChatUserId", StringLength = 64)]
        public string ChatUserId { get; set; } = "";

        /// <summary>客服Id</summary>
        [Column(Name = "AgentId")]
        public int AgentId { get; set; }

        /// <summary>
        /// 评分 1-5
        /// </summary>
        [Column(Name = "Rating")]
        public int Rating { get; set; }

        /// <summary>
        /// 评价标签(逗号分隔)
        /// </summary>
        [Column(Name = "Tags", StringLength = 200)]
        public string Tags { get; set; } = "";

        /// <summary>
        /// 评价内容
        /// </summary>
        [Column(Name = "Comment", StringLength = 500)]
        public string Comment { get; set; } = "";

        /// <summary>
        /// 会话ID
        /// </summary>
        [Column(Name = "ConversationId", StringLength = 128)]
        public string ConversationId { get; set; } = "";

        /// <summary>创建时间</summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }
    }
}
