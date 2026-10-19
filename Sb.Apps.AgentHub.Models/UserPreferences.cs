using FreeSql.DataAnnotations;
using System;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 用户偏好设置
    /// </summary>
    [Table(Name = "sb_agenthub_user_preferences")]
    [Index("ix_userpreferences_userid_usertype", "UserId,UserType", true)]
    public class UserPreferences : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        [Column(Name = "UserId")]
        public int UserId { get; set; }

        /// <summary>
        /// 用户类型: Admin / Member
        /// </summary>
        [Column(Name = "UserType", StringLength = 20)]
        public string UserType { get; set; } = null!;

        /// <summary>
        /// 新消息提示音开关
        /// </summary>
        [Column(Name = "SoundEnabled")]
        public bool SoundEnabled { get; set; } = true;

        /// <summary>
        /// 新消息提示音文件
        /// </summary>
        [Column(Name = "SoundFile", StringLength = 100, IsNullable = true)]
        public string SoundFile { get; set; } = "ding.mp3";

        /// <summary>
        /// 新消息提示音音量 (0-100)
        /// </summary>
        [Column(Name = "SoundVolume")]
        public int SoundVolume { get; set; } = 80;

        /// <summary>
        /// 系统通知提示音开关
        /// </summary>
        [Column(Name = "SystemNotificationEnabled")]
        public bool SystemNotificationEnabled { get; set; } = true;

        /// <summary>
        /// 系统通知提示音文件
        /// </summary>
        [Column(Name = "SystemNotificationSound", StringLength = 100, IsNullable = true)]
        public string SystemNotificationSound { get; set; } = "dingdong.mp3";

        /// <summary>
        /// 系统通知提示音音量 (0-100)
        /// </summary>
        [Column(Name = "SystemNotificationVolume")]
        public int SystemNotificationVolume { get; set; } = 80;

        /// <summary>
        /// 桌面通知
        /// </summary>
        [Column(Name = "DesktopNotification")]
        public bool DesktopNotification { get; set; } = true;

        /// <summary>
        /// 自动回复
        /// </summary>
        [Column(Name = "AutoReply")]
        public bool AutoReply { get; set; } = false;

        /// <summary>
        /// 自动回复消息
        /// </summary>
        [Column(Name = "AutoReplyMessage", StringLength = 500, IsNullable = true)]
        public string AutoReplyMessage { get; set; } = null!;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()", CanUpdate = false)]
        public DateTime CreateTime { get; set; }

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
}
