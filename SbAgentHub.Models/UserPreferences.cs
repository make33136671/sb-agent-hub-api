using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 用户偏好设置
    /// </summary>
    [Table(Name = "SB_UserPreferences")]
    public class UserPreferences
    {
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
        public string UserType { get; set; }

        /// <summary>
        /// 新消息提示音
        /// </summary>
        [Column(Name = "SoundEnabled")]
        public bool SoundEnabled { get; set; } = true;

        /// <summary>
        /// 提示音类型
        /// </summary>
        [Column(Name = "SoundType", StringLength = 50)]
        public string SoundType { get; set; } = "default";

        /// <summary>
        /// 提示音音量 (0-100)
        /// </summary>
        [Column(Name = "SoundVolume")]
        public int SoundVolume { get; set; } = 80;

        /// <summary>
        /// 桌面通知
        /// </summary>
        [Column(Name = "DesktopNotification")]
        public bool DesktopNotification { get; set; } = true;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column(Name = "UpdateTime", DbType = "timestamp")]
        public DateTime? UpdateTime { get; set; }
    }
}
