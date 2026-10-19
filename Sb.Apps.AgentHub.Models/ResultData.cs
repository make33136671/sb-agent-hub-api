namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 通用响应数据
    /// </summary>
    public class ResultData
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
        public object Data { get; set; } = null!;

        /// <summary>
        /// 成功的构造函数
        /// </summary>
        /// <param name="data">响应的数据</param>
        /// <returns>请求结果响应对象</returns>
        public static ResultData Ok(object data)
        {
            return Ok("ok", data);
        }

        /// <summary>
        /// 成功的构造函数
        /// </summary>
        /// <param name="message">响应消息</param>
        /// <param name="data">响应的数据</param>
        /// <returns>请求结果响应对象</returns>
        public static ResultData Ok(string message = "ok", object data = null!)
        {
            ResultData result = new ResultData();
            result.Code = 0;
            result.Message = message;
            result.Data = data;
            result.Type = "success";
            return result;
        }

        /// <summary>
        /// 失败的构造函数
        /// </summary>
        /// <param name="message">失败消息</param>
        /// <param name="data">响应的数据</param>
        /// <returns>请求结果响应对象</returns>
        public static ResultData No(string message = "Failed", object data = null!)
        {
            ResultData result = new ResultData();
            result.Message = message;
            result.Code = -1;
            result.Data = data;
            result.Type = "error";
            return result;
        }
    }

    /// <summary>
    /// 泛型通用响应数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class ResultData<T> : ResultData where T : class
    {
        /// <summary>
        /// 返回数据
        /// </summary>
        public T Result { get; set; } = null!;


        /// <summary>
        /// 成功的构造函数
        /// </summary>
        /// <param name="message">响应消息</param>
        /// <param name="data">响应的数据</param>
        /// <returns>请求结果响应对象</returns>
        public static ResultData<T> Ok(string message = "Successfully", T data = null!)
        {
            ResultData<T> result = new ResultData<T>();
            result.Message = message;
            result.Code = 0;
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 失败的构造函数
        /// </summary>
        /// <param name="message">失败消息</param>
        /// <param name="data">响应的数据</param>
        /// <returns>请求结果响应对象</returns>
        public static ResultData<T> No(string message = "Failed", T data = null!)
        {
            ResultData<T> result = new ResultData<T>();
            result.Message = message;
            result.Code = -1;
            result.Data = data;
            return result;
        }
    }
}
