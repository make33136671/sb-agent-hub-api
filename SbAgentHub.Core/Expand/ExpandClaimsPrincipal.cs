using System.Linq;
using System.Security.Claims;

namespace SbAgentHub.Core.Expand
{
    public static class ExpandClaimsPrincipal
    {
        /// <summary>
        /// 登录的用户Id
        /// </summary>
        public static int Id(this ClaimsPrincipal claims)
        {
            var value = claims?.FindFirst("Id")?.Value;
            return int.TryParse(value, out int id) ? id : 0;
        }

        /// <summary>
        /// 登录人的用户名
        /// </summary>
        public static string Username(this ClaimsPrincipal claims)
        {
            return claims?.FindFirst("Username")?.Value ?? "";
        }

        /// <summary>
        /// 登录人的头像
        /// </summary>
        public static string Avatar(this ClaimsPrincipal claims)
        {
            return claims?.FindFirst("Avatar")?.Value ?? "";
        }

        /// <summary>
        /// 登录人的角色类型
        /// </summary>
        public static string UserRole(this ClaimsPrincipal claims)
        {
            return claims?.FindFirst("UserRole")?.Value ?? "";
        }
    }
}
