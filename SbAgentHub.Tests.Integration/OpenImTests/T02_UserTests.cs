using SbAgentHub.Services;
using SbAgentHub.Tests.Integration.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace SbAgentHub.Tests.Integration.OpenImTests
{
    /// <summary>
    /// OpenIM 用户测试 — 注册、查询、更新
    /// </summary>
    public class T02_UserTests
    {
        private readonly ITestOutputHelper _output;
        private readonly OpenImUserService _userService;
        private readonly OpenImAuthService _authService;

        public T02_UserTests(ITestOutputHelper output)
        {
            _output = output;
            TestBootstrap.EnsureInitialized();
            var cache = TestBootstrap.GetCacheService();
            _authService = new OpenImAuthService(
                new TestOutputLogger<OpenImAuthService>(output), cache);
            _userService = new OpenImUserService(
                new TestOutputLogger<OpenImUserService>(output), _authService);
        }

        [Fact]
        public async Task RegisterUser_ReturnsSuccess()
        {
            // Arrange
            var userId = $"test_reg_{Guid.NewGuid():N}"[..32];

            // Act
            var result = await _userService.RegisterUserAsync(userId, "测试用户_注册");

            // Assert
            Assert.True(result, "用户注册应返回成功");
            _output.WriteLine($"✓ 用户注册成功: {userId}");
        }

        [Fact]
        public async Task RegisterUser_DuplicateStillReturnsSuccess()
        {
            // Arrange
            var userId = $"test_dup_{Guid.NewGuid():N}"[..32];

            // Act
            var result1 = await _userService.RegisterUserAsync(userId, "测试用户_重复注册");
            var result2 = await _userService.RegisterUserAsync(userId, "测试用户_重复注册");

            // Assert
            Assert.True(result1, "首次注册应成功");
            Assert.True(result2, "重复注册应仍返回成功（errCode 11001-11099）");
            _output.WriteLine($"✓ 重复注册处理正确: {userId}");
        }

        [Fact]
        public async Task CheckUserExists_RegisteredUser_ReturnsTrue()
        {
            // Arrange
            var userId = $"test_exist_{Guid.NewGuid():N}"[..32];
            await _userService.RegisterUserAsync(userId, "测试用户_查询存在");

            // Act
            var exists = await _userService.CheckUserExistsAsync(userId);

            // Assert
            Assert.True(exists, "已注册用户应返回 true");
            _output.WriteLine($"✓ 已注册用户存在: {userId}");
        }

        [Fact]
        public async Task CheckUserExists_NonExistentUser_ReturnsFalse()
        {
            // Arrange
            var userId = $"nonexist_{Guid.NewGuid():N}"[..32];

            // Act
            var exists = await _userService.CheckUserExistsAsync(userId);

            // Assert
            Assert.False(exists, "不存在的用户应返回 false");
            _output.WriteLine($"✓ 不存在用户查询正确: {userId}");
        }

        [Fact]
        public async Task UpdateUserInfo_ReturnsSuccess()
        {
            // Arrange
            var userId = $"test_upd_{Guid.NewGuid():N}"[..32];
            await _userService.RegisterUserAsync(userId, "原始昵称");

            // Act
            var result = await _userService.UpdateUserInfoAsync(userId, "更新后昵称");

            // Assert
            Assert.True(result, "更新用户信息应返回成功");
            _output.WriteLine($"✓ 用户信息更新成功: {userId}");
        }

        [Fact]
        public async Task RegisterAndGetToken_ReturnsValidTokenData()
        {
            // Arrange
            var userId = $"test_rgt_{Guid.NewGuid():N}"[..32];

            // Act
            var tokenData = await _userService.RegisterAndGetTokenAsync(userId, "测试用户_注册获取Token");

            // Assert
            Assert.NotNull(tokenData);
            Assert.False(string.IsNullOrEmpty(tokenData.Token), "Token 不应为空");
            Assert.True(tokenData.ExpireTimeSeconds > 0, "过期时间应 > 0");
            _output.WriteLine($"✓ 注册并获取Token成功: {userId}");
            _output.WriteLine($"  Token: {tokenData.Token[..Math.Min(30, tokenData.Token.Length)]}...");
            _output.WriteLine($"  过期: {tokenData.ExpireTimeSeconds} 秒");
        }
    }
}
