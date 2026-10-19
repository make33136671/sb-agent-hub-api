using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

var jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJJZCI6IjEiLCJVc2VybmFtZSI6ImFkbWluIiwiVXNlclR5cGUiOiJBZG1pbiIsImV4cCI6MTc4NzU1NDUzNCwiaXNzIjoicWluZ2xvbmdBZG1pbkFQSSIsImF1ZCI6InFpbmdsb25nQWRtaW4ifQ.bIbqxS0c_yjJpGKF_spCHrKpc6_ux_m_IfI1tpp1voI";
var hubUrl = "http://localhost:5153/hubs/realtime";

Console.WriteLine("=== AgentHub Realtime 通讯测试 ===");
Console.WriteLine($"Hub URL: {hubUrl}");
Console.WriteLine($"Token (first 20 chars): {jwtToken[..20]}...");
Console.WriteLine();

var connection = new HubConnectionBuilder()
    .WithUrl(hubUrl, options =>
    {
        options.AccessTokenProvider = () => Task.FromResult<string?>(jwtToken);
    })
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Information);
        logging.AddConsole();
    })
    .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
    .Build();

// 监听消息事件
var messageCount = 0;
connection.On<object>("OnMessage", (data) =>
{
    Interlocked.Increment(ref messageCount);
    var json = System.Text.Json.JsonSerializer.Serialize(data);
    Console.WriteLine($"[收到消息 #{messageCount}] {json[..Math.Min(json.Length, 200)]}");
});

// 监听已读回执
connection.On<object>("OnReadReceipt", (data) =>
{
    Console.WriteLine($"[已读回执] {System.Text.Json.JsonSerializer.Serialize(data)}");
});

// 连接状态事件
connection.Reconnecting += (ex) =>
{
    Console.WriteLine($"[重连中] {ex?.Message}");
    return Task.CompletedTask;
};

connection.Reconnected += (id) =>
{
    Console.WriteLine($"[已重连] ConnectionId={id}");
    return Task.CompletedTask;
};

connection.Closed += (ex) =>
{
    Console.WriteLine($"[连接关闭] {ex?.Message}");
    return Task.CompletedTask;
};

try
{
    Console.WriteLine("正在连接 SignalR Hub...");
    await connection.StartAsync();
    Console.WriteLine($"[连接成功] ConnectionId={connection.ConnectionId}");
    Console.WriteLine();

    // 测试 1: Ping
    Console.WriteLine("--- 测试 1: Ping ---");
    try
    {
        await connection.InvokeAsync("Ping");
        Console.WriteLine("[Ping] 成功");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Ping] 失败: {ex.Message}");
    }
    Console.WriteLine();

    // 测试 2: 获取会话列表
    Console.WriteLine("--- 测试 2: GetConversationList ---");
    try
    {
        var conversations = await connection.InvokeAsync<object>("GetConversationList", 10);
        Console.WriteLine($"[GetConversationList] 成功: {System.Text.Json.JsonSerializer.Serialize(conversations)}"[..Math.Min(200, System.Text.Json.JsonSerializer.Serialize(conversations).Length)]);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GetConversationList] 失败: {ex.Message}");
    }
    Console.WriteLine();

    // 测试 3: 发送消息
    Console.WriteLine("--- 测试 3: SendMessage ---");
    try
    {
        var msg = await connection.InvokeAsync<object>("SendMessage", "test-conversation-1", 1, "Hello from test client");
        var json = System.Text.Json.JsonSerializer.Serialize(msg);
        Console.WriteLine($"[SendMessage] 成功: {json[..Math.Min(json.Length, 200)]}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SendMessage] 失败: {ex.Message}");
    }
    Console.WriteLine();

    // 测试 4: 获取历史消息
    Console.WriteLine("--- 测试 4: GetHistory ---");
    try
    {
        var history = await connection.InvokeAsync<object>("GetHistory", "test-conversation-1", 0L, 10);
        var json = System.Text.Json.JsonSerializer.Serialize(history);
        Console.WriteLine($"[GetHistory] 成功: {json[..Math.Min(json.Length, 200)]}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GetHistory] 失败: {ex.Message}");
    }
    Console.WriteLine();

    // 等待推送消息
    Console.WriteLine("等待 30 秒接收推送消息...");
    await Task.Delay(TimeSpan.FromSeconds(30));
}
catch (Microsoft.AspNetCore.SignalR.HubException ex)
{
    Console.WriteLine($"[Hub 错误] {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"[连接失败] {ex.Message}");
}
finally
{
    if (connection.State == HubConnectionState.Connected)
    {
        await connection.StopAsync();
    }
    Console.WriteLine();
    Console.WriteLine($"=== 测试结束 ===");
    Console.WriteLine($"收到消息总数: {messageCount}");
}
