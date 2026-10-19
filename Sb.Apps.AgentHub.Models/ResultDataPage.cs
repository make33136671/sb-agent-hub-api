namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 通用分页响应数据
    /// </summary>
    public class ResultDataPage
    {
        /// <summary>
        /// 请求响应消息
        /// </summary>
        private string _message = null!;
        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }

            set
            {
                _message = value;
            }
        }

        /// <summary>
        /// 响应类型
        /// </summary>
        private string _type = null!;
        /// <summary>
        /// 响应类型
        /// </summary>
        public string Type
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }

        /// <summary>
        /// 状态码
        /// </summary>
        public int Code { get; set; } = 0;

        /// <summary>
        /// 返回数据
        /// </summary>
        public object Result { get; set; } = null!;

        /// <summary>
        /// 成功的构造函数
        /// </summary>
        /// <param name="data">响应的数据</param>
        /// <returns>请求结果响应对象</returns>
        public static ResultDataPage Ok(object data)
        {
            return Ok("ok", data);
        }

        /// <summary>
        /// 成功的构造函数
        /// </summary>
        /// <param name="message">响应消息</param>
        /// <param name="data">响应的数据</param>
        /// <returns>请求结果响应对象</returns>
        public static ResultDataPage Ok(string message = "ok", object data = null!)
        {
            ResultDataPage result = new ResultDataPage();
            result.Code = 0;
            result.Message = message;
            result.Result = data;
            result.Type = "success";
            return result;
        }

        /// <summary>
        /// 失败的构造函数
        /// </summary>
        /// <param name="message">失败消息</param>
        /// <param name="data">响应的数据</param>
        /// <returns>请求结果响应对象</returns>
        public static ResultDataPage No(string message = "Failed", object data = null!)
        {
            ResultDataPage result = new ResultDataPage();
            result.Message = message;
            result.Code = -1;
            result.Result = data;
            result.Type = "error";
            return result;
        }
    }

    /// <summary>
    /// 泛型通用分页响应数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class ResultDataPage<T> : ResultDataPage where T : class
    {
        /// <summary>
        /// 返回数据
        /// </summary>
        public new T Result { get; set; } = null!;


        /// <summary>
        /// 成功的构造函数
        /// </summary>
        /// <param name="message">响应消息</param>
        /// <param name="data">响应的数据</param>
        /// <returns>请求结果响应对象</returns>
        public static ResultDataPage<T> Ok(string message = "Successfully", T data = null!)
        {
            ResultDataPage<T> result = new ResultDataPage<T>();
            result.Message = message;
            result.Code = 0;
            result.Result = data;
            return result;
        }

        /// <summary>
        /// 失败的构造函数
        /// </summary>
        /// <param name="message">失败消息</param>
        /// <param name="data">响应的数据</param>
        /// <returns>请求结果响应对象</returns>
        public static ResultDataPage<T> No(string message = "Failed", T data = null!)
        {
            ResultDataPage<T> result = new ResultDataPage<T>();
            result.Message = message;
            result.Code = -1;
            result.Result = data;
            return result;
        }
    }
}
