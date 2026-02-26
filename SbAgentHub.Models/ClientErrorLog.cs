using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 客户端错误日志
    /// </summary>
    [Table(Name = "SB_ClientErrorLog")]
    public class ClientErrorLog : ISoftDelete
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        [Column(Name = "Source", StringLength = 50, IsNullable = true)]
        public string Source { get; set; }

        [Column(Name = "ErrorType", StringLength = 100, IsNullable = true)]
        public string ErrorType { get; set; }

        [Column(Name = "ErrorMessage", StringLength = 2000, IsNullable = true)]
        public string ErrorMessage { get; set; }

        [Column(Name = "Stack", DbType = "text", IsNullable = true)]
        public string Stack { get; set; }

        [Column(Name = "Url", StringLength = 500, IsNullable = true)]
        public string Url { get; set; }

        [Column(Name = "UserAgent", StringLength = 500, IsNullable = true)]
        public string UserAgent { get; set; }

        [Column(Name = "UserId", StringLength = 128, IsNullable = true)]
        public string UserId { get; set; }

        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }

        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "0")]
        public bool IsDeleted { get; set; }
    }
}
