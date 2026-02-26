using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 聊天用户登录记录
    /// </summary>
    [Table(Name = "SB_ChatUserLoginRecord")]
    public class ChatUserLoginRecord : ISoftDelete
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        [Column(Name = "ChatUserId")]
        public int ChatUserId { get; set; }

        [Column(Name = "PlatformId")]
        public int PlatformId { get; set; }

        [Column(Name = "ClientId", StringLength = 128, IsNullable = true)]
        public string ClientId { get; set; }

        [Column(Name = "IpAddress", StringLength = 50, IsNullable = true)]
        public string IpAddress { get; set; }

        [Column(Name = "UserAgent", StringLength = 500, IsNullable = true)]
        public string UserAgent { get; set; }

        [Column(Name = "DeviceType", StringLength = 50, IsNullable = true)]
        public string DeviceType { get; set; }

        [Column(Name = "LoginTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime LoginTime { get; set; }

        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "0")]
        public bool IsDeleted { get; set; }
    }
}
