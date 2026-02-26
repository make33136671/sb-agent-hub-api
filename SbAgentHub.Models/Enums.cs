namespace SbAgentHub.Models
{
    /// <summary>
    /// 通用用户状态
    /// </summary>
    public enum UserStatus
    {
        Active = 0,
        Banned = 1,
    }

    /// <summary>
    /// 聊天用户类型
    /// </summary>
    public enum ChatUserType
    {
        Anonymous = 0,
        Authenticated = 1,
        VIP = 2
    }

    /// <summary>
    /// 聊天用户状态
    /// </summary>
    public enum ChatUserStatus
    {
        Normal = 0,
        Disabled = 1,
        Blacklisted = 2
    }

    /// <summary>
    /// 平台类型
    /// </summary>
    public enum PlatformType
    {
        Web = 0,
        App = 1,
        MiniProgram = 2,
        H5 = 3,
        OfficialAccount = 4,
        Other = 99
    }

    /// <summary>
    /// 客服分配规则
    /// </summary>
    public enum AllocationRule
    {
        RoundRobin = 0,
        LeastBusy = 1,
        Random = 2,
        Fixed = 3
    }

    /// <summary>
    /// 离线处理方式
    /// </summary>
    public enum OfflineHandling
    {
        LeaveMessage = 0,
        AutoReply = 1,
        TransferOther = 2
    }
}
