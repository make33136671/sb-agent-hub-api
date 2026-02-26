using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 客户端错误日志
    /// </summary>
    [Table(Name = "SB_ClientErrorLog")]
    public class ClientErrorLog
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        [Column(Name = "Source", StringLength = 50)]
        public string Source { get; set; }

        [Column(Name = "ErrorType", StringLength = 100)]
        public string ErrorType { get; set; }

        [Column(Name = "Message", StringLength = 2000)]
        public string Message { get; set; }

        [Column(Name = "Stack", DbType = "text")]
        public string Stack { get; set; }

        [Column(Name = "Url", StringLength = 500)]
        public string Url { get; set; }

        [Column(Name = "UserAgent", StringLength = 500)]
        public string UserAgent { get; set; }

        [Column(Name = "UserId", StringLength = 128)]
        public string UserId { get; set; }

        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }
    }
}
