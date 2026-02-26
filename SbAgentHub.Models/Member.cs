using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 业务用户（站长/客服）
    /// </summary>
    [Table(Name = "SB_Member")]
    public class Member : IEntity, ISoftDelete
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Column(Name = "Username")]
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Column(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [Column(Name = "Avatar")]
        public string Avatar { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Column(Name = "Name")]
        public string Name { get; set; }

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
        /// OpenIM 用户Id
        /// </summary>
        [Column(Name = "OpenImUserId", StringLength = 100, IsNullable = true)]
        public string OpenImUserId { get; set; }

        /// <summary>
        /// 在线状态
        /// </summary>
        [Column(Name = "OnlineStatus")]
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
        public string WelcomeMessage { get; set; }

        /// <summary>
        /// 账户状态
        /// </summary>
        [Column(Name = "Status", MapType = typeof(string))]
        public UserStatus Status { get; set; } = UserStatus.Active;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreatedTime", DbType = "timestamp", InsertValueSql = "now()", CanUpdate = false)]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 最后在线时间
        /// </summary>
        [Column(Name = "LastOnlineTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime LastOnlineTime { get; set; }

        /// <summary>
        /// 软删除标记
        /// </summary>
        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "0")]
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// Member 角色
    /// </summary>
    public enum MemberRole
    {
        Owner,
        Agent
    }

    /// <summary>
    /// 在线状态
    /// </summary>
    public enum OnlineStatus
    {
        Offline = 0,
        Online = 1,
        Busy = 2
    }
}
