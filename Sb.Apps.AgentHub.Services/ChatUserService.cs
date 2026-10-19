using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Core.Util;
using Sb.Apps.AgentHub.Models;
using System.Linq.Expressions;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 聊天用户管理服务（亦管理 ChatUserLoginRecord 登录记录）
    /// </summary>
    [AutoDependencyInjection]
    public class ChatUserService : BaseCrudService<ChatUser>
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MemberService _memberService;

        public ChatUserService(IFreeSql fsql, ILogger<ChatUserService> logger,
            IHttpContextAccessor httpContextAccessor,
            MemberService memberService) : base(fsql, logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _memberService = memberService;
        }

        #region 仓储访问入口（供其他 Service 调用）

        /// <summary>
        /// 按主键获取聊天用户
        /// </summary>
        public ChatUser GetById(int id)
        {
            return Select.Where(c => c.Id == id).First();
        }

        /// <summary>
        /// 按 ExternalUserId + 平台获取聊天用户
        /// </summary>
        public ChatUser GetByExternalUserId(string externalUserId, int platformId)
        {
            return Select.Where(c => c.ExternalUserId == externalUserId && c.PlatformId == platformId).First();
        }

        /// <summary>
        /// 创建聊天用户并返回（含自增 Id）
        /// </summary>
        public ChatUser CreateChatUser(ChatUser chatUser)
        {
            chatUser = Insert(chatUser);
            return chatUser;
        }

        /// <summary>
        /// 统计站长下在线访客数（配额 max_concurrent_sessions / 并发检查用）
        /// </summary>
        public int CountOnlineByOwner(int ownerId)
        {
            return (int)Select.Where(c => c.OwnerId == ownerId && c.OnlineStatus == OnlineStatus.Online && !c.IsDeleted).Count();
        }

        /// <summary>
        /// 统计客服已分配的访客数
        /// </summary>
        public int CountAssignedByAgent(int agentId)
        {
            return (int)Select.Where(c => c.AssignedAgentId == agentId && !c.IsDeleted).Count();
        }

        /// <summary>
        /// 按 ChatUserId 列表批量获取聊天用户
        /// </summary>
        public List<ChatUser> GetByChatUserIds(List<string> chatUserIds)
        {
            if (chatUserIds == null || !chatUserIds.Any()) return new List<ChatUser>();
            return Select.Where(c => chatUserIds.Contains(c.ChatUserId)).ToList();
        }

        /// <summary>
        /// 广播模式：获取站长下有 GroupId 的活跃访客（含 GroupId、AssignedAgentId）
        /// </summary>
        public List<ChatUser> GetActiveChatsWithGroupId(int ownerId)
        {
            return Select.Where(c => c.OwnerId == ownerId
                    && !string.IsNullOrEmpty(c.GroupId)
                    && !c.IsDeleted)
                .ToList(c => new { c.ChatUserId, c.GroupId, c.AssignedAgentId })
                .Select(x => new ChatUser { ChatUserId = x.ChatUserId, GroupId = x.GroupId, AssignedAgentId = x.AssignedAgentId })
                .ToList();
        }

        /// <summary>
        /// 通用列更新（供其他 Service 按需更新 ChatUser 字段）
        /// </summary>
        public void UpdateColumns(ChatUser source, Expression<Func<ChatUser, object>> columns)
        {
            UpdateDiy.UpdateColumns(columns).SetSource(source).ExecuteAffrows();
        }

        /// <summary>
        /// 更新分配客服（供 KefuService.TransferVisitor/AcceptFromQueue 调用）
        /// </summary>
        public void UpdateAssignedAgent(int chatUserId, int agentId)
        {
            UpdateDiy.Where(c => c.Id == chatUserId)
                .Set(c => c.AssignedAgentId, agentId)
                .ExecuteAffrows();
        }

        /// <summary>
        /// 获取客服访客列表（广播/普通模式）— 返回原始数据，由调用方投影
        /// </summary>
        public (List<ChatUser> data, long total) GetAgentVisitors(int agentId, int ownerId, bool broadcastMode,
            string keyword, int page, int pageSize)
        {
            ISelect<ChatUser> query;
            if (broadcastMode)
            {
                query = Select.Where(c => c.OwnerId == ownerId && !c.IsDeleted);
            }
            else
            {
                query = Select.Where(c => c.AssignedAgentId == agentId && !c.IsDeleted);
            }

            query = query.WhereIf(!string.IsNullOrEmpty(keyword),
                c => c.Nickname.Contains(keyword) || c.NicknameRemark.Contains(keyword) || c.ChatUserId.Contains(keyword));

            var data = query.OrderByDescending(c => c.LastActiveTime)
                .Count(out long dataCount)
                .Page(page, pageSize)
                .ToList();

            return (data, dataCount);
        }

        /// <summary>
        /// 统计平台在指定时间范围内的去重登录访客数（配额 max_daily_visitors 用）
        /// </summary>
        public int CountDailyLoginsByPlatformIds(List<int> platformIds, DateTime start, DateTime end)
        {
            if (platformIds == null || !platformIds.Any()) return 0;
            return (int)_fsql.Select<ChatUserLoginRecord>()
                .Where(r => platformIds.Contains(r.PlatformId)
                    && r.LoginTime >= start && r.LoginTime < end)
                .GroupBy(r => r.ChatUserDbId)
                .Count();
        }

        /// <summary>
        /// 查询登录记录（数据导出用）
        /// </summary>
        public List<ChatUserLoginRecord> GetLoginRecords(List<int> platformIds, DateTime? start, DateTime? end)
        {
            var query = _fsql.Select<ChatUserLoginRecord>().Where(r => true);
            if (platformIds != null && platformIds.Any())
                query = query.Where(r => platformIds.Contains(r.PlatformId));
            if (start.HasValue)
                query = query.Where(r => r.LoginTime >= start.Value);
            if (end.HasValue)
                query = query.Where(r => r.LoginTime < end.Value.AddDays(1));
            return query.OrderByDescending(r => r.LoginTime).ToList();
        }

        /// <summary>
        /// 记录访客登录（供 ChatAccessValidator 调用）
        /// </summary>
        public void RecordLogin(int chatUserDbId, int platformId, string clientId,
            string deviceType, string userAgent, string ipAddress)
        {
            _fsql.Insert(new ChatUserLoginRecord
            {
                ChatUserDbId = chatUserDbId,
                PlatformId = platformId,
                ClientId = clientId,
                DeviceType = deviceType,
                UserAgent = userAgent,
                IpAddress = ipAddress,
                LoginTime = DateTime.Now,
                IsDeleted = false
            }).ExecuteAffrows();
        }

        #endregion

        /// <summary>
        /// 聊天用户列表
        /// </summary>
        public ResultData GetList(string keyword = "", int? ownerId = null, int? platformId = null,
            ChatUserStatus? status = null, ChatUserType? userType = null,
            string field = "", string order = "",
            int page = 1, int pageSize = 20)
        {
            var user = _httpContextAccessor.HttpContext!.User;
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
            query = query.WhereIf(ownerId.HasValue, c => c.OwnerId == ownerId!.Value);
            query = query.WhereIf(platformId.HasValue, c => c.PlatformId == platformId!.Value);
            query = query.WhereIf(status.HasValue, c => c.Status == status!.Value);
            query = query.WhereIf(userType.HasValue, c => c.UserType == userType!.Value);

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

            var agent = _memberService.GetActiveAgent(agentId);
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
            var user = _httpContextAccessor.HttpContext!.User;
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

            query = query.WhereIf(ownerId.HasValue, c => c.OwnerId == ownerId!.Value);

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
