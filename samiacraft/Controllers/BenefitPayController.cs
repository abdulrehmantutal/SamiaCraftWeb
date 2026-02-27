using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using samiacraft.Services;

namespace samiacraft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BenefitPayController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BenefitPayGatewayService _benefitPayService;

        public BenefitPayController(IConfiguration configuration)
        {
            _configuration = configuration;

            var tranportalId = _configuration["BenefitPay:TranportalId"] ?? "";
            var tranportalPassword = _configuration["BenefitPay:TranportalPassword"] ?? "";
            var resourceKey = _configuration["BenefitPay:ResourceKey"] ?? "";
            var responseUrl = _configuration["BenefitPay:ResponseUrl"] ?? "";
            var errorUrl = _configuration["BenefitPay:ErrorUrl"] ?? "";
            var cancelUrl = _configuration["BenefitPay:CancelUrl"] ?? "";
            var isTestMode = _configuration.GetValue<bool>("BenefitPay:IsTestMode", false);

            _benefitPayService = new BenefitPayGatewayService(
                tranportalId, tranportalPassword, resourceKey,
                responseUrl, errorUrl, cancelUrl, isTestMode);
        }

        [HttpPost("response")]
        [IgnoreAntiforgeryToken]
        public IActionResult ProcessPaymentResponse()
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"]
                           ?? "http://samiacrafts-001-site5.site4future.com";
            var trandata = Request.Form["trandata"].ToString();

            if (!string.IsNullOrEmpty(trandata))
            {
                var parsed = _benefitPayService.ParseResponse(trandata);

                int.TryParse(parsed.trackId, out int orderId);

                if (parsed.result == "CAPTURED")
                {
                    return Content($"REDIRECT={baseUrl}/Order/OrderComplete?OrderID={orderId}");
                }
                else
                {
                    var reason = Uri.EscapeDataString(
                        BenefitPayGatewayService.GetDeclineMessage(parsed.authRespCode ?? ""));
                    return Content($"REDIRECT={baseUrl}/Order/PaymentFailed?OrderID={orderId}&reason={reason}");
                }
            }

            var errorText = Request.Form["ErrorText"].ToString();
            if (!string.IsNullOrEmpty(errorText))
            {
                int.TryParse(Request.Form["trackid"].ToString(), out int orderIdFromForm);
                var errMsg = Uri.EscapeDataString(errorText);
                return Content($"REDIRECT={baseUrl}/Order/PaymentFailed?OrderID={orderIdFromForm}&reason={errMsg}");
            }

            return Content($"REDIRECT={baseUrl}/Order/PaymentFailed");
        }

        [HttpGet("error")]
        [HttpPost("error")]
        [IgnoreAntiforgeryToken]
        public IActionResult HandlePaymentError()
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"]
                            ?? "http://samiacrafts-001-site5.site4future.com";
            var errorText = Request.Query["ErrorText"].ToString();
            if (string.IsNullOrEmpty(errorText))
                errorText = Request.Form["ErrorText"].ToString();

            var trackId = Request.Query["trackid"].ToString();
            if (string.IsNullOrEmpty(trackId))
                trackId = Request.Form["trackid"].ToString();

            int.TryParse(trackId, out int orderId);

            var errMsg = Uri.EscapeDataString(
                string.IsNullOrEmpty(errorText) ? "Payment processing error" : errorText);

            return Redirect($"{baseUrl}/Order/PaymentFailed?OrderID={orderId}&reason={errMsg}");
        }
    }
}