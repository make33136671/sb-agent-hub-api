using FreeSql.DataAnnotations;
using System;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 业务用户（站长/客服）
    /// </summary>
    [Table(Name = "sb_agenthub_member")]
    [Index("ix_member_username", "Username", true)]
    [Index("ix_member_realtimeuserid", "RealtimeUserId")]
    [Index("ix_member_ownerid", "OwnerId")]
    [Index("ix_member_role_status", "Role,Status")]
    public class Member : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Column(Name = "Username")]
        public required string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Column(Name = "Password")]
        public required string Password { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [Column(Name = "Avatar")]
        public string Avatar { get; set; } = null!;

        /// <summary>
        /// 名称
        /// </summary>
        [Column(Name = "Name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// 工号（可选）
        /// </summary>
        [Column(Name = "AgentNo", StringLength = 50, IsNullable = true)]
        public string AgentNo { get; set; } = null!;

        /// <summary>
        /// 角色: Owner(站长) / Agent(客服)
        /// </summary>
        [Column(Name = "Role", MapType = typeof(string))]
        public MemberRole Role { get; set; }

        /// <summary>
        /// 所属站长Id（Agent 用户才有值）
        /// </summary>
        [Column(Name = "OwnerId", IsNullable = true)]
        public int? OwnerId { get; set; }

        /// <summary>
        /// Realtime 用户Id
        /// </summary>
        [Column(Name = "RealtimeUserId", StringLength = 100, IsNullable = true)]
        public string RealtimeUserId { get; set; } = null!;

        /// <summary>
        /// 在线状态
        /// </summary>
        [Column(Name = "OnlineStatus", MapType = typeof(string))]
        public OnlineStatus OnlineStatus { get; set; } = OnlineStatus.Offline;

        /// <summary>
        /// 最大同时服务数
        /// </summary>
        [Column(Name = "MaxConcurrent")]
        public int MaxConcurrent { get; set; } = 10;

        /// <summary>
        /// 当前服务数
        /// </summary>
        [Column(Name = "CurrentCount")]
        public int CurrentCount { get; set; } = 0;

        /// <summary>
        /// 欢迎语
        /// </summary>
        [Column(Name = "WelcomeMessage", StringLength = 500, IsNullable = true)]
        public string WelcomeMessage { get; set; } = null!;

        /// <summary>
        /// 账户状态
        /// </summary>
        [Column(Name = "Status", MapType = typeof(string))]
        public UserStatus Status { get; set; } = UserStatus.Active;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()", CanUpdate = false)]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后在线时间
        /// </summary>
        [Column(Name = "LastOnlineTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime LastOnlineTime { get; set; }

        /// <summary>
        /// 软删除标记
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

    /// <summary>
    /// Member 角色
    /// </summary>
    public enum MemberRole
    {
        /// <summary>站长</summary>
        Owner,
        /// <summary>客服</summary>
        Agent
    }

    /// <summary>
    /// 在线状态
    /// </summary>
    public enum OnlineStatus
    {
        /// <summary>离线</summary>
        Offline = 0,
        /// <summary>在线</summary>
        Online = 1,
        /// <summary>忙碌</summary>
        Busy = 2
    }
}
