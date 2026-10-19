using FreeSql.DataAnnotations;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 访客信息收集表单配置
    /// </summary>
    [Table(Name = "sb_agenthub_visitor_form")]
    [Index("ix_visitorform_platformid_ownerid", "PlatformId,OwnerId")]
    public class VisitorForm : IEntity, ISoftDelete
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

        /// <summary>表单名称</summary>
        [Column(Name = "FormName", StringLength = 100)]
        public string FormName { get; set; } = "访客信息";

        /// <summary>
        /// 表单字段配置(JSON)，格式: [{"key":"name","label":"姓名","type":"text","required":true}, ...]
        /// </summary>
        [Column(Name = "FormFields", DbType = "TEXT")]
        public string FormFields { get; set; } = "[]";

        /// <summary>是否必填</summary>
        [Column(Name = "IsRequired")]
        public bool IsRequired { get; set; } = true;

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
    /// 访客表单提交记录
    /// </summary>
    [Table(Name = "sb_agenthub_visitor_form_submission")]
    [Index("ix_visitorformsubmission_formid", "FormId")]
    [Index("ix_visitorformsubmission_chatuserid", "ChatUserId")]
    public class VisitorFormSubmission : IEntity
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>表单Id</summary>
        [Column(Name = "FormId")]
        public int FormId { get; set; }

        /// <summary>所属站长Id</summary>
        [Column(Name = "OwnerId")]
        public int OwnerId { get; set; }

        /// <summary>所属平台Id</summary>
        [Column(Name = "PlatformId")]
        public int PlatformId { get; set; }

        /// <summary>聊天用户Id</summary>
        [Column(Name = "ChatUserId", StringLength = 64)]
        public string ChatUserId { get; set; } = "";

        /// <summary>
        /// 提交数据(JSON)
        /// </summary>
        [Column(Name = "FormData", DbType = "TEXT")]
        public string FormData { get; set; } = "{}";

        /// <summary>IP地址</summary>
        [Column(Name = "IpAddress", StringLength = 50)]
        public string IpAddress { get; set; } = "";

        /// <summary>创建时间</summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }
    }
}
