using FreeSql;
using Microsoft.AspNetCore.Http;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Models;
using System.Text.Json;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 订单服务
    /// </summary>
    [AutoDependencyInjection]
    public class OrderService
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly QuotaService _quotaService;
        private readonly QuotaPackageService _packageService;

        public OrderService(IFreeSql fsql, IHttpContextAccessor httpContextAccessor,
            QuotaService quotaService, QuotaPackageService packageService)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _quotaService = quotaService;
            _packageService = packageService;
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        public ResultData CreateOrder(int memberId, int packageId, int orderType, string paymentMethod, string remark = "")
        {
            var member = _fsql.Select<Member>()
                .Where(m => m.Id == memberId && m.Role == MemberRole.Owner)
                .First();
            if (member == null)
                return ResultData.No("站长不存在!");

            var package = _fsql.Select<QuotaPackage>()
                .Where(p => p.Id == packageId && p.IsEnabled)
                .First();
            if (package == null)
                return ResultData.No("套餐不存在或未启用!");

            // 计算生效时间
            DateTime effectiveStart = DateTime.Now;
            DateTime effectiveEnd;

            if (orderType == Models.OrderType.Renewal)
            {
                // 续费：从当前有效期结束后开始
                var lastOrder = _fsql.Select<Order>()
                    .Where(o => o.MemberId == memberId && o.Status == OrderStatus.Paid)
                    .OrderByDescending(o => o.EffectiveEndTime)
                    .First();
                if (lastOrder?.EffectiveEndTime != null && lastOrder.EffectiveEndTime > DateTime.Now)
                    effectiveStart = lastOrder.EffectiveEndTime.Value;
            }

            effectiveEnd = package.PricePeriod == "year"
                ? effectiveStart.AddYears(1)
                : effectiveStart.AddMonths(1);

            var order = new Order
            {
                OrderNo = GenerateOrderNo(),
                MemberId = memberId,
                PackageId = packageId,
                PackageName = package.PackageName,
                OrderType = orderType,
                Amount = package.Price,
                Currency = "CNY",
                Status = OrderStatus.Pending,
                PaymentMethod = paymentMethod,
                EffectiveStartTime = effectiveStart,
                EffectiveEndTime = effectiveEnd,
                Remark = remark,
                CreateTime = DateTime.Now
            };

            _fsql.Insert(order).ExecuteAffrows();

            return ResultData.Ok(new
            {
                order.OrderNo,
                order.Amount,
                order.EffectiveStartTime,
                order.EffectiveEndTime
            });
        }

        /// <summary>
        /// 管理员手动确认支付（手动收款场景）
        /// </summary>
        public ResultData ConfirmPayment(string orderNo, string transactionId = "")
        {
            var order = _fsql.Select<Order>()
                .Where(o => o.OrderNo == orderNo && o.Status == OrderStatus.Pending)
                .First();
            if (order == null)
                return ResultData.No("订单不存在或非待支付状态!");

            return ProcessPaymentSuccess(order, "manual", transactionId);
        }

        /// <summary>
        /// 处理支付成功（通用）
        /// </summary>
        public ResultData ProcessPaymentSuccess(Order order, string paymentMethod, string transactionId)
        {
            // 更新订单状态
            order.Status = OrderStatus.Paid;
            order.PaymentMethod = paymentMethod;
            order.PaymentTime = DateTime.Now;
            order.PaymentTransactionId = transactionId;
            order.UpdateTime = DateTime.Now;

            _fsql.Update<Order>().SetSource(order)
                .UpdateColumns(o => new
                {
                    o.Status, o.PaymentMethod, o.PaymentTime,
                    o.PaymentTransactionId, o.UpdateTime
                }).ExecuteAffrows();

            // 应用套餐配额
            ApplyPackageQuotas(order);

            return ResultData.Ok("支付确认成功!");
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        public ResultData CancelOrder(string orderNo)
        {
            var order = _fsql.Select<Order>()
                .Where(o => o.OrderNo == orderNo && o.Status == OrderStatus.Pending)
                .First();
            if (order == null)
                return ResultData.No("订单不存在或无法取消!");

            _fsql.Update<Order>(order.Id)
                .Set(o => o.Status, OrderStatus.Cancelled)
                .Set(o => o.UpdateTime, DateTime.Now)
                .ExecuteAffrows();

            return ResultData.Ok("订单已取消!");
        }

        /// <summary>
        /// 订单列表（管理员）
        /// </summary>
        public ResultData GetOrderList(int? memberId, int? status, int page = 1, int pageSize = 20)
        {
            var query = _fsql.Select<Order>();

            if (memberId.HasValue)
                query = query.Where(o => o.MemberId == memberId.Value);
            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            var total = query.Count();
            var list = query.OrderByDescending(o => o.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(o => new
                {
                    o.Id,
                    o.OrderNo,
                    o.MemberId,
                    o.PackageId,
                    o.PackageName,
                    o.OrderType,
                    o.Amount,
                    o.Currency,
                    o.Status,
                    o.PaymentMethod,
                    o.PaymentTime,
                    o.EffectiveStartTime,
                    o.EffectiveEndTime,
                    o.Remark,
                    o.CreateTime
                });

            return ResultPage.Ok(list, total, page, pageSize);
        }

        /// <summary>
        /// 站长的订单列表
        /// </summary>
        public ResultData GetMyOrders(int page = 1, int pageSize = 20)
        {
            int currentId = _httpContextAccessor.HttpContext.User.Id();
            var member = _fsql.Select<Member>().Where(m => m.Id == currentId).First();
            int ownerId = member?.Role == MemberRole.Owner ? member.Id : (member?.OwnerId ?? 0);

            var query = _fsql.Select<Order>().Where(o => o.MemberId == ownerId);

            var total = query.Count();
            var list = query.OrderByDescending(o => o.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(o => new
                {
                    o.Id,
                    o.OrderNo,
                    o.PackageName,
                    o.OrderType,
                    o.Amount,
                    o.Status,
                    o.PaymentMethod,
                    o.PaymentTime,
                    o.EffectiveStartTime,
                    o.EffectiveEndTime,
                    o.CreateTime
                });

            return ResultPage.Ok(list, total, page, pageSize);
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        public ResultData GetOrderDetail(string orderNo)
        {
            var order = _fsql.Select<Order>()
                .Where(o => o.OrderNo == orderNo)
                .First();
            if (order == null)
                return ResultData.No("订单不存在!");

            return ResultData.Ok(order);
        }

        /// <summary>
        /// 获取站长当前有效套餐
        /// </summary>
        public ResultData GetCurrentPackage(int memberId)
        {
            var order = _fsql.Select<Order>()
                .Where(o => o.MemberId == memberId
                    && o.Status == OrderStatus.Paid
                    && o.EffectiveEndTime > DateTime.Now)
                .OrderByDescending(o => o.EffectiveEndTime)
                .First();

            if (order == null)
                return ResultData.Ok(new { hasPackage = false });

            return ResultData.Ok(new
            {
                hasPackage = true,
                order.PackageId,
                order.PackageName,
                order.EffectiveStartTime,
                order.EffectiveEndTime,
                daysRemaining = order.EffectiveEndTime.HasValue
                    ? (int)(order.EffectiveEndTime.Value - DateTime.Now).TotalDays
                    : 0
            });
        }

        /// <summary>
        /// 获取预警信息（到期提醒 + 超额预警）
        /// </summary>
        public ResultData GetAlerts(int memberId)
        {
            var now = DateTime.Now;
            var alerts = new List<object>();

            // 1. 配额到期提醒
            var expiringQuotas = _fsql.Select<MemberQuota>()
                .Where(q => q.MemberId == memberId
                    && q.ExpiresAt != null
                    && q.ExpiresAt > now
                    && q.ExpiresAt <= now.AddDays(7))
                .ToList();

            foreach (var quota in expiringQuotas)
            {
                var daysLeft = (int)(quota.ExpiresAt!.Value - now).TotalDays;
                var config = _fsql.Select<QuotaConfig>()
                    .Where(c => c.QuotaKey == quota.QuotaKey)
                    .First();

                alerts.Add(new
                {
                    type = "expiring",
                    level = daysLeft <= 1 ? "error" : (daysLeft <= 3 ? "warning" : "info"),
                    quotaKey = quota.QuotaKey,
                    quotaName = config?.QuotaName ?? quota.QuotaKey,
                    message = $"{config?.QuotaName ?? quota.QuotaKey} 将在 {daysLeft} 天后到期",
                    expiresAt = quota.ExpiresAt,
                    daysLeft
                });
            }

            // 2. 超额预警
            var quotaConfigs = _fsql.Select<QuotaConfig>()
                .Where(c => c.IsEnabled && c.QuotaType == 0)
                .ToList();

            foreach (var config in quotaConfigs)
            {
                int limit = _quotaService.GetEffectiveQuotaValue(memberId, config.QuotaKey);
                if (limit <= 0) continue;

                int used = 0;
                switch (config.QuotaKey)
                {
                    case "max_platforms":
                        used = (int)_fsql.Select<ChatPlatform>()
                            .Where(p => p.OwnerId == memberId).Count();
                        break;
                    case "max_agents":
                        used = (int)_fsql.Select<Member>()
                            .Where(m => m.OwnerId == memberId && m.Role == MemberRole.Agent).Count();
                        break;
                    default:
                        continue;
                }

                double percent = (double)used / limit * 100;
                if (percent >= 80)
                {
                    alerts.Add(new
                    {
                        type = "overage",
                        level = percent >= 95 ? "error" : "warning",
                        quotaKey = config.QuotaKey,
                        quotaName = config.QuotaName,
                        message = $"{config.QuotaName} 使用率 {percent:F0}% ({used}/{limit})",
                        used,
                        limit,
                        percent = Math.Round(percent, 1)
                    });
                }
            }

            return ResultData.Ok(alerts);
        }

        #region Private Helpers

        /// <summary>
        /// 应用套餐配额到站长
        /// </summary>
        private void ApplyPackageQuotas(Order order)
        {
            var package = _fsql.Select<QuotaPackage>()
                .Where(p => p.Id == order.PackageId)
                .First();
            if (package == null) return;

            Dictionary<string, int> settings;
            try
            {
                settings = JsonSerializer.Deserialize<Dictionary<string, int>>(package.QuotaSettings)
                    ?? new Dictionary<string, int>();
            }
            catch
            {
                return;
            }

            var remark = $"订单 {order.OrderNo} - {package.PackageName}";

            foreach (var kv in settings)
            {
                // 查找已有记录
                var existing = _fsql.Select<MemberQuota>()
                    .Where(q => q.MemberId == order.MemberId && q.QuotaKey == kv.Key)
                    .First();

                if (existing != null)
                {
                    existing.QuotaValue = kv.Value;
                    existing.GrantedBy = null;
                    existing.GrantedByType = "System";
                    existing.ExpiresAt = order.EffectiveEndTime;
                    existing.Remark = remark;
                    existing.UpdateTime = DateTime.Now;

                    _fsql.Update<MemberQuota>().SetSource(existing)
                        .UpdateColumns(q => new
                        {
                            q.QuotaValue, q.GrantedBy, q.GrantedByType,
                            q.ExpiresAt, q.Remark, q.UpdateTime
                        }).ExecuteAffrows();
                }
                else
                {
                    _fsql.Insert(new MemberQuota
                    {
                        MemberId = order.MemberId,
                        QuotaKey = kv.Key,
                        QuotaValue = kv.Value,
                        GrantedByType = "System",
                        ExpiresAt = order.EffectiveEndTime,
                        Remark = remark,
                        CreateTime = DateTime.Now,
                        IsDeleted = false
                    }).ExecuteAffrows();
                }
            }
        }

        /// <summary>
        /// 生成订单号
        /// </summary>
        private static string GenerateOrderNo()
        {
            return $"ORD{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        }

        #endregion
    }
}
