using FreeSql;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Models;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 客户端错误日志服务
    /// </summary>
    [AutoDependencyInjection]
    public class ClientErrorLogService
    {
        private readonly IFreeSql _fsql;
        private readonly ILogger<ClientErrorLogService> _logger;

        public ClientErrorLogService(IFreeSql fsql, ILogger<ClientErrorLogService> logger)
        {
            _fsql = fsql;
            _logger = logger;
        }

        /// <summary>
        /// 记录客户端错误
        /// </summary>
        public ResultData Report(ClientErrorLog log)
        {
            if (string.IsNullOrEmpty(log.ErrorMessage))
                return ResultData.No("错误信息不能为空!");

            log.CreateTime = DateTime.Now;
            log.IsDeleted = false;
            _fsql.Insert(log).ExecuteAffrows();

            return ResultData.Ok("错误已记录");
        }
    }
}
