using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SbAgentHub.Models
{
    public class ResultPage : ResultData
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 请求数据条数
        /// </summary>
        public int PageSize { get; set; }



        /// <summary>
        /// 数据总条数
        /// </summary>
        private long _Total;
        public long Total { get { return _Total; } set { _Total = value; } }

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// 合计行数据
        /// </summary>
        public object TotalRow { get; set; }

        /// <summary>
        /// 有上一页
        /// </summary>
        public bool HasPreviousPage => Page > 1;

        /// <summary>
        /// 有下一页
        /// </summary>
        public bool HasNextPage => PageCount == 1 ? false : Page < PageCount;

        /// <summary>
        /// 分页数据
        /// </summary>
        public object Items { get; set; }

        /// <summary>
        /// 分页数据
        /// </summary>
        public object Result => Items;

        /// <summary>
        /// 成功的构造函数
        /// </summary>
        /// <param name="data">响应的数据</param>
        /// <param name="total">数据总条数</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <returns>请求结果响应对象</returns>
        public static ResultData Ok(object data, long total, int page, int pageSize, string msg = "请求成功")
        {
            ResultPage result = new ResultPage();
            result.Total = total;
            result.Page = page;
            result.PageSize = pageSize;


            if (result.Total >= 1)
            {
                result.PageCount = Convert.ToInt32(Math.Ceiling(result.Total / (result.PageSize * 1.0)));
            }
            else
            {
                result.PageCount = 1;
            }


            result.Items = data;
            return ResultData.Ok(result);
        }

        /// <summary>
        /// 成功的构造函数
        /// </summary>
        /// <param name="data">响应的数据</param>
        /// <param name="total">数据总条数</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <returns>请求结果响应对象</returns>
        public static ResultData PageOk(object data, long total, int page, int pageSize, string msg = "请求成功")
        {
            ResultPage result = new ResultPage();

            result.Code = 0;
            result.Message = msg;

            result.Total = total;
            result.Page = page;
            result.PageSize = pageSize;


            if (result.Total >= 1)
            {
                result.PageCount = Convert.ToInt32(Math.Ceiling(result.Total / (result.PageSize * 1.0)));
            }
            else
            {
                result.PageCount = 1;
            }


            result.Items = data;
            return result;
        }
    }
}
