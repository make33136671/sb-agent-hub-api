using FreeSql.DataAnnotations;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 满意度评价
    /// </summary>
    [Table(Name = "SB_SatisfactionRating")]
    public class SatisfactionRating : IEntity
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        [Column(Name = "OwnerId")]
        public int OwnerId { get; set; }

        [Column(Name = "PlatformId")]
        public int PlatformId { get; set; }

        [Column(Name = "ChatUserId", StringLength = 64)]
        public string ChatUserId { get; set; } = "";

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

        [Column(Name = "CreateTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
