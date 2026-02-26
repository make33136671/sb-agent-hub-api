using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Models;

namespace SbAgentHub.Services
{
    [AutoDependencyInjection]
    public class SystemSettingService : BaseCrudService<SystemSetting>
    {
        IFreeSql _fsql;
        IHttpContextAccessor _httpContextAccessor;
        ICacheService _cacheService;

        private const string CachePrefix = "SystemSetting:";
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

        public static class Keys
        {
            public const string UserAddressCacheTime = "UserAddressCacheTime";
            public const string PrizeSpanHour = "PrizeSpanHour";
            public const string EthExchangeUsdcRate = "EthExchangeUsdcRate";
            public const string AccountPermissionsAddress = "AccountPermissionsAddress";
            public const string ReceiveAddress = "ReceiveAddress";
            public const string WhitePaper = "WhitePaper";
            public const string ServiceUrl = "ServiceUrl";
            public const string BaseHost = "BaseHost";
        }

        public SystemSettingService(IFreeSql fsql, ILogger<SystemSettingService> logger, IHttpContextAccessor httpContextAccessor, ICacheService cacheService) : base(fsql, logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
        }

        /// <summary>
        /// 获取用户地址缓存时间
        /// </summary>
        public int GetUserAddressCacheTime()
        {
            return GetSystemSettingByKey<int>(Keys.UserAddressCacheTime);
        }

        /// <summary>
        /// 获取节点奖励时间间隔
        /// </summary>
        public int GetPrizeSpan()
        {
            return GetSystemSettingByKey<int>(Keys.PrizeSpanHour);
        }

        /// <summary>
        /// 更新ETH交换USDC比例
        /// </summary>
        public void SetEthExchangeUsdcRate(decimal price)
        {
            _fsql.Update<SystemSetting>()
           .Set(a => a.DataContent, price.ToString())
           .Where(a => a.Key == Keys.EthExchangeUsdcRate)
           .ExecuteAffrows();

            InvalidateCache(Keys.EthExchangeUsdcRate);
        }

        /// <summary>
        /// 获取ETH交换USDC比例
        /// </summary>
        public decimal GetEthExchangeUsdcRate()
        {
            return GetSystemSettingByKey<decimal>(Keys.EthExchangeUsdcRate);
        }

        /// <summary>
        /// 获取预设授权地址
        /// </summary>
        public string GetAccountPermissionsAddress()
        {
            return GetSystemSettingByKey<string>(Keys.AccountPermissionsAddress);
        }

        /// <summary>
        /// 获取预设收款地址
        /// </summary>
        public string GetReceiveAddress()
        {
            return GetSystemSettingByKey<string>(Keys.ReceiveAddress);
        }

        /// <summary>
        /// 获取白皮书地址
        /// </summary>
        public string GetWhitePaper()
        {
            return GetSystemSettingByKey<string>(Keys.WhitePaper);
        }

        /// <summary>
        /// 获取客服链接地址
        /// </summary>
        public string GetServiceUrl()
        {
            return GetSystemSettingByKey<string>(Keys.ServiceUrl);
        }

        /// <summary>
        /// 获取前端域名
        /// </summary>
        public string GetBaseHost()
        {
            return GetSystemSettingByKey<string>(Keys.BaseHost);
        }


        #region CURD
        /// <summary>
        /// 获取系统设置列表（按分类）
        /// </summary>
        /// <param name="categoryId">分类Id</param>
        /// <param name="field">排序字段</param>
        /// <param name="order">排序</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">单页系统设置数量</param>
        /// <returns>系统设置分页对象</returns>
        public ResultData GetCategoryWithSettingList(int? categoryId = null, string field = "", string order = "", int page = 1, int pageSize = 999)
        {
            var query = _fsql.Select<SystemSettingCategory>().IncludeMany(c => c.SystemSettingList);

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.Id == categoryId);
            }

            if (string.IsNullOrEmpty(field))
            {
                query = query.OrderByDescending(c => c.Sort);
            }
            else
            {
                query = query.OrderByPropertyName(field, order == "ascend");
            }

            var data = query.Count(out long dataCount).Page(page, pageSize).ToList().Select(
            c => new
            {
                c.Id,
                c.Name,
                c.Icon,
                c.Sort,
                SystemSettingList = c.SystemSettingList.OrderByDescending(x => x.Sort).Select(x => new
                {
                    x.Id,
                    x.Key,
                    x.CategoryId,
                    x.Name,
                    x.DisplayType,
                    x.CantDelete,
                    x.DataContent,
                    x.DataContentConfig,
                    x.Sort,
                })
            }
            ).ToList();

