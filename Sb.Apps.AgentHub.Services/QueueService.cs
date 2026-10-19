using FreeSql;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 访客排队服务
    /// </summary>
    [AutoDependencyInjection]
    public class QueueService
    {
        private readonly IFreeSql _fsql;
        private readonly ILogger<QueueService> _logger;

        public QueueService(IFreeSql fsql, ILogger<QueueService> logger)
        {
            _fsql = fsql;
            _logger = logger;
        }

        /// <summary>
        /// 加入排队
        /// </summary>
        public VisitorQueue? Enqueue(string chatUserId, int platformId, int ownerId)
        {
            // 检查是否已在排队中
            var existing = _fsql.Select<VisitorQueue>()
                .Where(q => q.ChatUserId == chatUserId && q.Status == QueueStatus.Waiting)
                .First();

            if (existing != null)
                return existing;

            // 获取当前最大排队位置
            var maxPosition = _fsql.Select<VisitorQueue>()
                .Where(q => q.OwnerId == ownerId && q.Status == QueueStatus.Waiting)
                .Max(q => q.QueuePosition);

            var queue = new VisitorQueue
            {
                ChatUserId = chatUserId,
                PlatformId = platformId,
                OwnerId = ownerId,
                QueuePosition = maxPosition + 1,
                EnqueueTime = DateTime.Now,
                Status = QueueStatus.Waiting
            };

            _fsql.Insert(queue).ExecuteAffrows();

            // 重新查询获取 ID
            queue = _fsql.Select<VisitorQueue>()
                .Where(q => q.ChatUserId == chatUserId && q.Status == QueueStatus.Waiting)
                .First();

            _logger.LogInformation("访客入队: ChatUserId={ChatUserId}, Position={Position}, OwnerId={OwnerId}",
                chatUserId, queue?.QueuePosition, ownerId);

            return queue;
        }

        /// <summary>
        /// 获取排队位置信息
        /// </summary>
        public QueuePositionInfo? GetPosition(string chatUserId)
        {
            var queue = _fsql.Select<VisitorQueue>()
                .Where(q => q.ChatUserId == chatUserId && q.Status == QueueStatus.Waiting)
                .First();

            if (queue == null)
                return null;

            // 计算前面排队人数
            var aheadCount = _fsql.Select<VisitorQueue>()
                .Where(q => q.OwnerId == queue.OwnerId
                    && q.Status == QueueStatus.Waiting
                    && q.QueuePosition < queue.QueuePosition)
                .Count();

            return new QueuePositionInfo
            {
                QueueId = queue.Id,
                Position = (int)aheadCount + 1,
                TotalWaiting = (int)_fsql.Select<VisitorQueue>()
                    .Where(q => q.OwnerId == queue.OwnerId && q.Status == QueueStatus.Waiting)
                    .Count(),
                EnqueueTime = queue.EnqueueTime,
                EstimatedWaitMinutes = (int)aheadCount * 3 // 预估每位 3 分钟
            };
        }

        /// <summary>
        /// 取出下一个排队的访客
        /// </summary>
        public VisitorQueue Dequeue(int ownerId)
        {
            var next = _fsql.Select<VisitorQueue>()
                .Where(q => q.OwnerId == ownerId && q.Status == QueueStatus.Waiting)
                .OrderBy(q => q.QueuePosition)
                .First();

            return next;
        }

        /// <summary>
        /// 标记为已分配
        /// </summary>
        public void MarkAssigned(int queueId, int agentId)
        {
            _fsql.Update<VisitorQueue>()
                .Where(q => q.Id == queueId)
                .Set(q => q.Status, QueueStatus.Assigned)
                .Set(q => q.AssignedAgentId, agentId)
                .Set(q => q.AssignedTime, DateTime.Now)
                .ExecuteAffrows();
        }

        /// <summary>
        /// 取消排队
        /// </summary>
        public void Cancel(string chatUserId)
        {
            _fsql.Update<VisitorQueue>()
                .Where(q => q.ChatUserId == chatUserId && q.Status == QueueStatus.Waiting)
                .Set(q => q.Status, QueueStatus.Cancelled)
                .Set(q => q.CancelTime, DateTime.Now)
                .ExecuteAffrows();
        }

        /// <summary>
        /// 获取站长当前排队人数
        /// </summary>
        public int GetWaitingCount(int ownerId)
        {
            return (int)_fsql.Select<VisitorQueue>()
                .Where(q => q.OwnerId == ownerId && q.Status == QueueStatus.Waiting)
                .Count();
        }

        /// <summary>
        /// 按 ChatUserId 获取排队中的记录（供 KefuService.AcceptFromQueue 调用）
        /// </summary>
        public VisitorQueue GetWaitingByChatUserId(string chatUserId)
        {
            return _fsql.Select<VisitorQueue>()
                .Where(q => q.ChatUserId == chatUserId && q.Status == QueueStatus.Waiting)
                .First();
        }

        /// <summary>
        /// 清理超时排队（超过 30 分钟未分配的标记为超时）
        /// </summary>
        public int CleanupTimeout(int timeoutMinutes = 30)
        {
            var cutoff = DateTime.Now.AddMinutes(-timeoutMinutes);
            return _fsql.Update<VisitorQueue>()
                .Where(q => q.Status == QueueStatus.Waiting && q.EnqueueTime < cutoff)
                .Set(q => q.Status, QueueStatus.Timeout)
                .Set(q => q.CancelTime, DateTime.Now)
                .ExecuteAffrows();
        }

        /// <summary>
        /// 尝试处理排队队列：当有空闲客服时从队列中取出访客
        /// </summary>
        public List<VisitorQueue> GetWaitingList(int ownerId, int limit = 10)
        {
            return _fsql.Select<VisitorQueue>()
                .Where(q => q.OwnerId == ownerId && q.Status == QueueStatus.Waiting)
                .OrderBy(q => q.QueuePosition)
                .Take(limit)
                .ToList();
        }
    }

    /// <summary>
    /// 排队位置信息
    /// </summary>
    public class QueuePositionInfo
    {
        public int QueueId { get; set; }
        /// <summary>排队位置（第几位）</summary>
        public int Position { get; set; }
        /// <summary>总排队人数</summary>
        public int TotalWaiting { get; set; }
        /// <summary>入队时间</summary>
        public DateTime EnqueueTime { get; set; }
        /// <summary>预计等待分钟数</summary>
        public int EstimatedWaitMinutes { get; set; }
    }
}
