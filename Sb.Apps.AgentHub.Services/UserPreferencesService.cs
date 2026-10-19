using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 用户偏好设置服务
    /// </summary>
    [AutoDependencyInjection]
    public class UserPreferencesService : BaseCrudService<UserPreferences>
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserPreferencesService(IFreeSql fsql, ILogger<UserPreferencesService> logger,
            IHttpContextAccessor httpContextAccessor) : base(fsql, logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 获取当前用户偏好设置
        /// </summary>
        public ResultData GetPreferences()
        {
            var user = _httpContextAccessor.HttpContext!.User;
            int userId = user.Id();
            string userType = user.UserType();
            if (userId <= 0) return ResultData.No("授权失效!");

            var prefs = Select.Where(c => c.UserId == userId && c.UserType == userType).First();

            if (prefs == null)
            {
                // 返回默认设置
                prefs = new UserPreferences
                {
                    UserId = userId,
                    UserType = userType,
                    SoundEnabled = true,
                    SoundVolume = 80,
                    DesktopNotification = true,
                    AutoReply = false,
                    AutoReplyMessage = ""
                };
            }

            return ResultData.Ok(prefs);
        }

        /// <summary>
        /// 保存用户偏好设置
        /// </summary>
        public ResultData SavePreferences(UserPreferences prefs)
        {
            var user = _httpContextAccessor.HttpContext!.User;
            int userId = user.Id();
            string userType = user.UserType();
            if (userId <= 0) return ResultData.No("授权失效!");

            var existing = Select.Where(c => c.UserId == userId && c.UserType == userType).First();

            if (existing == null)
            {
                prefs.UserId = userId;
                prefs.UserType = userType;
                prefs.CreateTime = DateTime.Now;
                prefs = Insert(prefs);
                return ResultData.Ok("保存成功!", prefs);
            }

            existing.SoundEnabled = prefs.SoundEnabled;
            existing.SoundFile = prefs.SoundFile;
            existing.SoundVolume = prefs.SoundVolume;
            existing.SystemNotificationEnabled = prefs.SystemNotificationEnabled;
            existing.SystemNotificationSound = prefs.SystemNotificationSound;
            existing.SystemNotificationVolume = prefs.SystemNotificationVolume;
            existing.DesktopNotification = prefs.DesktopNotification;
            existing.AutoReply = prefs.AutoReply;
            existing.AutoReplyMessage = prefs.AutoReplyMessage;

            UpdateDiy.UpdateColumns(c => new
            {
                c.SoundEnabled, c.SoundFile, c.SoundVolume,
                c.SystemNotificationEnabled, c.SystemNotificationSound, c.SystemNotificationVolume,
                c.DesktopNotification, c.AutoReply, c.AutoReplyMessage
            }).SetSource(existing).ExecuteAffrows();

            return ResultData.Ok("保存成功!");
        }

        /// <summary>
        /// 获取声音设置
        /// </summary>
        public ResultData GetSoundSettings()
        {
            var user = _httpContextAccessor.HttpContext!.User;
            int userId = user.Id();
            string userType = user.UserType();
            if (userId <= 0) return ResultData.No("授权失效!");

            var prefs = Select.Where(c => c.UserId == userId && c.UserType == userType).First();

            var dto = new SoundSettingsDto
            {
                NewMessageEnabled = prefs?.SoundEnabled ?? true,
                NewMessageSound = prefs?.SoundFile ?? "ding.mp3",
                NewMessageVolume = prefs?.SoundVolume ?? 80,
                SystemNotificationEnabled = prefs?.SystemNotificationEnabled ?? true,
                SystemNotificationSound = prefs?.SystemNotificationSound ?? "dingdong.mp3",
                SystemNotificationVolume = prefs?.SystemNotificationVolume ?? 80,
            };

            return ResultData.Ok(dto);
        }

        /// <summary>
        /// 保存声音设置
        /// </summary>
        public ResultData SaveSoundSettings(SoundSettingsDto dto)
        {
            var user = _httpContextAccessor.HttpContext!.User;
            int userId = user.Id();
            string userType = user.UserType();
            if (userId <= 0) return ResultData.No("授权失效!");

            var existing = Select.Where(c => c.UserId == userId && c.UserType == userType).First();

            if (existing == null)
            {
                var prefs = new UserPreferences
                {
                    UserId = userId,
                    UserType = userType,
                    SoundEnabled = dto.NewMessageEnabled,
                    SoundFile = dto.NewMessageSound,
                    SoundVolume = dto.NewMessageVolume,
                    SystemNotificationEnabled = dto.SystemNotificationEnabled,
                    SystemNotificationSound = dto.SystemNotificationSound,
                    SystemNotificationVolume = dto.SystemNotificationVolume,
                    CreateTime = DateTime.Now,
                };
                Insert(prefs);
                return ResultData.Ok("保存成功!");
            }

            existing.SoundEnabled = dto.NewMessageEnabled;
            existing.SoundFile = dto.NewMessageSound;
            existing.SoundVolume = dto.NewMessageVolume;
            existing.SystemNotificationEnabled = dto.SystemNotificationEnabled;
            existing.SystemNotificationSound = dto.SystemNotificationSound;
            existing.SystemNotificationVolume = dto.SystemNotificationVolume;

            UpdateDiy.UpdateColumns(c => new
            {
                c.SoundEnabled, c.SoundFile, c.SoundVolume,
                c.SystemNotificationEnabled, c.SystemNotificationSound, c.SystemNotificationVolume,
            }).SetSource(existing).ExecuteAffrows();

            return ResultData.Ok("保存成功!");
        }

        /// <summary>
        /// 获取可用提示音列表
        /// </summary>
        public ResultData GetAvailableSounds()
        {
            var sounds = new[]
            {
                new { label = "叮（清脆）", value = "ding.mp3" },
                new { label = "叮咚（双音）", value = "dingdong.mp3" },
                new { label = "铃铃铃（铃声）", value = "dinglingling.mp3" },
                new { label = "Duang（短促）", value = "duang.mp3" },
            };
            return ResultData.Ok(sounds);
        }
    }
}
