using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers
{
    [ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
    public class SystemSettingCategoryController : Controller
    {
        private readonly ILogger<SystemSettingCategoryController> _logger;
        private readonly SystemSettingCategoryService _systemSettingCategoryService;

        public SystemSettingCategoryController(ILogger<SystemSettingCategoryController> logger,
            SystemSettingCategoryService systemSettingCategoryService)
        {
            _logger = logger;
            _systemSettingCategoryService = systemSettingCategoryService;
        }

        #region CURD
        /// <summary>
        /// 设置分类列表
        /// </summary>
        [HttpGet]
        public ResultData List(string key = "", string name = "", string field = "", string order = "", int page = 1, int pageSize = 10)
        {
            return _systemSettingCategoryService.GetList(key, name, field, order, page, pageSize);
        }

        /// <summary>
        /// 查询设置分类
        /// </summary>
        [HttpGet]
        public ResultData QuerySystemSettingCategory(string key) => _systemSettingCategoryService.QuerySystemSettingCategory(key);

        /// <summary>
        /// 添加设置分类
        /// </summary>
        [HttpPost]
        public ResultData Add([FromForm] SystemSettingCategory systemSettingCategory)
        {
            return _systemSettingCategoryService.Create(systemSettingCategory);
        }

        /// <summary>
        /// 更新设置分类
        /// </summary>
        [HttpPost]
        public ResultData Edit([FromForm] SystemSettingCategory systemSettingCategory)
        {
            return _systemSettingCategoryService.Edit(systemSettingCategory);
        }

        /// <summary>
        /// 删除设置分类
        /// </summary>
        [HttpPost]
        public ResultData Del([FromForm] int id)
        {
            return _systemSettingCategoryService.Del(id);
        }

        /// <summary>
        /// 设置分类详细信息
        /// </summary>
        [HttpGet]
        public ResultData Detail(int id)
        {
            return _systemSettingCategoryService.Detail(id);
        }
        #endregion
    }
}