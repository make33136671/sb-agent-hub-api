using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Core.Util;
using SbAgentHub.Models;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 聊天用户管理服务
    /// </summary>
    [AutoDependencyInjection]
    public class ChatUserService : BaseCrudService<ChatUser>
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatUserService(IFreeSql fsql, ILogger<ChatUserService> logger,
            IHttpContextAccessor httpContextAccessor) : base(fsql, logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 聊天用户列表
        /// </summary>
        public ResultData GetList(string keyword = "", int? ownerId = null, int? platformId = null,
            ChatUserStatus? status = null, ChatUserType? userType = null,
            string field = "", string order = "",
            int page = 1, int pageSize = 20)
        {
            var user = _httpContextAccessor.HttpContext.User;
            string currentUserType = user.UserType();
            string role = user.Role();

            var query = Select;

            // Owner 只能看自己平台下的用户
            if (currentUserType == "Member" && role == "Owner")
            {
                int currentUserId = user.Id();
                query = query.Where(c => c.OwnerId == currentUserId);
            }
            // Agent 只能看分配给自己的用户
            else if (currentUserType == "Member" && role == "Agent")
            {
                int currentUserId = user.Id();
                query = query.Where(c => c.AssignedAgentId == currentUserId);
            }

            query = query.WhereIf(!string.IsNullOrEmpty(keyword),
                c => c.Nickname.Contains(keyword) || c.ChatUserId.Contains(keyword) || c.ExternalUserId.Contains(keyword));
            query = query.WhereIf(ownerId.HasValue, c => c.OwnerId == ownerId.Value);
            query = query.WhereIf(platformId.HasValue, c => c.PlatformId == platformId.Value);
            query = query.WhereIf(status.HasValue, c => c.Status == status.Value);
            query = query.WhereIf(userType.HasValue, c => c.UserType == userType.Value);

            query = ApplySorting(query, field, order);

            var data = query.Count(out long dataCount).Page(page, pageSize).ToList(c => new
            {
                c.Id, c.ChatUserId, c.UserType, c.PlatformId, c.OwnerId,
                c.ExternalUserId, c.Username, c.Nickname, c.Avatar,
                c.AssignedAgentId, c.Status, c.OnlineStatus,
                c.NicknameRemark, c.Remark,
                c.FirstVisitTime, c.LastActiveTime, c.CreateTime
            });

            return ResultPage.Ok(data, dataCount, page, pageSize, "数据查询成功");
        }

        /// <summary>
        /// 更新聊天用户状态
        /// </summary>
        public ResultData UpdateStatus(int id, ChatUserStatus status)
        {
            var chatUser = Select.Where(c => c.Id == id).First();
            if (chatUser == null) return ResultData.No("用户不存在!");

            chatUser.Status = status;
            UpdateDiy.UpdateColumns(c => c.Status).SetSource(chatUser).ExecuteAffrows();

            return ResultData.Ok("状态更新成功!", new { chatUser.Status });
        }

        /// <summary>
        /// 加入黑名单
        /// </summary>
        public ResultData AddToBlacklist(int id)
        {
            return UpdateStatus(id, ChatUserStatus.Blacklisted);
        }

        /// <summary>
        /// 移出黑名单
        /// </summary>
        public ResultData RemoveFromBlacklist(int id)
        {
            return UpdateStatus(id, ChatUserStatus.Normal);
        }

        /// <summary>
        /// 手动分配客服
        /// </summary>
        public ResultData AssignAgent(int chatUserId, int agentId)
        {
            var chatUser = Select.Where(c => c.Id == chatUserId).First();
            if (chatUser == null) return ResultData.No("用户不存在!");

            var agent = _fsql.Select<Member>().Where(c => c.Id == agentId && c.Role == MemberRole.Agent).First();
            if (agent == null) return ResultData.No("客服不存在!");

            chatUser.AssignedAgentId = agentId;
            UpdateDiy.UpdateColumns(c => c.AssignedAgentId).SetSource(chatUser).ExecuteAffrows();

            return ResultData.Ok("客服分配成功!");
        }

        /// <summary>
        /// 在线用户列表
        /// </summary>
        public ResultData OnlineUsers(int? ownerId = null)
        {
            var user = _httpContextAccessor.HttpContext.User;
            string userType = user.UserType();
            string role = user.Role();

            var query = Select.Where(c => c.OnlineStatus == OnlineStatus.Online);

            if (userType == "Member" && role == "Owner")
            {
                query = query.Where(c => c.OwnerId == user.Id());
            }
            else if (userType == "Member" && role == "Agent")
            {
                query = query.Where(c => c.AssignedAgentId == user.Id());
            }

            query = query.WhereIf(ownerId.HasValue, c => c.OwnerId == ownerId.Value);

            var data = query.OrderByDescending(c => c.LastActiveTime).ToList(c => new
            {
                c.Id, c.ChatUserId, c.Nickname, c.Avatar, c.AssignedAgentId,
                c.OnlineStatus, c.LastActiveTime
            });

            return ResultData.Ok(data);
        }

        /// <summary>
        /// 更新用户备注
        /// </summary>
        public ResultData UpdateRemark(int id, string nicknameRemark, string remark)
        {
            var chatUser = Select.Where(c => c.Id == id).First();
            if (chatUser == null) return ResultData.No("用户不存在!");

            chatUser.NicknameRemark = nicknameRemark;
            chatUser.Remark = remark;
            UpdateDiy.UpdateColumns(c => new { c.NicknameRemark, c.Remark }).SetSource(chatUser).ExecuteAffrows();

            return ResultData.Ok("备注更新成功!");
        }

        /// <summary>
        /// 编辑聊天用户
        /// </summary>
        public ResultData Edit(ChatUser chatUser)
        {
            var existing = Select.Where(c => c.Id == chatUser.Id).First();
            if (existing == null) return ResultData.No("用户不存在!");

            existing.Nickname = chatUser.Nickname;
            existing.NicknameRemark = chatUser.NicknameRemark;
            existing.Remark = chatUser.Remark;

            UpdateDiy.UpdateColumns(c => new { c.Nickname, c.NicknameRemark, c.Remark }).SetSource(existing).ExecuteAffrows();

            return ResultData.Ok("更新成功!");
        }

        /// <summary>
        /// 通过 ChatUserId 获取用户
        /// </summary>
        public ChatUser GetByChatUserId(string chatUserId)
        {
            return Select.Where(c => c.ChatUserId == chatUserId).First();
        }

        /// <summary>
        /// 通过 ClientId 获取用户
        /// </summary>
        public ChatUser GetByClientId(string clientId, int platformId)
        {
            return Select.Where(c => c.ClientId == clientId && c.PlatformId == platformId).First();
        }

        /// <summary>
        /// 更新在线状态
        /// </summary>
        public void UpdateOnlineStatus(int id, OnlineStatus onlineStatus)
        {
            _fsql.Update<ChatUser>()
                .Where(c => c.Id == id)
                .Set(c => c.OnlineStatus, onlineStatus)
                .Set(c => c.LastActiveTime, DateTime.Now)
                .ExecuteAffrows();
        }
    }
}
