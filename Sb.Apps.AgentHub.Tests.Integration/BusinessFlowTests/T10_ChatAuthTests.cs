using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Authentication.Jwt;
using Sb.Apps.AgentHub.Models.Dto;
using Sb.Apps.AgentHub.Services;
using Sb.Apps.AgentHub.Tests.Integration.Helpers;
using Sb.Framework.Chat.Realtime.Events;
using Sb.Framework.Chat.Realtime.Messages;
using Sb.Framework.Chat.Realtime.Options;
using Xunit;
using Xunit.Abstractions;

namespace Sb.Apps.AgentHub.Tests.Integration.BusinessFlowTests
{
    /// <summary>
    /// 业务流程测试 — 访客认证、Token刷新、客服Token
    /// 依赖：数据库中需要有 Owner（Member）+ Platform 数据
    /// </summary>
    public class T10_ChatAuthTests
    {
        private readonly ITestOutputHelper _output;
        private readonly ChatAuthService _chatAuthService;
        private readonly IFreeSql _fsql;

        // 从数据库中查到的测试数据
        private string? _testPlatformKey;
        private int _testMemberId;

        public T10_ChatAuthTests(ITestOutputHelper output)
        {
            _output = output;
            TestBootstrap.EnsureInitialized();

            var cache = TestBootstrap.GetCacheService();
            _fsql = TestBootstrap.GetFreeSql();

            // 构建业务服务链（按依赖顺序创建，循环依赖用 null! 占位）
            var jwtToken = new JwtToken(TestBootstrap.GetConfiguration());

            // 1. QuotaService — 循环依赖 MemberService，先用 null!
            var quotaService = new QuotaService(
                _fsql,
                new TestOutputLogger<QuotaService>(output),
                null!,
                null!, // MemberService — circular
                null!, // ChatUserService — circular
                null!);

            // 2. PendingDomainService
            var pendingDomainService = new PendingDomainService(
                _fsql,
                new TestOutputLogger<PendingDomainService>(output),
                null!,
                null!);

            // 3. QuotaPackageService → MemberService
            var quotaPackageService = new QuotaPackageService(
                _fsql,
                null!,
                quotaService);

            var memberService = new MemberService(
                _fsql,
                new TestOutputLogger<MemberService>(output),
                null!,
                TestBootstrap.GetConfiguration(),
                quotaPackageService,
                null!);

            // 4. AdminService（简化 — 测试中不直接使用其方法）
            var adminService = new AdminService(
                _fsql,
                new TestOutputLogger<AdminService>(output),
                null!,
                jwtToken,
                null!, // MenuService
                null!, // AdminRoleMenuService
                cache);

            // 5. ChatPlatformService
            var platformService = new ChatPlatformService(
                _fsql,
                new TestOutputLogger<ChatPlatformService>(output),
                null!,
                quotaService,
                pendingDomainService,
                memberService,
                adminService);

            // 6. ChatUserService
            var chatUserService = new ChatUserService(
                _fsql,
                new TestOutputLogger<ChatUserService>(output),
                null!,
                memberService);

            // 7. CustomerServiceConfigService
            var configService = new CustomerServiceConfigService(
                _fsql,
                new TestOutputLogger<CustomerServiceConfigService>(output),
                null!,
                memberService);

            // 8. QueueService
            var queueService = new QueueService(
                _fsql,
                new TestOutputLogger<QueueService>(output));

            // 9. AgentAllocationService
            var allocationService = new AgentAllocationService(
                new TestOutputLogger<AgentAllocationService>(output),
                configService,
                quotaService,
                queueService,
                platformService,
                adminService,
                memberService,
                chatUserService);

            // 10. AccessViolationService
            var violationService = new AccessViolationService(
                _fsql,
                new TestOutputLogger<AccessViolationService>(output),
                null!,
                platformService);

            // Realtime 模块服务
            var realtimeMessages = new MessageService(
                _fsql,
                new MessageDeliveryService(null!),
                new NullRealtimeEventSink(),
                new RealtimeOptions(),
                new TestOutputLogger<MessageService>(output));
            var realtimeConversations = new Sb.Framework.Chat.Realtime.Conversations.ConversationService(
                _fsql,
                new NullRealtimeEventSink(),
                new TestOutputLogger<Sb.Framework.Chat.Realtime.Conversations.ConversationService>(output));

            // 拆分后的会话编排与访问校验服务
            var sessionOrchestrator = new ChatSessionOrchestrator(
                new TestOutputLogger<ChatSessionOrchestrator>(output),
                realtimeConversations,
                realtimeMessages,
                configService,
                memberService,
                chatUserService);

            var accessValidator = new ChatAccessValidator(
                new TestOutputLogger<ChatAccessValidator>(output),
                violationService,
                pendingDomainService,
                chatUserService);

            _chatAuthService = new ChatAuthService(
                new TestOutputLogger<ChatAuthService>(output),
                platformService,
                chatUserService,
                allocationService,
                memberService,
                jwtToken,
                violationService,
                queueService,
                quotaService,
                null!, // IHttpContextAccessor — 测试中不依赖
                sessionOrchestrator,
                accessValidator);

            // 从数据库读取测试数据
            LoadTestData();
        }

