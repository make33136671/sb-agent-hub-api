using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Services;
using System.Security.Cryptography;
using System.Text;

namespace Sb.Apps.AgentHub.WebApi.Controllers;

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
/// 支付回调（公开接口，需签名验证）
/// </summary>
[ApiController, Route("/api/[controller]/[action]")]
public class PaymentCallbackController : Controller
{
    private readonly PaymentService _service;
    private readonly IConfiguration _config;

    public PaymentCallbackController(PaymentService service, IConfiguration config)
    {
        _service = service;
        _config = config;
    }

    /// <summary>
    /// 支付宝回调
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> AlipayNotify()
    {
        var form = await Request.ReadFormAsync();
        var orderNo = form["out_trade_no"].ToString();
        var transactionId = form["trade_no"].ToString();
        var totalAmount = decimal.TryParse(form["total_amount"], out var amount) ? amount : 0;
        var tradeStatus = form["trade_status"].ToString();
        var sign = form["sign"].ToString();
        var signType = form["sign_type"].ToString();

        if (string.IsNullOrEmpty(sign))
            return Content("fail");

        if (!VerifyAlipaySign(form, sign, signType))
            return Content("fail");

        if (tradeStatus != "TRADE_SUCCESS" && tradeStatus != "TRADE_FINISHED")
            return Content("success");

        _service.HandleCallback("alipay", orderNo, transactionId, totalAmount);
        return Content("success");
    }

    /// <summary>
    /// 微信支付回调
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> WechatNotify()
    {
        var body = await new StreamReader(Request.Body).ReadToEndAsync();
        if (string.IsNullOrEmpty(body))
            return Content("<xml><return_code><![CDATA[FAIL]]></return_code></xml>", "application/xml");

        var sign = ExtractXmlValue(body, "sign");
        if (string.IsNullOrEmpty(sign) || !VerifyWechatSign(body, sign))
            return Content("<xml><return_code><![CDATA[FAIL]]></return_code></xml>", "application/xml");

        var orderNo = ExtractXmlValue(body, "out_trade_no");
        var transactionId = ExtractXmlValue(body, "transaction_id");
        var totalAmount = decimal.TryParse(ExtractXmlValue(body, "total_fee"), out var fee) ? fee / 100m : 0;
        var resultCode = ExtractXmlValue(body, "result_code");

        if (resultCode != "SUCCESS")
            return Content("<xml><return_code><![CDATA[SUCCESS]]></return_code></xml>", "application/xml");

        _service.HandleCallback("wechat", orderNo, transactionId, totalAmount);
        return Content("<xml><return_code><![CDATA[SUCCESS]]></return_code></xml>", "application/xml");
    }

    /// <summary>
    /// 验证支付宝签名（RSA2）
    /// </summary>
    private bool VerifyAlipaySign(IFormCollection form, string sign, string signType)
    {
        var publicKey = _config["Payment:Alipay:PublicKey"];
        if (string.IsNullOrEmpty(publicKey)) return false;

        var signData = form
            .Where(kv => kv.Key != "sign" && kv.Key != "sign_type" && !string.IsNullOrEmpty(kv.Value.ToString()))
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => $"{kv.Key}={kv.Value}")
            .Aggregate((a, b) => $"{a}&{b}");

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
            var hashAlg = signType == "RSA2" ? HashAlgorithmName.SHA256 : HashAlgorithmName.SHA1;
            return rsa.VerifyData(Encoding.UTF8.GetBytes(signData), Convert.FromBase64String(sign), hashAlg, RSASignaturePadding.Pkcs1);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 验证微信支付签名（HMAC-SHA256 或 MD5）
    /// </summary>
    private bool VerifyWechatSign(string xmlBody, string sign)
    {
        var apiKey = _config["Payment:Wechat:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) return false;

        var pairs = new List<string>();
        var regex = new System.Text.RegularExpressions.Regex(@"<(\w+)><!\[CDATA\[(.*?)\]\]></\1>");
        foreach (System.Text.RegularExpressions.Match m in regex.Matches(xmlBody))
        {
            if (m.Groups[1].Value != "sign")
                pairs.Add($"{m.Groups[1].Value}={m.Groups[2].Value}");
        }

        pairs.Sort(StringComparer.Ordinal);
        var signData = string.Join("&", pairs) + $"&key={apiKey}";
        var expected = BitConverter.ToString(new HMACSHA256().ComputeHash(Encoding.UTF8.GetBytes(signData))).Replace("-", "").ToUpper();

        return string.Equals(expected, sign, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 从 XML 中提取字段值
    /// </summary>
    private static string ExtractXmlValue(string xml, string tag)
    {
        var match = System.Text.RegularExpressions.Regex.Match(xml, $@"<{tag}><!\[CDATA\[(.*?)\]\]></{tag}>");
        return match.Success ? match.Groups[1].Value : "";
    }
}

public class CreatePaymentRequest
{
    public string OrderNo { get; set; } = "";
    public string PaymentMethod { get; set; } = "manual";
}
