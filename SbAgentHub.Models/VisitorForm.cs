using FreeSql.DataAnnotations;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 访客信息收集表单配置
    /// </summary>
    [Table(Name = "SB_VisitorForm")]
    public class VisitorForm : IEntity, ISoftDelete
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        [Column(Name = "OwnerId")]
        public int OwnerId { get; set; }

        [Column(Name = "PlatformId")]
        public int PlatformId { get; set; }

        [Column(Name = "FormName", StringLength = 100)]
        public string FormName { get; set; } = "访客信息";

        /// <summary>
        /// 表单字段配置(JSON)，格式: [{"key":"name","label":"姓名","type":"text","required":true}, ...]
        /// </summary>
        [Column(Name = "FormFields", DbType = "TEXT")]
        public string FormFields { get; set; } = "[]";

        [Column(Name = "IsRequired")]
        public bool IsRequired { get; set; } = true;

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
    /// 访客表单提交记录
    /// </summary>
    [Table(Name = "SB_VisitorFormSubmission")]
    public class VisitorFormSubmission : IEntity
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        [Column(Name = "FormId")]
        public int FormId { get; set; }

        [Column(Name = "OwnerId")]
        public int OwnerId { get; set; }

        [Column(Name = "PlatformId")]
        public int PlatformId { get; set; }

        [Column(Name = "ChatUserId", StringLength = 64)]
        public string ChatUserId { get; set; } = "";

        /// <summary>
        /// 提交数据(JSON)
        /// </summary>
        [Column(Name = "FormData", DbType = "TEXT")]
        public string FormData { get; set; } = "{}";

        [Column(Name = "IpAddress", StringLength = 50)]
        public string IpAddress { get; set; } = "";

        [Column(Name = "CreateTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
