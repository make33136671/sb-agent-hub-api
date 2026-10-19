namespace Sb.Apps.AgentHub.Models.Dto
{
    /// <summary>
    /// 系统消息类型常量 — 与 realtime-core-js/src/message/SystemMessage.ts 对齐
    ///
    /// 所有系统消息使用 Realtime 自定义消息类型，
    /// data.type 字段标识具体类型，extension 包含 {systemMessage:true, notDisplay:true}。
    /// </summary>
    public static class SystemMessageTypes
    {
        /// <summary>
        /// 会话创建 — 服务端在分配客服后发送，用于创建 Realtime 会话
        /// 发送方: API 服务端（通过 Admin Token）
        /// 时机: 访客获取 Token、分配客服成功后
        /// </summary>
        public const string SessionInit = "session_init";

        /// <summary>
        /// 访客上线 — 客户端连接 IM 成功后发送，表示访客已就绪可通信
        /// 发送方: Chat 客户端（通过 SDK）
        /// 时机: 客户端 login 成功、sync 完成后
        /// </summary>
        public const string VisitorOnline = "visitor_online";

        /// <summary>
        /// 访客离线 — 客户端断开时发送
        /// 发送方: Chat 客户端（通过 SDK）
        /// 时机: 页面关闭、idle 超时、手动断开
        /// </summary>
        public const string VisitorOffline = "visitor_offline";

        /// <summary>
        /// 用户信息更新 — 客户端更新昵称/头像后发送
        /// 发送方: Chat 客户端（通过 SDK）
        /// </summary>
        public const string UserInfoUpdated = "user_info_updated";

        /// <summary>
        /// 消息已读 — 访客阅读了客服消息
        /// 发送方: API 服务端（Chat 客户端调用 MessageRead 接口触发）
        /// 时机: 访客收到客服消息后
        /// </summary>
        public const string MessageRead = "message_read";

        /// <summary>
        /// 所有系统消息类型列表（用于批量判断）
        /// </summary>
        public static readonly string[] All = new[]
        {
            SessionInit,
            VisitorOnline,
            VisitorOffline,
            UserInfoUpdated,
            MessageRead
        };

        /// <summary>
        /// 系统消息的 contentType
        /// </summary>
        public const int ContentType = 110;
    }
}