        private void LoadTestData()
        {
            // 查找第一个启用的平台
            var platform = _fsql.Select<Sb.Apps.AgentHub.Models.ChatPlatform>()
                .Where(p => p.IsEnabled)
                .First();
            if (platform != null)
            {
                _testPlatformKey = platform.PlatformKey;
                _output.WriteLine($"测试平台: {platform.PlatformName} (Key={platform.PlatformKey})");
            }

            // 查找第一个 Member（用于客服Token测试）
            var member = _fsql.Select<Sb.Apps.AgentHub.Models.Member>()
                .Where(m => m.Status == Sb.Apps.AgentHub.Models.UserStatus.Active)
                .First();
            if (member != null)
            {
                _testMemberId = member.Id;
                _output.WriteLine($"测试客服: {member.Name ?? member.Username} (Id={member.Id})");
            }
        }

        [Fact]
        public async Task VisitorAuth_NewVisitor_CreatesUserAndReturnsToken()
        {
            // Skip if no test platform
            if (string.IsNullOrEmpty(_testPlatformKey))
            {
                _output.WriteLine("⚠ 数据库中没有启用的平台，跳过此测试");
                return;
            }

            // Arrange
            var request = new VisitorAuthRequest
            {
                PlatformKey = _testPlatformKey!,
                ClientId = $"test_client_{Guid.NewGuid():N}"[..32],
                Nickname = "集成测试访客",
                DeviceType = "PC",
                IpAddress = "127.0.0.1"
            };

            // Act
            var result = await _chatAuthService.VisitorAuthAsync(request);

            // Assert
            Assert.Equal(0, result.Code);
            Assert.NotNull(result.Data);

            var response = result.Data as VisitorAuthResponse;
            Assert.NotNull(response);
            Assert.False(string.IsNullOrEmpty(response!.ChatUserId), "ChatUserId 不应为空");
            Assert.False(string.IsNullOrEmpty(response.Token), "Token 不应为空");
            Assert.True(response.TokenExpire > 0, "Token过期时间应 > 0");

            _output.WriteLine($"✓ 新访客认证成功:");
            _output.WriteLine($"  ChatUserId: {response.ChatUserId}");
            _output.WriteLine($"  Token: {response.Token[..Math.Min(30, response.Token.Length)]}...");
            _output.WriteLine($"  Token过期: {response.TokenExpire} 秒");
            _output.WriteLine($"  客服在线: {response.AgentOnline}");
            if (response.AgentOnline)
                _output.WriteLine($"  分配客服: {response.AgentName} (UserId={response.AgentUserId})");
            else
                _output.WriteLine($"  离线消息: {response.OfflineMessage}");
        }

