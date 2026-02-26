using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Models;

namespace SbAgentHub.Services
{
    [AutoDependencyInjection]
    public class CommonService
    {
        IFreeSql _fsql;
        ILogger<CommonService> _logger;
        IHttpContextAccessor _httpContextAccessor;
        public CommonService(IFreeSql fsql, ILogger<CommonService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _fsql = fsql;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
    }
}
