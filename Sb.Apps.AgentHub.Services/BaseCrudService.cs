using FreeSql;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Models;
using System.Linq.Expressions;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 通用 CRUD 基类 — 提供 Detail、Del、排序和分页的默认实现
    /// </summary>
    public class BaseCrudService<T> : BaseRepository<T> where T : class, IEntity
    {
        protected readonly ILogger _logger;

        public BaseCrudService(IFreeSql fsql, ILogger logger) : base(fsql)
        {
            _logger = logger;
        }

        /// <summary>
        /// 通用详情查询 — 子类可 override 添加 Include()
        /// </summary>
        public virtual ResultData Detail(int id)
        {
            T entity = Select.Where(c => c.Id == id).First();

            if (entity == null)
            {
                return ResultData.No("数据不存在,请确认操作是否正确!");
            }

            return ResultData.Ok(entity);
        }

        /// <summary>
        /// 通用删除 — 自动检测 ISoftDelete 走软删除，否则硬删除
        /// </summary>
        public virtual ResultData Del(int id)
        {
            T entity = Select.Where(c => c.Id == id).First();

            if (entity == null)
            {
                return ResultData.No("数据不存在,删除失败!");
            }

            if (entity is ISoftDelete softDelete)
            {
                softDelete.IsDeleted = true;
                int count = Update(entity);

                if (count <= 0)
                {
                    return ResultData.No("删除失败!");
                }
            }
            else
            {
                int count = Delete(c => c.Id == id);
                if (count <= 0)
                {
                    return ResultData.No("删除失败!");
                }
            }

            return ResultData.Ok("删除成功!");
        }

        /// <summary>
        /// 排序助手 — 空字段默认 Id DESC
        /// </summary>
        protected ISelect<T> ApplySorting(ISelect<T> query, string field, string order)
        {
            if (string.IsNullOrEmpty(field))
            {
                return query.OrderByDescending(c => c.Id);
            }

            return query.OrderByPropertyName(field, order == "ascend");
        }

        /// <summary>
        /// 分页助手 — 全量返回
        /// </summary>
        protected ResultData PagedResult(ISelect<T> query, int page, int pageSize)
        {
            var data = query.Count(out long dataCount).Page(page, pageSize).ToList();
            return ResultPage.Ok(data, dataCount, page, pageSize, "数据查询成功");
        }

        /// <summary>
        /// 分页助手 — 带投影
        /// </summary>
        protected ResultData PagedResult<TDto>(ISelect<T> query, int page, int pageSize,
            Expression<Func<T, TDto>> selector)
        {
            var data = query.Count(out long dataCount).Page(page, pageSize).ToList(selector);
            return ResultPage.Ok(data, dataCount, page, pageSize, "数据查询成功");
        }
    }
}
