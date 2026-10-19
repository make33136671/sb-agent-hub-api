namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 通用用户状态
    /// </summary>
    public enum UserStatus
    {
        /// <summary>正常</summary>
        Active = 0,
        /// <summary>禁用</summary>
        Banned = 1,
    }

    /// <summary>
    /// 聊天用户类型
    /// </summary>
    public enum ChatUserType
    {
        /// <summary>匿名用户</summary>
        Anonymous = 0,
        /// <summary>已认证用户</summary>
        Authenticated = 1,
        /// <summary>VIP 用户</summary>
        VIP = 2
    }

    /// <summary>
    /// 聊天用户状态
    /// </summary>
    public enum ChatUserStatus
    {
        /// <summary>正常</summary>
        Normal = 0,
        /// <summary>禁用</summary>
        Disabled = 1,
        /// <summary>黑名单</summary>
        Blacklisted = 2
    }

    /// <summary>
    /// 平台类型
    /// </summary>
    public enum PlatformType
    {
        /// <summary>Web 端</summary>
        Web = 0,
        /// <summary>App 端</summary>
        App = 1,
        /// <summary>小程序</summary>
        MiniProgram = 2,
        /// <summary>H5 页面</summary>
        H5 = 3,
        /// <summary>公众号</summary>
        OfficialAccount = 4,
        /// <summary>其他</summary>
        Other = 99
    }

    /// <summary>
    /// 客服分配规则
    /// </summary>
    public enum AllocationRule
    {
        /// <summary>轮询分配</summary>
        RoundRobin = 0,
        /// <summary>最少接待</summary>
        LeastBusy = 1,
        /// <summary>随机分配</summary>
        Random = 2,
        /// <summary>固定分配</summary>
        Fixed = 3
    }

    /// <summary>
    /// 离线处理方式
    /// </summary>
    public enum OfflineHandling
    {
        /// <summary>留言</summary>
        LeaveMessage = 0,
        /// <summary>自动回复</summary>
        AutoReply = 1,
        /// <summary>转接其他</summary>
        TransferOther = 2
    }

    /// <summary>
    /// 接入方式
    /// </summary>
    public enum AccessType
    {
        /// <summary>纯 JS 嵌入（IIFE script 标签）</summary>
        JS = 0,
        /// <summary>Vue 组件（npm 安装 + import）</summary>
        Vue = 1,
        /// <summary>独立聊天页面（URL 直接打开）</summary>
        Chat = 2
    }
}
