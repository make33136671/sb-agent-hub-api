using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Models;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 支付服务
    /// </summary>
    [AutoDependencyInjection]
    public class PaymentService
    {
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly OrderService _orderService;

        public PaymentService(ILogger<PaymentService> logger,
            IConfiguration configuration, OrderService orderService)
        {
            _logger = logger;
            _configuration = configuration;
            _orderService = orderService;
        }

        /// <summary>
        /// 创建支付请求（根据支付方式分发）
        /// </summary>
        public ResultData CreatePayment(string orderNo, string paymentMethod)
        {
            var order = _orderService.GetPendingByOrderNo(orderNo);
            if (order == null)
                return ResultData.No("订单不存在或非待支付状态!");

            switch (paymentMethod)
            {
                case "alipay":
                    return CreateAlipayPayment(order);
                case "wechat":
                    return CreateWechatPayment(order);
                case "manual":
                    return ResultData.Ok(new
                    {
                        paymentMethod = "manual",
                        orderNo = order.OrderNo,
                        amount = order.Amount,
                        message = "请联系管理员确认收款"
                    });
                default:
                    return ResultData.No("不支持的支付方式!");
            }
        }

        /// <summary>
        /// 处理支付回调（通用入口）
        /// </summary>
        public ResultData HandleCallback(string paymentMethod, string orderNo, string transactionId, decimal paidAmount)
        {
            var order = _orderService.GetByOrderNo(orderNo);
            if (order == null)
            {
                _logger.LogWarning("支付回调: 订单不存在 {OrderNo}", orderNo);
                return ResultData.No("订单不存在!");
            }

            if (order.Status == OrderStatus.Paid)
            {
                _logger.LogInformation("支付回调: 订单已支付，忽略重复通知 {OrderNo}", orderNo);
                return ResultData.Ok("已处理");
            }

            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogWarning("支付回调: 订单状态异常 {OrderNo} Status={Status}", orderNo, order.Status);
                return ResultData.No("订单状态异常!");
            }

            // 验证金额
            if (paidAmount < order.Amount)
            {
                _logger.LogWarning("支付回调: 金额不匹配 {OrderNo} 应付={Amount} 实付={Paid}",
                    orderNo, order.Amount, paidAmount);
                return ResultData.No("支付金额不匹配!");
            }

            _logger.LogInformation("支付回调: 确认支付 {OrderNo} Method={Method} TransId={TransId}",
                orderNo, paymentMethod, transactionId);

            return _orderService.ProcessPaymentSuccess(order, paymentMethod, transactionId);
        }

        /// <summary>
        /// 获取支付配置状态
        /// </summary>
        public ResultData GetPaymentMethods()
        {
            var alipayEnabled = !string.IsNullOrEmpty(_configuration["Payment:Alipay:AppId"]);
            var wechatEnabled = !string.IsNullOrEmpty(_configuration["Payment:Wechat:AppId"]);

            return ResultData.Ok(new[]
            {
                new { method = "manual", name = "手动收款", enabled = true, description = "管理员手动确认收款" },
                new { method = "alipay", name = "支付宝", enabled = alipayEnabled, description = "支付宝在线支付" },
                new { method = "wechat", name = "微信支付", enabled = wechatEnabled, description = "微信在线支付" }
            });
        }

        #region 支付宝（预留）

        private ResultData CreateAlipayPayment(Order order)
        {
            var appId = _configuration["Payment:Alipay:AppId"];
            if (string.IsNullOrEmpty(appId))
                return ResultData.No("支付宝支付未配置!");

            // TODO: 接入支付宝 SDK
            // 1. 使用 Alipay.AopSdk 创建支付请求
            // 2. 返回支付链接或二维码
            _logger.LogInformation("创建支付宝支付: OrderNo={OrderNo} Amount={Amount}", order.OrderNo, order.Amount);

            return ResultData.Ok(new
            {
                paymentMethod = "alipay",
                orderNo = order.OrderNo,
                amount = order.Amount,
                payUrl = "", // 支付宝支付链接
                qrCode = ""  // 二维码内容
            });
        }

        #endregion

        #region 微信支付（预留）

        private ResultData CreateWechatPayment(Order order)
        {
            var appId = _configuration["Payment:Wechat:AppId"];
            if (string.IsNullOrEmpty(appId))
                return ResultData.No("微信支付未配置!");

            // TODO: 接入微信支付 SDK
            // 1. 使用 WechatPay SDK 创建支付请求
            // 2. 返回支付链接或二维码
            _logger.LogInformation("创建微信支付: OrderNo={OrderNo} Amount={Amount}", order.OrderNo, order.Amount);

            return ResultData.Ok(new
            {
                paymentMethod = "wechat",
                orderNo = order.OrderNo,
                amount = order.Amount,
                payUrl = "",  // 微信支付链接
                qrCode = ""   // 二维码内容
            });
        }

        #endregion
    }
}
