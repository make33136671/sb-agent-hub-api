using FreeSql.DataAnnotations;
using System;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 聊天用户登录记录
    /// </summary>
    [Table(Name = "sb_agenthub_chat_user_login_record")]
    [Index("ix_chatuserloginrecord_chatuserdbid", "ChatUserDbId")]
    [Index("ix_chatuserloginrecord_platformid", "PlatformId")]
    [Index("ix_chatuserloginrecord_logintime", "LoginTime")]
    public class ChatUserLoginRecord : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>对应的 ChatUser.Id（数据库主键）</summary>
        [Column(Name = "ChatUserDbId")]
        public int ChatUserDbId { get; set; }

        /// <summary>平台Id</summary>
        [Column(Name = "PlatformId")]
        public int PlatformId { get; set; }

        /// <summary>客户端唯一标识</summary>
        [Column(Name = "ClientId", StringLength = 128, IsNullable = true)]
        public string ClientId { get; set; } = null!;

        /// <summary>IP地址</summary>
        [Column(Name = "IpAddress", StringLength = 50, IsNullable = true)]
        public string IpAddress { get; set; } = null!;

        /// <summary>User-Agent</summary>
        [Column(Name = "UserAgent", StringLength = 500, IsNullable = true)]
        public string UserAgent { get; set; } = null!;

        /// <summary>设备类型</summary>
        [Column(Name = "DeviceType", StringLength = 50, IsNullable = true)]
        public string DeviceType { get; set; } = null!;

        /// <summary>登录时间</summary>
        [Column(Name = "LoginTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime LoginTime { get; set; }

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
