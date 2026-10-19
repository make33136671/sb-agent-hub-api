using FreeSql.DataAnnotations;
using System;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 登录日志
    /// </summary>
    [Table(Name = "sb_agenthub_login_log")]
    [Index("ix_loginlog_userid_usertype", "UserId,UserType")]
    [Index("ix_loginlog_createtime", "CreateTime")]
    public class LoginLog : IEntity
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// Member.Id 或 Admin.Id
        /// </summary>
        [Column(Name = "UserId")]
        public int UserId { get; set; }

        /// <summary>
        /// 0=H5会员, 1=Admin管理员
        /// </summary>
        [Column(Name = "UserType")]
        public int UserType { get; set; }

        /// <summary>用户名</summary>
        [Column(Name = "Username", StringLength = 100)]
        public string Username { get; set; } = null!;

        /// <summary>IP地址</summary>
        [Column(Name = "IpAddress", StringLength = 50, IsNullable = true)]
        public string? IpAddress { get; set; }

        /// <summary>User-Agent</summary>
        [Column(Name = "UserAgent", StringLength = 500, IsNullable = true)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// 0=成功, 1=失败
        /// </summary>
        [Column(Name = "Status")]
        public int Status { get; set; }

        /// <summary>创建时间</summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()", CanUpdate = false)]
        public DateTime CreateTime { get; set; }
    }
}
