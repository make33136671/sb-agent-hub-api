using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Models;

namespace SbAgentHub.Services
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
            var user = _httpContextAccessor.HttpContext.User;
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
            var user = _httpContextAccessor.HttpContext.User;
            int userId = user.Id();
            string userType = user.UserType();
            if (userId <= 0) return ResultData.No("授权失效!");

            var existing = Select.Where(c => c.UserId == userId && c.UserType == userType).First();

            if (existing == null)
            {
                prefs.UserId = userId;
                prefs.UserType = userType;
                prefs.CreatedTime = DateTime.Now;
                prefs = Insert(prefs);
                return ResultData.Ok("保存成功!", prefs);
            }

            existing.SoundEnabled = prefs.SoundEnabled;
            existing.SoundVolume = prefs.SoundVolume;
            existing.DesktopNotification = prefs.DesktopNotification;
            existing.AutoReply = prefs.AutoReply;
            existing.AutoReplyMessage = prefs.AutoReplyMessage;

            UpdateDiy.UpdateColumns(c => new
            {
                c.SoundEnabled, c.SoundVolume, c.DesktopNotification,
                c.AutoReply, c.AutoReplyMessage
            }).SetSource(existing).ExecuteAffrows();

            return ResultData.Ok("保存成功!");
        }
    }
}
