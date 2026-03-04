using Newtonsoft.Json;
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

        private string BaseUrl => "http://samiacrafts-001-site5.site4future.com";

        public BenefitPayController(IConfiguration configuration)
        {
            _configuration = configuration;

            var tranportalId = _configuration["BenefitPay:TranportalId"] ?? "";
            var tranportalPassword = _configuration["BenefitPay:TranportalPassword"] ?? "";
            var resourceKey = _configuration["BenefitPay:ResourceKey"] ?? "";
            var responseUrl = _configuration["BenefitPay:ResponseUrl"] ?? "";
            var errorUrl = _configuration["BenefitPay:ErrorUrl"] ?? "";
            var isTestMode = _configuration.GetValue<bool>("BenefitPay:IsTestMode", false);

            _benefitPayService = new BenefitPayGatewayService(
                tranportalId, tranportalPassword, resourceKey,
                responseUrl, errorUrl, isTestMode);
        }

        // ════════════════════════════════════════════════════════════════
        // POST /api/benefitpay/response
        // BENEFIT yahan POST karta hai payment ke baad
        // ⚠️ BENEFIT ke liye: "REDIRECT=url" return karo
        // ⚠️ PRG Pattern: POST → process → GET redirect (no resubmission)
        // ════════════════════════════════════════════════════════════════
        [HttpPost("response")]
        [IgnoreAntiforgeryToken]
        public IActionResult ProcessPaymentResponse()
        {
            var trandata = Request.Form["trandata"].ToString();

            if (!string.IsNullOrEmpty(trandata))
            {
                var parsed = _benefitPayService.ParseResponse(trandata);
                int.TryParse(parsed.trackId, out int orderId);

                if (parsed.result == "CAPTURED")
                {
                    // ✅ Success - BENEFIT ke liye REDIRECT= format
                    // Browser GET request karega - refresh problem nahi hogi
                    return Content($"REDIRECT={BaseUrl}/Order/OrderComplete?OrderID={orderId}");
                }
                else
                {
                    var reason = Uri.EscapeDataString(
                        BenefitPayGatewayService.GetDeclineMessage(parsed.authRespCode ?? ""));
                    return Content($"REDIRECT={BaseUrl}/Order/PaymentFailed?OrderID={orderId}&reason={reason}");
                }
            }

            // Error fields direct aaye
            var errorText = Request.Form["ErrorText"].ToString();
            if (!string.IsNullOrEmpty(errorText))
            {
                int.TryParse(Request.Form["trackid"].ToString(), out int orderIdFromForm);
                var errMsg = Uri.EscapeDataString(errorText);
                return Content($"REDIRECT={BaseUrl}/Order/PaymentFailed?OrderID={orderIdFromForm}&reason={errMsg}");
            }

            return Content($"REDIRECT={BaseUrl}");
        }

        // ════════════════════════════════════════════════════════════════
        // GET /api/benefitpay/response  (PRG - refresh safe)
        // Browser yahan aata hai BENEFIT ke REDIRECT= ke baad
        // Yeh GET request hai - refresh karo toh koi problem nahi
        // ════════════════════════════════════════════════════════════════
        [HttpGet("response")]
        public IActionResult PaymentResponseGet(
            [FromQuery] string result = "",
            [FromQuery] int orderId = 0)
        {
            if (result == "success")
                return Redirect($"{BaseUrl}/Order/OrderComplete?OrderID={orderId}");

            return Redirect($"{BaseUrl}/Order/PaymentFailed?OrderID={orderId}");
        }

        // ════════════════════════════════════════════════════════════════
        // POST /api/benefitpay/error
        // BENEFIT cancel/error pe yahan POST karta hai
        // ════════════════════════════════════════════════════════════════
        [HttpPost("error")]
        [IgnoreAntiforgeryToken]
        public IActionResult HandlePaymentErrorPost()
        {
            var errorText = Request.Form["ErrorText"].ToString();
            var trackId = Request.Form["trackid"].ToString();
            int.TryParse(trackId, out int orderId);

            // PRG: POST data ko query string mein daal ke GET pe redirect karo
            // Ab refresh karne pe POST dobara nahi hoga - sirf GET hoga
            if (string.IsNullOrEmpty(errorText) ||
                errorText.ToLower().Contains("cancel") ||
                errorText.ToLower().Contains("cancelled"))
            {
                // User ne cancel kiya - home page
                return Redirect($"{BaseUrl}/api/benefitpay/error/cancelled");
            }

            var errMsg = Uri.EscapeDataString(errorText);
            return Redirect($"{BaseUrl}/api/benefitpay/error/failed?orderid={orderId}&reason={errMsg}");
        }

        // ════════════════════════════════════════════════════════════════
        // GET /api/benefitpay/error/cancelled  (PRG safe - refresh ok)
        // ════════════════════════════════════════════════════════════════
        [HttpGet("error/cancelled")]
        public IActionResult PaymentCancelled()
        {
            // Refresh karo - yeh sirf GET hai, koi POST nahi hoga
            return Redirect($"{BaseUrl}");  // Home page
        }

        // ════════════════════════════════════════════════════════════════
        // GET /api/benefitpay/error/failed  (PRG safe - refresh ok)
        // ════════════════════════════════════════════════════════════════
        [HttpGet("error/failed")]
        public IActionResult PaymentFailed(
            [FromQuery] int orderid = 0,
            [FromQuery] string reason = "")
        {
            // Refresh karo - yeh sirf GET hai, koi POST nahi hoga
            return Redirect($"{BaseUrl}/Order/PaymentFailed?OrderID={orderid}&reason={reason}");
        }
    }
}