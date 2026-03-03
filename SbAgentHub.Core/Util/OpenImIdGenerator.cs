namespace SbAgentHub.Core.Util
{
    /// <summary>
    /// OpenIM 用户ID 生成器
    /// </summary>
    public static class OpenImIdGenerator
    {
        /// <summary>
        /// 生成客服 OpenIM userID
        /// </summary>
        public static string GenerateAgentId(int memberId)
        {
            return $"agent_{Guid.NewGuid():N}";
        }

        /// <summary>
        /// 生成聊天用户 OpenIM userID
        /// </summary>
        public static string GenerateChatUserId()
        {
            return $"chat_{Guid.NewGuid():N}";
        }

        /// <summary>
        /// 生成管理员 OpenIM userID
        /// </summary>
        public static string GenerateAdminId(int adminId)
        {
            return $"admin_{adminId}";
        }
    }
}
