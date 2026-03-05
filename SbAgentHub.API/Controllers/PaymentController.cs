using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 支付管理（管理端）
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class PaymentController : Controller
{
    private readonly PaymentService _service;

    public PaymentController(PaymentService service)
    {
        _service = service;
    }

    /// <summary>
    /// 创建支付请求
    /// </summary>
    [HttpPost]
    public ResultData CreatePayment([FromBody] CreatePaymentRequest req)
    {
        return _service.CreatePayment(req.OrderNo, req.PaymentMethod);
    }

    /// <summary>
    /// 获取可用支付方式
    /// </summary>
    [HttpGet]
    public ResultData Methods()
    {
        return _service.GetPaymentMethods();
    }
}

/// <summary>
/// 支付回调（公开接口，无需认证）
/// </summary>
[ApiController, Route("/api/[controller]/[action]")]
public class PaymentCallbackController : Controller
{
    private readonly PaymentService _service;

    public PaymentCallbackController(PaymentService service)
    {
        _service = service;
    }

    /// <summary>
    /// 支付宝回调
    /// </summary>
    [HttpPost, AllowAnonymous]
    public IActionResult AlipayNotify()
    {
        // TODO: 验证支付宝签名
        // 从 Request.Form 中提取参数
        var orderNo = Request.Form["out_trade_no"].ToString();
        var transactionId = Request.Form["trade_no"].ToString();
        var totalAmount = decimal.TryParse(Request.Form["total_amount"], out var amount) ? amount : 0;
        var tradeStatus = Request.Form["trade_status"].ToString();

        if (tradeStatus != "TRADE_SUCCESS" && tradeStatus != "TRADE_FINISHED")
            return Content("success");

        _service.HandleCallback("alipay", orderNo, transactionId, totalAmount);
        return Content("success");
    }

    /// <summary>
    /// 微信支付回调
    /// </summary>
    [HttpPost, AllowAnonymous]
    public IActionResult WechatNotify()
    {
        // TODO: 验证微信签名，解析 XML/JSON 回调内容
        // 从请求体中提取订单信息
        return Content("<xml><return_code><![CDATA[SUCCESS]]></return_code></xml>",
            "application/xml");
    }
}

public class CreatePaymentRequest
{
    public string OrderNo { get; set; } = "";
    public string PaymentMethod { get; set; } = "manual";
}
