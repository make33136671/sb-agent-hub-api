using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Models;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 访客信息收集表单服务
    /// </summary>
    [AutoDependencyInjection]
    public class VisitorFormService
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly QuotaService _quotaService;

        public VisitorFormService(IFreeSql fsql, IHttpContextAccessor httpContextAccessor,
            QuotaService quotaService)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _quotaService = quotaService;
        }

        /// <summary>
        /// 获取表单列表（站长端）
        /// </summary>
        public ResultData GetFormList(int? platformId = null)
        {
            int ownerId = ResolveOwnerId();
            if (ownerId < 0) return ResultData.No("授权失效!");

            var query = _fsql.Select<VisitorForm>()
                .WhereIf(ownerId > 0, f => f.OwnerId == ownerId);

            if (platformId.HasValue)
                query = query.Where(f => f.PlatformId == platformId.Value);

            var list = query.OrderByDescending(f => f.CreateTime)
                .ToList(f => new
                {
                    f.Id, f.OwnerId, f.PlatformId, f.FormName,
                    f.FormFields, f.IsRequired, f.IsEnabled, f.CreateTime
                });

            return ResultData.Ok(list);
        }

        /// <summary>
        /// 获取表单详情
        /// </summary>
        public ResultData GetFormDetail(int formId)
        {
            int ownerId = ResolveOwnerId();
            if (ownerId < 0) return ResultData.No("授权失效!");

            var form = _fsql.Select<VisitorForm>()
                .Where(f => f.Id == formId)
                .WhereIf(ownerId > 0, f => f.OwnerId == ownerId)
                .First();

            if (form == null) return ResultData.No("表单不存在!");
            return ResultData.Ok(form);
        }

        /// <summary>
        /// 创建表单
        /// </summary>
        public ResultData CreateForm(int platformId, string formName, string formFields, bool isRequired)
        {
            int ownerId = ResolveOwnerId();
            if (ownerId <= 0) return ResultData.No("授权失效!");

            if (!_quotaService.CheckFeature(ownerId, "feature_visitor_form"))
                return ResultData.No("未开通「访客信息收集」功能!");

            var form = new VisitorForm
            {
                OwnerId = ownerId,
                PlatformId = platformId,
                FormName = formName,
                FormFields = formFields,
                IsRequired = isRequired,
                IsEnabled = true,
                CreateTime = DateTime.Now
            };

            _fsql.Insert(form).ExecuteAffrows();
            return ResultData.Ok("创建成功!");
        }

        /// <summary>
        /// 更新表单
        /// </summary>
        public ResultData UpdateForm(int formId, string formName, string formFields, bool isRequired, bool isEnabled)
        {
            int ownerId = ResolveOwnerId();
            if (ownerId < 0) return ResultData.No("授权失效!");

            var form = _fsql.Select<VisitorForm>()
                .Where(f => f.Id == formId)
                .WhereIf(ownerId > 0, f => f.OwnerId == ownerId)
                .First();
            if (form == null) return ResultData.No("表单不存在!");

            _fsql.Update<VisitorForm>()
                .Where(f => f.Id == formId)
                .Set(f => f.FormName, formName)
                .Set(f => f.FormFields, formFields)
                .Set(f => f.IsRequired, isRequired)
                .Set(f => f.IsEnabled, isEnabled)
                .Set(f => f.UpdateTime, DateTime.Now)
                .ExecuteAffrows();

            return ResultData.Ok("更新成功!");
        }

        /// <summary>
        /// 删除表单
        /// </summary>
        public ResultData DeleteForm(int formId)
        {
            int ownerId = ResolveOwnerId();
            if (ownerId < 0) return ResultData.No("授权失效!");

            _fsql.Update<VisitorForm>()
                .Where(f => f.Id == formId)
                .WhereIf(ownerId > 0, f => f.OwnerId == ownerId)
                .Set(f => f.IsDeleted, true)
                .ExecuteAffrows();

            return ResultData.Ok("删除成功!");
        }

        /// <summary>
        /// 获取平台的活跃表单配置（访客端调用）
        /// </summary>
        public VisitorForm GetActiveForm(int ownerId, int platformId)
        {
            // 优先查平台级表单，再查全局表单
            var form = _fsql.Select<VisitorForm>()
                .Where(f => f.OwnerId == ownerId && f.PlatformId == platformId && f.IsEnabled)
                .First();

            if (form == null)
            {
                form = _fsql.Select<VisitorForm>()
                    .Where(f => f.OwnerId == ownerId && f.PlatformId == 0 && f.IsEnabled)
                    .First();
            }

            return form;
        }

        /// <summary>
        /// 提交表单数据（访客端）
        /// </summary>
        public ResultData SubmitForm(int formId, string chatUserId, string formData, string ipAddress = "")
        {
            var form = _fsql.Select<VisitorForm>()
                .Where(f => f.Id == formId && f.IsEnabled)
                .First();
            if (form == null) return ResultData.No("表单不存在或已禁用!");

            _fsql.Insert(new VisitorFormSubmission
            {
                FormId = formId,
                OwnerId = form.OwnerId,
                PlatformId = form.PlatformId,
                ChatUserId = chatUserId,
                FormData = formData,
                IpAddress = ipAddress,
                CreateTime = DateTime.Now
            }).ExecuteAffrows();

            return ResultData.Ok("提交成功!");
        }

        /// <summary>
        /// 获取表单提交记录
        /// </summary>
        public ResultData GetSubmissions(int formId, int page = 1, int pageSize = 20)
        {
            int ownerId = ResolveOwnerId();
            if (ownerId < 0) return ResultData.No("授权失效!");

            var data = _fsql.Select<VisitorFormSubmission>()
                .Where(s => s.FormId == formId)
                .WhereIf(ownerId > 0, s => s.OwnerId == ownerId)
                .OrderByDescending(s => s.CreateTime)
                .Count(out long total)
                .Page(page, pageSize)
                .ToList();

            return ResultPage.Ok(data, total, page, pageSize, "查询成功");
        }

        /// <summary>
        /// 解析当前 ownerId：Admin 返回 0（不限制），Owner 返回自身 Id，否则返回 -1
        /// </summary>
        private int ResolveOwnerId()
        {
            var user = _httpContextAccessor.HttpContext.User;
            string userType = user.UserType();
            if (userType == "Admin") return 0;
            int id = user.Id();
            return id > 0 ? id : -1;
        }
    }
}
