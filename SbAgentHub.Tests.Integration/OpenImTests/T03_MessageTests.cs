using SbAgentHub.Services;
using SbAgentHub.Tests.Integration.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace SbAgentHub.Tests.Integration.OpenImTests
{
    /// <summary>
    /// OpenIM 消息测试 — 发文本、发欢迎语
    /// </summary>
    public class T03_MessageTests
    {
        private readonly ITestOutputHelper _output;
        private readonly OpenImMessageService _messageService;
        private readonly OpenImUserService _userService;
        private readonly OpenImAuthService _authService;

        // 测试用用户ID（每次运行时创建新用户）
        private readonly string _fromUserId;
        private readonly string _toUserId;

        public T03_MessageTests(ITestOutputHelper output)
        {
            _output = output;
            TestBootstrap.EnsureInitialized();
            var cache = TestBootstrap.GetCacheService();
            _authService = new OpenImAuthService(
                new TestOutputLogger<OpenImAuthService>(output), cache);
            _userService = new OpenImUserService(
                new TestOutputLogger<OpenImUserService>(output), _authService);
            _messageService = new OpenImMessageService(
                new TestOutputLogger<OpenImMessageService>(output), _authService);

            _fromUserId = $"test_msg_from_{Guid.NewGuid():N}"[..32];
            _toUserId = $"test_msg_to_{Guid.NewGuid():N}"[..32];
        }

        private async Task EnsureUsersRegistered()
        {
            await _userService.RegisterUserAsync(_fromUserId, "发送者");
            await _userService.RegisterUserAsync(_toUserId, "接收者");
        }

        [Fact]
        public async Task SendTextMessage_ReturnsSuccess()
        {
            // Arrange
            await EnsureUsersRegistered();

            // Act
            var result = await _messageService.SendTextMessageAsync(
                _fromUserId, _toUserId, "集成测试消息 - Hello OpenIM!");

            // Assert
            Assert.True(result, "发送文本消息应返回成功");
            _output.WriteLine($"✓ 文本消息发送成功: {_fromUserId} -> {_toUserId}");
        }

        [Fact]
        public async Task SendTextMessage_EmptyText_ReturnsFalse()
        {
            // Act — 空文本应被本地校验拦截，不需要注册用户
            var result = await _messageService.SendTextMessageAsync(
                "any_from", "any_to", "");

            // Assert
            Assert.False(result, "空文本消息应返回 false（本地校验）");
            _output.WriteLine("✓ 空文本消息被正确拒绝");
        }

        [Fact]
        public async Task SendWelcomeMessage_ReturnsSuccess()
        {
            // Arrange
            await EnsureUsersRegistered();

            // Act
            var result = await _messageService.SendWelcomeMessageAsync(
                _fromUserId, _toUserId, "欢迎来到客服系统！有什么可以帮您的？");

            // Assert
            Assert.True(result, "发送欢迎语应返回成功");
            _output.WriteLine($"✓ 欢迎语发送成功: {_fromUserId} -> {_toUserId}");
        }
    }
}
