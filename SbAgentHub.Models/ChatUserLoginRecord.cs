using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 聊天用户登录记录
    /// </summary>
    [Table(Name = "SB_ChatUserLoginRecord")]
    public class ChatUserLoginRecord
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        [Column(Name = "ChatUserId", StringLength = 128)]
        public string ChatUserId { get; set; }

        [Column(Name = "PlatformId")]
        public long PlatformId { get; set; }

        [Column(Name = "IpAddress", StringLength = 50)]
        public string IpAddress { get; set; }

        [Column(Name = "UserAgent", StringLength = 500)]
        public string UserAgent { get; set; }

        [Column(Name = "DeviceType", StringLength = 50)]
        public string DeviceType { get; set; }

        [Column(Name = "LoginTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime LoginTime { get; set; }
    }
}
