using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Models.Dto;
using SbAgentHub.Services;
using SbAgentHub.Tests.Integration.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace SbAgentHub.Tests.Integration.BusinessFlowTests
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

            // 构建 OpenIM 服务链
            var authService = new OpenImAuthService(
                new TestOutputLogger<OpenImAuthService>(output), cache);
            var userService = new OpenImUserService(
                new TestOutputLogger<OpenImUserService>(output), authService);
            var messageService = new OpenImMessageService(
                new TestOutputLogger<OpenImMessageService>(output), authService);

            // 构建业务服务链
            var quotaService = new QuotaService(
                _fsql,
                new TestOutputLogger<QuotaService>(output),
                null!);

            var pendingDomainService = new PendingDomainService(
                _fsql,
                new TestOutputLogger<PendingDomainService>(output),
                null!);

            var platformService = new ChatPlatformService(
                _fsql,
                new TestOutputLogger<ChatPlatformService>(output),
                null!, // IHttpContextAccessor — 测试中不依赖
                quotaService,
                pendingDomainService);

            var chatUserService = new ChatUserService(
                _fsql,
                new TestOutputLogger<ChatUserService>(output),
                null!);

            var configService = new CustomerServiceConfigService(
                _fsql,
                new TestOutputLogger<CustomerServiceConfigService>(output),
                null!);

            var queueService = new QueueService(
                _fsql,
                new TestOutputLogger<QueueService>(output));

            var allocationService = new AgentAllocationService(
                _fsql,
                new TestOutputLogger<AgentAllocationService>(output),
                configService,
                quotaService,
                queueService);

            var violationService = new AccessViolationService(
                _fsql,
                new TestOutputLogger<AccessViolationService>(output),
                null!);

            var visitorFormService = new VisitorFormService(_fsql, null!, quotaService);

            var groupService = new OpenImGroupService(
                new TestOutputLogger<OpenImGroupService>(output),
                authService);

            _chatAuthService = new ChatAuthService(
                _fsql,
                new TestOutputLogger<ChatAuthService>(output),
                platformService,
                chatUserService,
                allocationService,
                userService,
                authService,
                messageService,
                groupService,
                violationService,
                pendingDomainService,
                queueService,
                quotaService,
                visitorFormService,
                configService,
                null!); // IHttpContextAccessor — 测试中不依赖

            // 从数据库读取测试数据
            LoadTestData();
        }

        private void LoadTestData()
        {
            // 查找第一个启用的平台
            var platform = _fsql.Select<SbAgentHub.Models.ChatPlatform>()
                .Where(p => p.IsEnabled)
                .First();
            if (platform != null)
            {
                _testPlatformKey = platform.PlatformKey;
                _output.WriteLine($"测试平台: {platform.PlatformName} (Key={platform.PlatformKey})");
            }

            // 查找第一个 Member（用于客服Token测试）
            var member = _fsql.Select<SbAgentHub.Models.Member>()
                .Where(m => m.Status == SbAgentHub.Models.UserStatus.Active)
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
            Assert.False(string.IsNullOrEmpty(response.ImToken), "ImToken 不应为空");
            Assert.True(response.ImTokenExpire > 0, "Token过期时间应 > 0");

            _output.WriteLine($"✓ 新访客认证成功:");
            _output.WriteLine($"  ChatUserId: {response.ChatUserId}");
            _output.WriteLine($"  ImToken: {response.ImToken[..Math.Min(30, response.ImToken.Length)]}...");
            _output.WriteLine($"  Token过期: {response.ImTokenExpire} 秒");
            _output.WriteLine($"  客服在线: {response.AgentOnline}");
            if (response.AgentOnline)
                _output.WriteLine($"  分配客服: {response.AgentName} (ImUserId={response.AgentImUserId})");
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
        public async Task RefreshImToken_ExistingUser_ReturnsNewToken()
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
            var refreshResult = await _chatAuthService.RefreshImTokenAsync(authResp!.ChatUserId);

            // Assert
            Assert.Equal(0, refreshResult.Code);
            Assert.NotNull(refreshResult.Data);

            var tokenResp = refreshResult.Data as ImTokenResponse;
            Assert.NotNull(tokenResp);
            Assert.False(string.IsNullOrEmpty(tokenResp!.Token), "刷新后的 Token 不应为空");

            _output.WriteLine($"✓ Token 刷新成功:");
            _output.WriteLine($"  Token: {tokenResp.Token[..Math.Min(30, tokenResp.Token.Length)]}...");
            _output.WriteLine($"  过期: {tokenResp.ExpireTimeSeconds} 秒");
        }

        [Fact]
        public async Task GetAgentImToken_ValidMember_ReturnsToken()
        {
            if (_testMemberId == 0)
            {
                _output.WriteLine("⚠ 数据库中没有活跃的 Member，跳过此测试");
                return;
            }

            // Act
            var result = await _chatAuthService.GetAgentImTokenAsync(_testMemberId);

            // Assert
            Assert.Equal(0, result.Code);
            Assert.NotNull(result.Data);

            // GetAgentImTokenAsync 返回匿名对象，通过 dynamic 访问
            var json = System.Text.Json.JsonSerializer.Serialize(result.Data);
            var data = System.Text.Json.JsonDocument.Parse(json).RootElement;

            Assert.True(data.TryGetProperty("imUserId", out var imUserIdProp), "返回数据应包含 imUserId");
            Assert.True(data.TryGetProperty("token", out var tokenProp), "返回数据应包含 token");
            Assert.False(string.IsNullOrEmpty(imUserIdProp.GetString()), "imUserId 不应为空");
            Assert.False(string.IsNullOrEmpty(tokenProp.GetString()), "token 不应为空");

            _output.WriteLine($"✓ 客服Token获取成功:");
            _output.WriteLine($"  imUserId: {imUserIdProp.GetString()}");
            _output.WriteLine($"  token: {tokenProp.GetString()![..Math.Min(30, tokenProp.GetString()!.Length)]}...");
        }
    }
}
