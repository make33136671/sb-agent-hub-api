using FreeSql;
using Microsoft.AspNetCore.Http;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;
using System.Data.Common;
using System.Text.Json;

namespace Sb.Apps.AgentHub.Services
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
        private readonly MemberService _memberService;
        private readonly ChatPlatformService _platformService;

        public OrderService(IFreeSql fsql, IHttpContextAccessor httpContextAccessor,
            QuotaService quotaService, QuotaPackageService packageService,
            MemberService memberService, ChatPlatformService platformService)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _quotaService = quotaService;
            _packageService = packageService;
            _memberService = memberService;
            _platformService = platformService;
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        public ResultData CreateOrder(int memberId, int packageId, OrderType orderType, string paymentMethod, string remark = "")
        {
            var member = _memberService.GetOwner(memberId);
            if (member == null)
                return ResultData.No("站长不存在!");

            var package = _packageService.GetEnabledById(packageId);
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
        /// 处理支付成功（通用）— 事务包裹订单状态+配额应用
        /// </summary>
        public ResultData ProcessPaymentSuccess(Order order, string paymentMethod, string transactionId)
        {
            using var uow = _fsql.CreateUnitOfWork();
            var tx = uow.GetOrBeginTransaction();

            try
            {
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
                    }).WithTransaction(tx).ExecuteAffrows();

                ApplyPackageQuotas(order, tx);

                uow.Commit();
                return ResultData.Ok("支付确认成功!");
            }
            catch (Exception ex)
            {
                uow.Rollback();
                return ResultData.No($"支付处理失败: {ex.Message}");
            }
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
        public ResultData GetOrderList(int? memberId, OrderStatus? status, int page = 1, int pageSize = 20)
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
            int currentId = _httpContextAccessor.HttpContext!.User.Id();
            var member = _memberService.GetById(currentId);
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
            var expiringQuotas = _quotaService.GetExpiringQuotas(memberId, now);

            foreach (var quota in expiringQuotas)
            {
                var daysLeft = (int)(quota.ExpiresAt!.Value - now).TotalDays;
                var config = _quotaService.GetByQuotaKey(quota.QuotaKey);

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
            var quotaConfigs = _quotaService.GetEnabledQuantityConfigs();

            foreach (var config in quotaConfigs)
            {
                int limit = _quotaService.GetEffectiveQuotaValue(memberId, config.QuotaKey);
                if (limit <= 0) continue;

                int used = 0;
                switch (config.QuotaKey)
                {
                    case "max_platforms":
                        used = _platformService.CountByOwner(memberId);
                        break;
                    case "max_agents":
                        used = _memberService.CountActiveAgentsByOwner(memberId);
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

        #region 仓储访问入口（供其他 Service 调用）

        /// <summary>
        /// 按 OrderNo 获取订单
        /// </summary>
        public Order GetByOrderNo(string orderNo)
        {
            return _fsql.Select<Order>().Where(o => o.OrderNo == orderNo).First();
        }

        /// <summary>
        /// 按 OrderNo 获取待支付订单
        /// </summary>
        public Order GetPendingByOrderNo(string orderNo)
        {
            return _fsql.Select<Order>()
                .Where(o => o.OrderNo == orderNo && o.Status == OrderStatus.Pending)
                .First();
        }

        /// <summary>
        /// 按主键获取订单
        /// </summary>
        public Order GetById(int id)
        {
            return _fsql.Select<Order>().Where(o => o.Id == id).First();
        }

        /// <summary>
        /// 插入订单（供 MemberService.AssignTrialPackage 在事务内调用）
        /// </summary>
        public void InsertOrder(Order order, DbTransaction? tx = null)
        {
            _fsql.Insert(order).WithTransaction(tx).ExecuteAffrows();
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// 应用套餐配额到站长（在事务内执行）
        /// </summary>
        private void ApplyPackageQuotas(Order order, DbTransaction? tx = null)
        {
            var package = _packageService.GetById(order.PackageId);
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
                _quotaService.UpsertMemberQuota(order.MemberId, kv.Key, kv.Value,
                    "System", order.EffectiveEndTime, remark, tx);
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
