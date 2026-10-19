using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

var baseUrl = "http://localhost:5153";
var jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJJZCI6IjEiLCJVc2VybmFtZSI6ImFkbWluIiwiVXNlclR5cGUiOiJBZG1pbiIsImV4cCI6MTc4NzU1NDUzNCwiaXNzIjoicWluZ2xvbmdBZG1pbkFQSSIsImF1ZCI6InFpbmdsb25nQWRtaW4ifQ.bIbqxS0c_yjJpGKF_spCHrKpc6_ux_m_IfI1tpp1voI";
var hubUrl = $"{baseUrl}/hubs/realtime";

Console.WriteLine("=== AgentHub Realtime 连通性测试 ===");
Console.WriteLine($"Base URL: {baseUrl}");
Console.WriteLine($"Hub URL: {hubUrl}");
Console.WriteLine($"Token (first 20 chars): {jwtToken[..20]}...");
Console.WriteLine();

var passCount = 0;
var failCount = 0;
var results = new List<(string Test, bool Pass, string Detail)>();

void Record(string test, bool pass, string detail)
{
    var status = pass ? "PASS" : "FAIL";
    Console.WriteLine($"  [{status}] {test}: {detail}");
    if (pass) Interlocked.Increment(ref passCount);
    else Interlocked.Increment(ref failCount);
    results.Add((test, pass, detail));
}

// ── 测试 1: HTTP 健康检查 ──
Console.WriteLine("--- 测试 1: HTTP 健康检查 ---");
try
{
    using var http = new HttpClient();
    var resp = await http.GetAsync($"{baseUrl}/health");
    Record("HTTP /health", resp.IsSuccessStatusCode, $"Status={resp.StatusCode}");
}
catch (Exception ex)
{
    Record("HTTP /health", false, ex.Message);
}
Console.WriteLine();

// ── 测试 2: SignalR negotiate 端点 ──
Console.WriteLine("--- 测试 2: SignalR negotiate ---");
try
{
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
    var resp = await http.PostAsync($"{hubUrl}/negotiate", null);
    var body = await resp.Content.ReadAsStringAsync();
    var pass = resp.IsSuccessStatusCode && body.Contains("connectionId");
    Record("SignalR negotiate", pass, $"Status={resp.StatusCode}, Body={body[..Math.Min(body.Length, 100)]}");
}
catch (Exception ex)
{
    Record("SignalR negotiate", false, ex.Message);
}
Console.WriteLine();

// ── 测试 3: SignalR WebSocket 连接 ──
Console.WriteLine("--- 测试 3: SignalR WebSocket 连接 ---");
var connection = new HubConnectionBuilder()
    .WithUrl(hubUrl, options =>
    {
        options.AccessTokenProvider = () => Task.FromResult<string?>(jwtToken);
    })
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Warning);
        logging.AddConsole();
    })
    .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
    .Build();

var messageCount = 0;
connection.On<object>("OnMessage", (data) =>
{
    Interlocked.Increment(ref messageCount);
    var json = JsonSerializer.Serialize(data);
    Console.WriteLine($"  [收到消息 #{messageCount}] {json[..Math.Min(json.Length, 200)]}");
});

connection.On<object>("OnReadReceipt", (data) =>
{
    Console.WriteLine($"  [已读回执] {JsonSerializer.Serialize(data)}");
});

connection.Reconnecting += (ex) =>
{
    Console.WriteLine($"  [重连中] {ex?.Message}");
    return Task.CompletedTask;
};

connection.Reconnected += (id) =>
{
    Console.WriteLine($"  [已重连] ConnectionId={id}");
    return Task.CompletedTask;
};

connection.Closed += (ex) =>
{
    Console.WriteLine($"  [连接关闭] {ex?.Message}");
    return Task.CompletedTask;
};

try
{
    Console.WriteLine("  正在连接 SignalR Hub...");
    await connection.StartAsync();
    Record("WebSocket 连接", connection.State == HubConnectionState.Connected,
        $"State={connection.State}, ConnectionId={connection.ConnectionId}");
}
catch (Exception ex)
{
    Record("WebSocket 连接", false, ex.Message);
}
Console.WriteLine();

if (connection.State == HubConnectionState.Connected)
{
    // ── 测试 4: Ping ──
    Console.WriteLine("--- 测试 4: Ping ---");
    try
    {
        await connection.InvokeAsync("Ping");
        Record("Ping", true, "成功");
    }
    catch (Exception ex)
    {
        Record("Ping", false, ex.Message);
    }
    Console.WriteLine();

    // ── 测试 5: GetConversationList ──
    Console.WriteLine("--- 测试 5: GetConversationList ---");
    try
    {
        var conversations = await connection.InvokeAsync<object>("GetConversationList", 10);
        var json = JsonSerializer.Serialize(conversations);
        Record("GetConversationList", true, json[..Math.Min(json.Length, 200)]);
    }
    catch (Exception ex)
    {
        Record("GetConversationList", false, ex.Message);
    }
    Console.WriteLine();

    // ── 测试 6: SendMessage ──
    Console.WriteLine("--- 测试 6: SendMessage ---");
    try
    {
        var msg = await connection.InvokeAsync<object>("SendMessage", "test-conversation-1", 1, "Hello from AgentHub test client");
        var json = JsonSerializer.Serialize(msg);
        Record("SendMessage", true, json[..Math.Min(json.Length, 200)]);
    }
    catch (Exception ex)
    {
        Record("SendMessage", false, ex.Message);
    }
    Console.WriteLine();

    // ── 测试 7: GetHistory ──
    Console.WriteLine("--- 测试 7: GetHistory ---");
    try
    {
        var history = await connection.InvokeAsync<object>("GetHistory", "test-conversation-1", 0L, 10);
        var json = JsonSerializer.Serialize(history);
        Record("GetHistory", true, json[..Math.Min(json.Length, 200)]);
    }
    catch (Exception ex)
    {
        Record("GetHistory", false, ex.Message);
    }
    Console.WriteLine();

    // ── 测试 8: 等待推送消息 ──
    Console.WriteLine("--- 测试 8: 实时消息推送（等待 15 秒）---");
    var beforeWait = messageCount;
    await Task.Delay(TimeSpan.FromSeconds(15));
    Record("实时消息推送", messageCount > beforeWait,
        $"等待前={beforeWait}, 等待后={messageCount}");
    Console.WriteLine();
}

// ── 清理 ──
if (connection.State == HubConnectionState.Connected)
{
    await connection.StopAsync();
}

Console.WriteLine("=== 测试结果汇总 ===");
Console.WriteLine($"通过: {passCount}  失败: {failCount}");
Console.WriteLine();
foreach (var (test, pass, detail) in results)
{
    var icon = pass ? "[PASS]" : "[FAIL]";
    Console.WriteLine($"  {icon} {test}");
}
Console.WriteLine();
Console.WriteLine($"=== 测试结束 ===");