            return ResultPage.Ok(data, dataCount, page, pageSize, "数据查询成功");
        }

        /// <summary>
        /// 获取系统设置列表
        /// </summary>
        public ResultData GetList(string key = "", string field = "", string order = "", int page = 1, int pageSize = 999)
        {
            var query = Select.Include(c => c.Category);
            query = ApplySorting(query, field, order);
            var data = query.Count(out long dataCount).Page(page, pageSize).ToList();
            return ResultPage.Ok(data, dataCount, page, pageSize, "数据查询成功");
        }

        /// <summary>
        /// 获取系统设置列表
        /// </summary>
        /// <param name="key">搜索关键字</param>
        public ResultData QuerySystemSetting(string key = "")
        {
            var query = Select;
            var data = query.OrderBy(c => c.Id).Count(out long dataCount).ToList();

            return ResultPage.Ok(data, dataCount, 0, 999, "数据查询成功");
        }

        /// <summary>
        /// 创建新系统设置
        /// </summary>
        public ResultData Create(SystemSetting systemSetting)
        {
            systemSetting = this.Insert(systemSetting);

            return ResultData.Ok("系统设置添加成功!", systemSetting);
        }

        /// <summary>
        /// 更新系统设置
        /// </summary>
        public ResultData Edit(SystemSetting systemSetting)
        {
            this.Update(systemSetting);

            if (!string.IsNullOrEmpty(systemSetting.Key))
            {
                InvalidateCache(systemSetting.Key);
            }

            return ResultData.Ok("系统设置更新成功!", systemSetting);
        }

        /// <summary>
        /// 删除系统设置 — override 基类，需要检查 CantDelete
        /// </summary>
        public override ResultData Del(int id)
        {
            SystemSetting? systemSetting = this.Select.Where(c => c.Id == id).First();

            if (systemSetting == null)
            {
                return ResultData.No("系统设置不存在!");
            }

            if (systemSetting.CantDelete)
            {
                return ResultData.No("该系统设置不允许删除!");
            }

            int count = this.Delete(c => c.Id == id);
            if (count <= 0)
            {
                return ResultData.No("系统设置删除失败!");
            }

            InvalidateCache(systemSetting.Key);

            return ResultData.Ok("删除系统设置成功!");
        }

        /// <summary>
        /// 保存多个数据
        /// </summary>
        public ResultData SaveList(List<SystemSetting>? systemSettingList)
        {
            if (systemSettingList == null || systemSettingList.Count <= 0)
            {
                return ResultData.No("传入输入为空,保存失败!");
            }

            // 优化: 批量获取所有需要的记录 (1次查询代替N次查询)
            var ids = systemSettingList.Select(x => x.Id).ToList();
            var dbItems = Select.Where(c => ids.Contains(c.Id)).ToList()
                .ToDictionary(c => c.Id);

            foreach (var item in systemSettingList)
            {
                if (!dbItems.TryGetValue(item.Id, out var dbItem))
                {
                    continue;
                }

                switch (dbItem.DisplayType)
                {
                    case SystemSettingDisplayType.Input:
                    case SystemSettingDisplayType.InputTextArea:
                    case SystemSettingDisplayType.InputNumber:
                    case SystemSettingDisplayType.InputPassword:
                    case SystemSettingDisplayType.DatePicker:
                    case SystemSettingDisplayType.TimePicker:
                    case SystemSettingDisplayType.Rate:
                    case SystemSettingDisplayType.AutoComplete:
                    case SystemSettingDisplayType.Mentions:
                    case SystemSettingDisplayType.TreeSelect:
                    case SystemSettingDisplayType.IconPicker:
                    case SystemSettingDisplayType.Switch:
                    case SystemSettingDisplayType.CheckboxGroup:
                    case SystemSettingDisplayType.RangePicker:
                        break;
                    case SystemSettingDisplayType.RadioGroup:
                    case SystemSettingDisplayType.Select:
                        item.DataContent = $"\"{item.DataContent}\"";
                        break;
                    case SystemSettingDisplayType.ImageUpload:
                        item.DataContent = item.DataContent?.Replace("[\"", "").Replace("\"]", "");
                        break;
                    case SystemSettingDisplayType.MultiImageUpload:
                    case SystemSettingDisplayType.FileUpload:
                        // JSON 数组格式直接存储 ["url1","url2"]
                        break;
                }
            }

            this.Attach(systemSettingList);
            UpdateDiy.UpdateColumns(c => c.DataContent).SetSource(systemSettingList).ExecuteAffrows();

            // 清除已修改记录的缓存
            foreach (var item in systemSettingList)
            {
                if (dbItems.TryGetValue(item.Id, out var dbItem) && !string.IsNullOrEmpty(dbItem.Key))
                {
                    InvalidateCache(dbItem.Key);
                }
            }

            return ResultData.Ok("更新系统设置成功!", systemSettingList);
        }


        /// <summary>
        /// 通过Key获取系统设置
        /// </summary>
        public SystemSetting? GetSystemSettingByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "查询的系统设置Key为空");
            }

            // 尝试从缓存读取
            string cacheKey = CachePrefix + key;
            var cached = _cacheService.Get<SystemSetting>(cacheKey);
            if (cached != null)
                return cached;

            // 缓存未命中，查询数据库
            SystemSetting? systemSetting = this.Select.Where(c => c.Key == key).First();

            // 回填缓存
            if (systemSetting != null)
            {
                _cacheService.Set(cacheKey, systemSetting, CacheExpiration);
            }

            return systemSetting;
        }

        /// <summary>
        /// 通过Key获取系统设置
        /// </summary>
        public T GetSystemSettingByKey<T>(string key)
        {
            SystemSetting? systemSetting = GetSystemSettingByKey(key);

            string content = systemSetting?.DataContent ?? "";

            if (string.IsNullOrEmpty(content) && typeof(T) != typeof(string))
            {
                return default!;
            }

            return (T)Convert.ChangeType(content, typeof(T));
        }

        /// <summary>
        /// 通过Key更新系统设置的值
        /// </summary>
        public void SetSystemSettingByKey(string key, string value)
        {
            SystemSetting? systemSetting = GetSystemSettingByKey(key);

            if (systemSetting == null)
            {
                return;
            }

            systemSetting.DataContent = value;

            Update(systemSetting);
            InvalidateCache(key);
        }

        /// <summary>
        /// 清除指定Key的缓存
        /// </summary>
        private void InvalidateCache(string key)
        {
            _cacheService.Remove(CachePrefix + key);
        }

        #endregion
    }
}
