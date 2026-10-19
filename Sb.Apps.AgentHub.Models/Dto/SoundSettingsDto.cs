namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 声音设置 DTO
    /// </summary>
    public class SoundSettingsDto
    {
        /// <summary>
        /// 新消息提示音开关
        /// </summary>
        public bool NewMessageEnabled { get; set; } = true;

        /// <summary>
        /// 新消息提示音文件
        /// </summary>
        public string NewMessageSound { get; set; } = "ding.mp3";

        /// <summary>
        /// 新消息提示音音量 (0-100)
        /// </summary>
        public int NewMessageVolume { get; set; } = 80;

        /// <summary>
        /// 系统通知提示音开关
        /// </summary>
        public bool SystemNotificationEnabled { get; set; } = true;

        /// <summary>
        /// 系统通知提示音文件
        /// </summary>
        public string SystemNotificationSound { get; set; } = "dingdong.mp3";

        /// <summary>
        /// 系统通知提示音音量 (0-100)
        /// </summary>
        public int SystemNotificationVolume { get; set; } = 80;
    }
}
