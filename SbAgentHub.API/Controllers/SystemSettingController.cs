using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers
{
    [ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
    public class SystemSettingController : Controller
    {
        private readonly ILogger<SystemSettingController> _logger;
        private readonly SystemSettingService _systemSettingService;

        public SystemSettingController(ILogger<SystemSettingController> logger,
            SystemSettingService systemSettingService)
        {
            _logger = logger;
            _systemSettingService = systemSettingService;
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
            List<SystemSetting> systemSettingList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SystemSetting>>(settings);

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