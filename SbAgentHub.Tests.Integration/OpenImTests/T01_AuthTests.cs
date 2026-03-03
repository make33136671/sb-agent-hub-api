using SbAgentHub.Services;
using SbAgentHub.Tests.Integration.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace SbAgentHub.Tests.Integration.OpenImTests
{
    /// <summary>
    /// OpenIM 认证测试 — 管理员Token、用户Token
    /// </summary>
    public class T01_AuthTests
    {
        private readonly ITestOutputHelper _output;
        private readonly OpenImAuthService _authService;

        public T01_AuthTests(ITestOutputHelper output)
        {
            _output = output;
            TestBootstrap.EnsureInitialized();
            var cache = TestBootstrap.GetCacheService();
            _authService = new OpenImAuthService(
                new TestOutputLogger<OpenImAuthService>(output), cache);
        }

        [Fact]
        public async Task GetAdminToken_ReturnsNonEmptyToken()
        {
            // Act
            var token = await _authService.GetAdminTokenAsync();

            // Assert
            Assert.False(string.IsNullOrEmpty(token), "管理员 Token 不应为空");
            _output.WriteLine($"✓ 管理员 Token: {token[..Math.Min(30, token.Length)]}...");
        }

        [Fact]
        public async Task GetAdminToken_SecondCallHitsCache()
        {
            // Act
            var token1 = await _authService.GetAdminTokenAsync();
            var token2 = await _authService.GetAdminTokenAsync();

            // Assert
            Assert.Equal(token1, token2);
            _output.WriteLine($"✓ 两次调用 Token 一致（缓存命中）: {token1[..Math.Min(30, token1.Length)]}...");
        }

        [Fact]
        public async Task GetUserToken_ReturnsValidTokenData()
        {
            // Arrange
            var testUserId = $"test_auth_{Guid.NewGuid():N}"[..32];

            // 先注册用户，确保用户存在
            var userService = new OpenImUserService(
                new TestOutputLogger<OpenImUserService>(_output), _authService);
            await userService.RegisterUserAsync(testUserId, "认证测试用户");

            // Act
            var tokenData = await _authService.GetUserTokenAsync(testUserId);

            // Assert
            Assert.NotNull(tokenData);
            Assert.False(string.IsNullOrEmpty(tokenData.Token), "用户 Token 不应为空");
            Assert.True(tokenData.ExpireTimeSeconds > 0, "Token 过期时间应 > 0");
            _output.WriteLine($"✓ 用户 Token: {tokenData.Token[..Math.Min(30, tokenData.Token.Length)]}...");
            _output.WriteLine($"✓ 过期时间: {tokenData.ExpireTimeSeconds} 秒");
        }
    }
}
