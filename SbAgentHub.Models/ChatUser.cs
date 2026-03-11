using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 聊天用户（Widget 端用户）
    /// </summary>
    [Table(Name = "SB_ChatUser")]
    public class ChatUser : IEntity, ISoftDelete
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 聊天用户唯一标识（用于 OpenIM userID）
        /// </summary>
        [Column(Name = "ChatUserId", StringLength = 128, IsNullable = false)]
        public string ChatUserId { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        [Column(Name = "UserType")]
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
        public string ExternalUserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Column(Name = "Username", StringLength = 100)]
        public string Username { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [Column(Name = "Nickname", StringLength = 100)]
        public string Nickname { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [Column(Name = "Avatar", StringLength = 500)]
        public string Avatar { get; set; }

        /// <summary>
        /// 分配的客服Id（Member.Id）
        /// </summary>
        [Column(Name = "AssignedAgentId")]
        public int? AssignedAgentId { get; set; }

        /// <summary>
        /// OpenIM 群组 ID（所有会话统一使用群聊）
        /// </summary>
        [Column(Name = "GroupId", StringLength = 128)]
        public string GroupId { get; set; }

        /// <summary>
        /// 客户端唯一标识
        /// </summary>
        [Column(Name = "ClientId", StringLength = 64)]
        public string ClientId { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        [Column(Name = "DeviceType", StringLength = 50)]
        public string DeviceType { get; set; }

        /// <summary>
        /// User-Agent
        /// </summary>
        [Column(Name = "UserAgent", StringLength = 500)]
        public string UserAgent { get; set; }

        /// <summary>
        /// 最后登录IP
        /// </summary>
        [Column(Name = "LastLoginIp", StringLength = 50)]
        public string LastLoginIp { get; set; }

        /// <summary>
        /// IP归属地
        /// </summary>
        [Column(Name = "IpLocation", StringLength = 200)]
        public string IpLocation { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        [Column(Name = "Status")]
        public ChatUserStatus Status { get; set; } = ChatUserStatus.Normal;

        /// <summary>
        /// 在线状态
        /// </summary>
        [Column(Name = "OnlineStatus")]
        public OnlineStatus OnlineStatus { get; set; } = OnlineStatus.Offline;

        /// <summary>
        /// 昵称备注（客服可见）
        /// </summary>
        [Column(Name = "NicknameRemark", StringLength = 100)]
        public string NicknameRemark { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [Column(Name = "Remark", DbType = "text")]
        public string Remark { get; set; }

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
        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "0")]
        public bool IsDeleted { get; set; }
    }
}
