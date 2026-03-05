using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 订单管理
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class OrderController : Controller
{
    private readonly OrderService _service;

    public OrderController(OrderService service)
    {
        _service = service;
    }

    /// <summary>
    /// 创建订单
    /// </summary>
    [HttpPost]
    public ResultData Create([FromBody] CreateOrderRequest req)
    {
        return _service.CreateOrder(req.MemberId, req.PackageId, req.OrderType, req.PaymentMethod, req.Remark);
    }

    /// <summary>
    /// 管理员确认支付
    /// </summary>
    [HttpPost]
    public ResultData ConfirmPayment([FromBody] ConfirmPaymentRequest req)
    {
        return _service.ConfirmPayment(req.OrderNo, req.TransactionId);
    }

    /// <summary>
    /// 取消订单
    /// </summary>
    [HttpPost]
    public ResultData Cancel(string orderNo)
    {
        return _service.CancelOrder(orderNo);
    }

    /// <summary>
    /// 订单列表（管理员）
    /// </summary>
    [HttpGet]
    public ResultData List(int? memberId, int? status, int page = 1, int pageSize = 20)
    {
        return _service.GetOrderList(memberId, status, page, pageSize);
    }

    /// <summary>
    /// 我的订单（站长）
    /// </summary>
    [HttpGet]
    public ResultData MyOrders(int page = 1, int pageSize = 20)
    {
        return _service.GetMyOrders(page, pageSize);
    }

    /// <summary>
    /// 订单详情
    /// </summary>
    [HttpGet]
    public ResultData Detail(string orderNo)
    {
        return _service.GetOrderDetail(orderNo);
    }

    /// <summary>
    /// 站长当前有效套餐
    /// </summary>
    [HttpGet]
    public ResultData CurrentPackage(int memberId)
    {
        return _service.GetCurrentPackage(memberId);
    }

    /// <summary>
    /// 获取预警信息
    /// </summary>
    [HttpGet]
    public ResultData Alerts(int memberId)
    {
        return _service.GetAlerts(memberId);
    }
}

public class CreateOrderRequest
{
    public int MemberId { get; set; }
    public int PackageId { get; set; }
    public int OrderType { get; set; }
    public string PaymentMethod { get; set; } = "manual";
    public string Remark { get; set; } = "";
}

public class ConfirmPaymentRequest
{
    public string OrderNo { get; set; } = "";
    public string TransactionId { get; set; } = "";
}
