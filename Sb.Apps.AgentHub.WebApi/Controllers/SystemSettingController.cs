using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Services;

namespace Sb.Apps.AgentHub.WebApi.Controllers
{
    [ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
    public class SystemSettingController : Controller
    {
        private readonly ILogger<SystemSettingController> _logger;
        private readonly SystemSettingService _systemSettingService;
        // 框架级系统设置（SB_SystemSetting 表），如云端备份开关
        private readonly SB.Framework.Services.SystemSettingService _frameworkSystemSettingService;

        public SystemSettingController(ILogger<SystemSettingController> logger,
            SystemSettingService systemSettingService,
            SB.Framework.Services.SystemSettingService frameworkSystemSettingService)
        {
            _logger = logger;
            _systemSettingService = systemSettingService;
            _frameworkSystemSettingService = frameworkSystemSettingService;
        }

        /// <summary>
        /// 获取系统设置列表
        /// </summary>
        /// <param name="key">搜索关键词</param>
        /// <param name="field">排序字段</param>
        /// <param name="order">排序</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">单页系统设置数量</param>
        /// <returns>系统设置分页对象</returns>
        [HttpGet]
public ResultData List(string key = "", string field = "", string order = "", int page = 1, int pageSize = 10)
        {
            return _systemSettingService.GetList(key, field, order, page, pageSize);
        }

        /// <summary>
        /// 系统设置列表
        /// </summary>

        [HttpGet(Name = "SystemSettingCategoryWithSettingList")]
        public ResultData GetCategoryWithSettingList(int? categoryId = null, string field = "", string order = "", int page = 1, int pageSize = 10)
        {
            return _systemSettingService.GetCategoryWithSettingList(categoryId, field, order, page, pageSize);
        }

        /// <summary>
        /// 查询系统设置
        /// </summary>
        [HttpGet(Name = "SystemSettingQuerySystemSetting")]
        public ResultData QuerySystemSetting(string key) => _systemSettingService.QuerySystemSetting(key);

        /// <summary>
        /// 按 Key 精确查询单个系统设置项（读取框架 SB_SystemSetting 表）
        /// 用于前端读取云端备份上传开关（backup_upload_enabled）等框架级设置。
        /// </summary>
        [HttpGet]
        public ResultData GetByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return ResultData.No("key 不能为空");

            var setting = _frameworkSystemSettingService.GetSystemSettingByKey(key.Trim());
            if (setting == null) return ResultData.No("设置不存在");

            return ResultData.Ok("查询成功", setting);
        }

        /// <summary>
        /// 添加系统设置
        /// </summary>
        [HttpPost(Name = "SystemSettingAdd")]
        public ResultData Add([FromForm] SystemSetting systemSetting)
        {
            return _systemSettingService.Create(systemSetting);
        }

        /// <summary>
        /// 更新系统设置
        /// </summary>
        [HttpPost]
        public ResultData Edit([FromForm] SystemSetting systemSetting)
        {
            return _systemSettingService.Edit(systemSetting);
        }

        /// <summary>
        /// 批量保存系统设置
        /// </summary>
        [HttpPost(Name = "SystemSettingSaveList")]
        public ResultData SaveList([FromForm] string settings)
        {
            if (string.IsNullOrWhiteSpace(settings))
                return new ResultData { Code = -1, Message = "settings 为空" };

            var systemSettingList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SystemSetting>>(settings) ?? new List<SystemSetting>();

            return _systemSettingService.SaveList(systemSettingList);
        }

        /// <summary>
        /// 删除系统设置
        /// </summary>
        [HttpDelete(Name = "SystemSettingDel")]
        public ResultData Del(int id)
        {
            return _systemSettingService.Del(id);
        }

        /// <summary>
        /// 系统设置详细信息
        /// </summary>
        [HttpGet(Name = "SystemSettingDetail")]
        public ResultData Detail(int id)
        {
            return _systemSettingService.Detail(id);
        }
    }
}