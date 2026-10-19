using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Sb.Apps.AgentHub.Core.Authentication.Jwt
{
    /// <summary>
    /// JWT 令牌创建与验证工具类
    /// </summary>
    public class JwtToken
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 初始化 JwtToken 实例
        /// </summary>
        /// <param name="configuration">配置对象</param>
        /// <exception cref="ArgumentNullException">configuration 为 null 时抛出</exception>
        public JwtToken(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            _configuration = configuration;
        }

        /// <summary>
        /// 创建 JWT 令牌
        /// </summary>
        /// <param name="jwtConfigKey">JWT 配置节点名称</param>
        /// <param name="claims">声明列表</param>
        /// <returns>JWT 令牌字符串</returns>
        /// <exception cref="ArgumentNullException">参数为 null 时抛出</exception>
        public string CreateToken(string jwtConfigKey, List<Claim> claims)
        {
            ArgumentNullException.ThrowIfNull(jwtConfigKey);
            ArgumentNullException.ThrowIfNull(claims);

            IConfigurationSection jwtConfig = _configuration.GetSection(jwtConfigKey);

            string key = jwtConfig.GetSection("Key").Value ?? string.Empty;
            string issuer = jwtConfig.GetSection("Issuer").Value ?? string.Empty;
            string audience = jwtConfig.GetSection("Audience").Value ?? string.Empty;
            string expiresStr = jwtConfig.GetSection("Expires").Value ?? string.Empty;

            if (!int.TryParse(expiresStr, out int expires))
            {
                expires = 120;
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var Sectoken = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.Now.AddMinutes(expires),
                signingCredentials: credentials
              );

            var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

            return token;
        }

        /// <summary>
        /// 解密并验证 JWT 令牌
        /// </summary>
        /// <param name="jwtConfigKey">JWT 配置节点名称</param>
        /// <param name="token">JWT 令牌字符串</param>
        /// <returns>验证通过后的 ClaimsPrincipal</returns>
        /// <exception cref="ArgumentNullException">参数为 null 时抛出</exception>
        public ClaimsPrincipal DecryptToken(string jwtConfigKey, string token)
        {
            ArgumentNullException.ThrowIfNull(jwtConfigKey);
            ArgumentNullException.ThrowIfNull(token);

            IConfigurationSection jwtConfig = _configuration.GetSection(jwtConfigKey);

            string key = jwtConfig.GetSection("Key").Value ?? string.Empty;
            string issuer = jwtConfig.GetSection("Issuer").Value ?? string.Empty;
            string audience = jwtConfig.GetSection("Audience").Value ?? string.Empty;

            var handler = new JwtSecurityTokenHandler();

            var param = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            };

            return handler.ValidateToken(token, param, out SecurityToken validatedToken);
        }
    }
}
