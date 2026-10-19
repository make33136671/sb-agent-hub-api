using FreeSql.DataAnnotations;
using System;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 客户端错误日志
    /// </summary>
    [Table(Name = "sb_agenthub_client_error_log")]
    [Index("ix_clienterrorlog_userid", "UserId")]
    [Index("ix_clienterrorlog_createtime", "CreateTime")]
    public class ClientErrorLog : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>错误来源（js/vue/chat/admin）</summary>
        [Column(Name = "Source", StringLength = 50, IsNullable = true)]
        public string Source { get; set; } = null!;

        /// <summary>错误类型</summary>
        [Column(Name = "ErrorType", StringLength = 100, IsNullable = true)]
        public string ErrorType { get; set; } = null!;

        /// <summary>错误消息</summary>
        [Column(Name = "ErrorMessage", StringLength = 2000, IsNullable = true)]
        public string ErrorMessage { get; set; } = null!;

        /// <summary>堆栈信息</summary>
        [Column(Name = "Stack", DbType = "text", IsNullable = true)]
        public string Stack { get; set; } = null!;

        /// <summary>请求URL</summary>
        [Column(Name = "Url", StringLength = 500, IsNullable = true)]
        public string Url { get; set; } = null!;

        /// <summary>User-Agent</summary>
        [Column(Name = "UserAgent", StringLength = 500, IsNullable = true)]
        public string UserAgent { get; set; } = null!;

        /// <summary>用户Id</summary>
        [Column(Name = "UserId", StringLength = 128, IsNullable = true)]
        public string UserId { get; set; } = null!;

        /// <summary>创建时间</summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }

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
}
