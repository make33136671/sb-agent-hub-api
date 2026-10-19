using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;
using System.Text.Json;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 套餐模板服务
    /// </summary>
    [AutoDependencyInjection]
    public class QuotaPackageService
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Lazy<QuotaService> _quotaService;

        public QuotaPackageService(IFreeSql fsql, IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            // 惰性解析打破构造循环：QuotaService → MemberService → QuotaPackageService → QuotaService
            _quotaService = new Lazy<QuotaService>(() => serviceProvider.GetRequiredService<QuotaService>());
        }

        /// <summary>
        /// 获取套餐列表
        /// </summary>
        public ResultData GetPackageList(bool enabledOnly = false)
        {
            var query = _fsql.Select<QuotaPackage>();
            if (enabledOnly)
                query = query.Where(p => p.IsEnabled);

            var list = query.OrderBy(p => p.SortOrder).ToList(p => new
            {
                p.Id,
                p.PackageName,
                p.PackageKey,
                p.Description,
                p.Price,
                p.PricePeriod,
                p.QuotaSettings,
                p.SortOrder,
                p.IsEnabled,
                p.CreateTime,
                p.UpdateTime
            });

            return ResultData.Ok(list);
        }

        /// <summary>
        /// 获取套餐详情
        /// </summary>
        public ResultData GetPackageDetail(int id)
        {
            var pkg = _fsql.Select<QuotaPackage>().Where(p => p.Id == id).First();
            if (pkg == null)
                return ResultData.No("套餐不存在!");

            return ResultData.Ok(new
            {
                pkg.Id,
                pkg.PackageName,
                pkg.PackageKey,
                pkg.Description,
                pkg.Price,
                pkg.PricePeriod,
                pkg.QuotaSettings,
                pkg.SortOrder,
                pkg.IsEnabled
            });
        }

        /// <summary>
        /// 创建套餐
        /// </summary>
        public ResultData CreatePackage(string packageName, string packageKey, string description,
            decimal price, string pricePeriod, string quotaSettings, int sortOrder, bool isEnabled)
        {
            // 检查 key 唯一
            var exists = _fsql.Select<QuotaPackage>()
                .Where(p => p.PackageKey == packageKey).Any();
            if (exists)
                return ResultData.No("套餐标识已存在!");

            var pkg = new QuotaPackage
            {
                PackageName = packageName,
                PackageKey = packageKey,
                Description = description,
                Price = price,
                PricePeriod = pricePeriod,
                QuotaSettings = quotaSettings,
                SortOrder = sortOrder,
                IsEnabled = isEnabled,
                CreateTime = DateTime.Now
            };

            _fsql.Insert(pkg).ExecuteAffrows();
            return ResultData.Ok("创建成功!");
        }

        /// <summary>
        /// 更新套餐
        /// </summary>
        public ResultData UpdatePackage(int id, string packageName, string description,
            decimal price, string pricePeriod, string quotaSettings, int sortOrder, bool isEnabled)
        {
            var pkg = _fsql.Select<QuotaPackage>().Where(p => p.Id == id).First();
            if (pkg == null)
                return ResultData.No("套餐不存在!");

            pkg.PackageName = packageName;
            pkg.Description = description;
            pkg.Price = price;
            pkg.PricePeriod = pricePeriod;
            pkg.QuotaSettings = quotaSettings;
            pkg.SortOrder = sortOrder;
            pkg.IsEnabled = isEnabled;
            pkg.UpdateTime = DateTime.Now;

            _fsql.Update<QuotaPackage>().SetSource(pkg)
                .UpdateColumns(p => new
                {
                    p.PackageName, p.Description, p.Price, p.PricePeriod,
                    p.QuotaSettings, p.SortOrder, p.IsEnabled, p.UpdateTime
                }).ExecuteAffrows();

            return ResultData.Ok("更新成功!");
        }

        /// <summary>
        /// 删除套餐（软删除）
        /// </summary>
        public ResultData DeletePackage(int id)
        {
            var pkg = _fsql.Select<QuotaPackage>().Where(p => p.Id == id).First();
            if (pkg == null)
                return ResultData.No("套餐不存在!");

            _fsql.Update<QuotaPackage>(id)
                .Set(p => p.IsDeleted, true)
                .ExecuteAffrows();

            return ResultData.Ok("删除成功!");
        }

        #region 仓储访问入口（供其他 Service 调用）

        /// <summary>
        /// 按主键获取套餐
        /// </summary>
        public QuotaPackage GetById(int id)
        {
            return _fsql.Select<QuotaPackage>().Where(p => p.Id == id).First();
        }

        /// <summary>
        /// 按 PackageKey 获取启用的套餐（供 MemberService 试用套餐分配用）
        /// </summary>
        public QuotaPackage GetEnabledByPackageKey(string packageKey)
        {
            return _fsql.Select<QuotaPackage>()
                .Where(p => p.PackageKey == packageKey && p.IsEnabled && !p.IsDeleted)
                .First();
        }

        /// <summary>
        /// 按主键获取启用的套餐
        /// </summary>
        public QuotaPackage GetEnabledById(int id)
        {
            return _fsql.Select<QuotaPackage>()
                .Where(p => p.Id == id && p.IsEnabled)
                .First();
        }

        /// <summary>
        /// 获取所有启用的套餐（供套餐对比用）
        /// </summary>
        public List<QuotaPackage> GetEnabledPackages()
        {
            return _fsql.Select<QuotaPackage>()
                .Where(p => p.IsEnabled)
                .OrderBy(p => p.SortOrder)
                .ToList();
        }

        #endregion

        /// <summary>
        /// 应用套餐到站长（批量设置配额）
        /// </summary>
        public ResultData ApplyPackage(int packageId, int memberId)
        {
            var pkg = GetEnabledById(packageId);
            if (pkg == null)
                return ResultData.No("套餐不存在或未启用!");

            // 解析 QuotaSettings JSON
            Dictionary<string, int> settings;
            try
            {
                settings = JsonSerializer.Deserialize<Dictionary<string, int>>(pkg.QuotaSettings)
                    ?? new Dictionary<string, int>();
            }
            catch
            {
                return ResultData.No("套餐配额配置格式错误!");
            }

            if (!settings.Any())
                return ResultData.No("套餐未配置任何配额!");

            var quotaItems = settings.Select(kv => new QuotaItem
            {
                QuotaKey = kv.Key,
                QuotaValue = kv.Value
            }).ToList();

            var remark = $"应用套餐: {pkg.PackageName}";
            return _quotaService.Value.BatchSetMemberQuotas(memberId, quotaItems, remark);
        }

        /// <summary>
        /// 获取套餐对比数据
        /// </summary>
        public ResultData GetPackageComparison()
        {
            var packages = GetEnabledPackages();

            var configs = _quotaService.Value.GetEnabledConfigs();

            var result = new
            {
                quotaConfigs = configs.Select(c => new
                {
                    c.QuotaKey,
                    c.QuotaName,
                    c.QuotaType,
                    c.Unit
                }),
                packages = packages.Select(p =>
                {
                    Dictionary<string, int> settings;
                    try
                    {
                        settings = JsonSerializer.Deserialize<Dictionary<string, int>>(p.QuotaSettings)
                            ?? new Dictionary<string, int>();
                    }
                    catch
                    {
                        settings = new Dictionary<string, int>();
                    }

                    return new
                    {
                        p.Id,
                        p.PackageName,
                        p.PackageKey,
                        p.Price,
                        p.PricePeriod,
                        p.Description,
                        quotas = settings
                    };
                })
            };

            return ResultData.Ok(result);
        }
    }
}
