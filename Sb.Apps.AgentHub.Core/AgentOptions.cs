namespace Sb.Apps.AgentHub.Core;

/// <summary>
/// 客服系统特有配置（不属于 SB.Framework 通用底座，承载 agent-api 专属配置）。
/// 由 Program.cs 从 appsettings 绑定初始化。
/// </summary>
public static class AgentConfig
{
    public static AgentOptions Options { get; set; } = new();
}

/// <summary>
/// 客服系统专属配置项。
/// </summary>
public class AgentOptions
{
}
