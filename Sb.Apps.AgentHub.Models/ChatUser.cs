using FreeSql.DataAnnotations;
using System;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 聊天用户（Widget 端用户）
    /// </summary>
    [Table(Name = "sb_agenthub_chat_user")]
    [Index("ix_chatuser_chatuserid", "ChatUserId", true)]
    [Index("ix_chatuser_platformid", "PlatformId")]
    [Index("ix_chatuser_ownerid", "OwnerId")]
    [Index("ix_chatuser_assignedagentid", "AssignedAgentId")]
    [Index("ix_chatuser_groupid", "GroupId")]
    [Index("ix_chatuser_clientid", "ClientId")]
    [Index("ix_chatuser_status", "Status,OnlineStatus")]
    public class ChatUser : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 聊天用户唯一标识（Realtime 用户ID）
        /// </summary>
        [Column(Name = "ChatUserId", StringLength = 128, IsNullable = false)]
        public required string ChatUserId { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        [Column(Name = "UserType", MapType = typeof(string))]
        public ChatUserType UserType { get; set; } = ChatUserType.Anonymous;

        /// <summary>
        /// 所属平台Id
        /// </summary>
        [Column(Name = "PlatformId")]
        public int PlatformId { get; set; }

        /// <summary>
        /// 所属站长Id（通过平台关联）
        /// </summary>
        [Column(Name = "OwnerId")]
        public int OwnerId { get; set; }

        /// <summary>
        /// 外部平台用户ID
        /// </summary>
        [Column(Name = "ExternalUserId", StringLength = 100)]
        public string ExternalUserId { get; set; } = null!;

        /// <summary>
        /// 用户名
        /// </summary>
        [Column(Name = "Username", StringLength = 100)]
        public string Username { get; set; } = null!;

        /// <summary>
        /// 昵称
        /// </summary>
        [Column(Name = "Nickname", StringLength = 100)]
        public string Nickname { get; set; } = null!;

        /// <summary>
        /// 头像
        /// </summary>
        [Column(Name = "Avatar", StringLength = 500)]
        public string Avatar { get; set; } = null!;

        /// <summary>
        /// 分配的客服Id（Member.Id）
        /// </summary>
        [Column(Name = "AssignedAgentId")]
        public int? AssignedAgentId { get; set; }

        /// <summary>
        /// 会话群组 ID（所有会话统一使用群聊）
        /// </summary>
        [Column(Name = "GroupId", StringLength = 128)]
        public string GroupId { get; set; } = null!;

        /// <summary>
        /// 客户端唯一标识
        /// </summary>
        [Column(Name = "ClientId", StringLength = 64)]
        public string ClientId { get; set; } = null!;

        /// <summary>
        /// 设备类型
        /// </summary>
        [Column(Name = "DeviceType", StringLength = 50)]
        public string DeviceType { get; set; } = null!;

        /// <summary>
        /// User-Agent
        /// </summary>
        [Column(Name = "UserAgent", StringLength = 500)]
        public string UserAgent { get; set; } = null!;

        /// <summary>
        /// 最后登录IP
        /// </summary>
        [Column(Name = "LastLoginIp", StringLength = 50)]
        public string LastLoginIp { get; set; } = null!;

        /// <summary>
        /// IP归属地
        /// </summary>
        [Column(Name = "IpLocation", StringLength = 200)]
        public string IpLocation { get; set; } = null!;

        /// <summary>
        /// 用户状态
        /// </summary>
        [Column(Name = "Status", MapType = typeof(string))]
        public ChatUserStatus Status { get; set; } = ChatUserStatus.Normal;

        /// <summary>
        /// 在线状态
        /// </summary>
        [Column(Name = "OnlineStatus", MapType = typeof(string))]
        public OnlineStatus OnlineStatus { get; set; } = OnlineStatus.Offline;

        /// <summary>
        /// 昵称备注（客服可见）
        /// </summary>
        [Column(Name = "NicknameRemark", StringLength = 100)]
        public string NicknameRemark { get; set; } = null!;

        /// <summary>
        /// 备注信息
        /// </summary>
        [Column(Name = "Remark", DbType = "text")]
        public string Remark { get; set; } = null!;

        /// <summary>
        /// 首次访问时间
        /// </summary>
        [Column(Name = "FirstVisitTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime FirstVisitTime { get; set; }

        /// <summary>
        /// 最后活跃时间
        /// </summary>
        [Column(Name = "LastActiveTime", DbType = "timestamp")]
        public DateTime? LastActiveTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 软删除
        /// </summary>
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