        [Fact]
        public async Task VisitorAuth_ReturningVisitor_FindsExistingUser()
        {
            if (string.IsNullOrEmpty(_testPlatformKey))
            {
                _output.WriteLine("⚠ 数据库中没有启用的平台，跳过此测试");
                return;
            }

            // Arrange — 同一个 ClientId 调用两次
            var clientId = $"test_return_{Guid.NewGuid():N}"[..32];
            var request = new VisitorAuthRequest
            {
                PlatformKey = _testPlatformKey!,
                ClientId = clientId,
                Nickname = "回访测试访客",
                DeviceType = "Mobile",
                IpAddress = "127.0.0.1"
            };

            // Act
            var result1 = await _chatAuthService.VisitorAuthAsync(request);
            var result2 = await _chatAuthService.VisitorAuthAsync(request);

            // Assert
            Assert.Equal(0, result1.Code);
            Assert.Equal(0, result2.Code);

            var resp1 = result1.Data as VisitorAuthResponse;
            var resp2 = result2.Data as VisitorAuthResponse;
            Assert.NotNull(resp1);
            Assert.NotNull(resp2);
            Assert.Equal(resp1!.ChatUserId, resp2!.ChatUserId);

            _output.WriteLine($"✓ 回访访客认证正确: ChatUserId={resp1.ChatUserId}（两次一致）");
        }

        [Fact]
        public async Task VisitorAuth_InvalidPlatformKey_ReturnsError()
        {
            // Arrange
            var request = new VisitorAuthRequest
            {
                PlatformKey = "invalid_key_that_does_not_exist",
                ClientId = "test_invalid",
                Nickname = "无效平台测试"
            };

            // Act
            var result = await _chatAuthService.VisitorAuthAsync(request);

            // Assert
            Assert.Equal(-1, result.Code);
            Assert.Contains("无效的平台秘钥", result.Message);
            _output.WriteLine($"✓ 无效平台秘钥被正确拒绝: {result.Message}");
        }

        [Fact]
        public async Task RefreshToken_ExistingUser_ReturnsNewToken()
        {
            if (string.IsNullOrEmpty(_testPlatformKey))
            {
                _output.WriteLine("⚠ 数据库中没有启用的平台，跳过此测试");
                return;
            }

            // Arrange — 先创建一个访客
            var clientId = $"test_refresh_{Guid.NewGuid():N}"[..32];
            var authResult = await _chatAuthService.VisitorAuthAsync(new VisitorAuthRequest
            {
                PlatformKey = _testPlatformKey!,
                ClientId = clientId,
                Nickname = "Token刷新测试",
                IpAddress = "127.0.0.1"
            });
            Assert.Equal(0, authResult.Code);
            var authResp = authResult.Data as VisitorAuthResponse;
            Assert.NotNull(authResp);

            // Act
            var refreshResult = await _chatAuthService.RefreshTokenAsync(authResp!.ChatUserId);

            // Assert
            Assert.Equal(0, refreshResult.Code);
            Assert.NotNull(refreshResult.Data);

            var tokenResp = refreshResult.Data as TokenResponse;
            Assert.NotNull(tokenResp);
            Assert.False(string.IsNullOrEmpty(tokenResp!.Token), "刷新后的 Token 不应为空");

            _output.WriteLine($"✓ Token 刷新成功:");
            _output.WriteLine($"  Token: {tokenResp.Token[..Math.Min(30, tokenResp.Token.Length)]}...");
            _output.WriteLine($"  过期: {tokenResp.ExpireTimeSeconds} 秒");
        }

        [Fact]
        public async Task GetAgentToken_ValidMember_ReturnsToken()
        {
            if (_testMemberId == 0)
            {
                _output.WriteLine("⚠ 数据库中没有活跃的 Member，跳过此测试");
                return;
            }

            // Act
            var result = await _chatAuthService.GetAgentTokenAsync(_testMemberId);

            // Assert
            Assert.Equal(0, result.Code);
            Assert.NotNull(result.Data);

            // GetAgentTokenAsync 返回匿名对象，通过 dynamic 访问
            var json = System.Text.Json.JsonSerializer.Serialize(result.Data);
            var data = System.Text.Json.JsonDocument.Parse(json).RootElement;

            Assert.True(data.TryGetProperty("userId", out var userIdProp), "返回数据应包含 userId");
            Assert.True(data.TryGetProperty("token", out var tokenProp), "返回数据应包含 token");
            Assert.False(string.IsNullOrEmpty(userIdProp.GetString()), "userId 不应为空");
            Assert.False(string.IsNullOrEmpty(tokenProp.GetString()), "token 不应为空");

            _output.WriteLine($"✓ 客服Token获取成功:");
            _output.WriteLine($"  userId: {userIdProp.GetString()}");
            _output.WriteLine($"  token: {tokenProp.GetString()![..Math.Min(30, tokenProp.GetString()!.Length)]}...");
        }
    }
}
