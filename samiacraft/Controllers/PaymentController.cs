using BenefitPG.Services;
using Microsoft.AspNetCore.Mvc;
using samiacraft.Models.BLL;

namespace BenefitPG.Controllers
{
    public class PaymentController : Controller
    {
        // ─────────────────────────────────────────────────────────────────────
        // TODO: Replace these values with your actual credentials from bank
        // ─────────────────────────────────────────────────────────────────────
        private const string TRANPORTAL_ID = "YOUR_TRANPORTAL_ID";   // e.g. "101001"
        private const string TRANPORTAL_PWD = "YOUR_PASSWORD";         // e.g. "merchantdemo123"
        private const string RESOURCE_KEY = "YOUR_RESOURCE_KEY";     // 16 or 32 char key from bank
        private const bool USE_PRODUCTION = false;                   // false = UAT, true = Live

        // Your site base URL (must be publicly accessible, not localhost)
        private const string SITE_BASE_URL = "http://samiacrafts-001-site5.site4future.com/";

        // ─── GET: /Payment/Index ──────────────────────────────────────────────
        // Shows the payment initiation page
        [HttpGet]
        public IActionResult Index()
        {
            return View(new PaymentRequestModel());
        }

        // ─── POST: /Payment/Index ─────────────────────────────────────────────
        // Initiates payment and redirects customer to BENEFIT Gateway
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PaymentRequestModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var pipe = new iPayBenefitPipe
            {
                // Fixed values (do not change per docs)
                Action = "1",
                CurrencyCode = "048",
                Language = "EN",

                // Your credentials
                Id = TRANPORTAL_ID,
                Password = TRANPORTAL_PWD,
                ResourceKey = RESOURCE_KEY,

                // Transaction data from form
                Amt = model.Amount,
                TrackId = model.TrackId,
                Udf1 = "",  // Always empty per BENEFIT docs
                Udf2 = model.Udf2,
                Udf3 = model.Udf3,
                Udf4 = model.Udf4,
                Udf5 = model.Udf5,

                // Response URLs - must be publicly accessible
                ResponseURL = $"{SITE_BASE_URL}/Payment/Response",
                ErrorURL = $"{SITE_BASE_URL}/Payment/Error",
            };

            var result = await pipe.PerformTransactionAsync(USE_PRODUCTION);

            if (result.IsSuccess)
            {
                // Redirect customer to BENEFIT hosted payment page
                return Redirect(result.PaymentPageUrl);
            }
            else
            {
                ViewBag.Error = result.RawErrorText ?? result.Error ?? "Unknown error occurred.";
                return View(model);
            }
        }

        // ─── POST: /Payment/Response ──────────────────────────────────────────
        // BENEFIT PG calls this URL with encrypted trandata (Merchant Notification)
        // IMPORTANT: This page must return ONLY "REDIRECT=<url>" - NO HTML allowed
        [HttpPost]
        public IActionResult Response()
        {
            var trandata = Request.Form["trandata"].ToString();
            var errorText = Request.Form["ErrorText"].ToString();

            // Case 1: We got encrypted trandata
            if (!string.IsNullOrEmpty(trandata))
            {
                var pipe = new iPayBenefitPipe { ResourceKey = RESOURCE_KEY };
                bool parsed = pipe.ParseResponse(trandata);

                if (parsed)
                {
                    // Store result in TempData for the final page
                    TempData["PaymentId"] = pipe.PaymentId;
                    TempData["Result"] = pipe.Result;
                    TempData["AuthRespCode"] = pipe.AuthRespCode;
                    TempData["AuthCode"] = pipe.AuthCode;
                    TempData["TransId"] = pipe.TransId;
                    TempData["Ref"] = pipe.Ref;
                    TempData["Date"] = pipe.Date;
                    TempData["TrackId"] = pipe.TrackId;
                    TempData["Amt"] = pipe.Amt;
                    TempData["Udf2"] = pipe.Udf2;
                    TempData["Udf3"] = pipe.Udf3;
                    TempData["Udf4"] = pipe.Udf4;
                    TempData["Udf5"] = pipe.Udf5;

                    // ⚠️ BENEFIT requires ONLY this line - no HTML/JS
                    if (pipe.Result == "CAPTURED")
                    {
                        return Content($"REDIRECT={SITE_BASE_URL}/Payment/Approved");
                    }
                    else
                    {
                        TempData["ErrorCode"] = pipe.ErrorCode;
                        TempData["ErrorText"] = pipe.ErrorText;
                        TempData["ResponseMsg"] = iPayBenefitPipe.GetResponseDescription(pipe.AuthRespCode);
                        return Content($"REDIRECT={SITE_BASE_URL}/Payment/Declined");
                    }
                }
                else
                {
                    return Content($"REDIRECT={SITE_BASE_URL}/Payment/Error");
                }
            }
            // Case 2: Error came as plain form fields (Merchant Notification disabled)
            else if (!string.IsNullOrEmpty(errorText))
            {
                TempData["ErrorText"] = errorText;
                TempData["PaymentId"] = Request.Form["paymentid"].ToString();
                TempData["TrackId"] = Request.Form["trackid"].ToString();
                TempData["Amt"] = Request.Form["amt"].ToString();
                return Content($"REDIRECT={SITE_BASE_URL}/Payment/Declined");
            }

            return Content($"REDIRECT={SITE_BASE_URL}/Payment/Error");
        }

        // ─── GET: /Payment/Approved ───────────────────────────────────────────
        // Final success page shown to customer
        [HttpGet]
        public IActionResult Approved()
        {
            var model = new PaymentResultModel
            {
                Status = "approved",
                PaymentId = TempData["PaymentId"]?.ToString() ?? "",
                Result = TempData["Result"]?.ToString() ?? "",
                AuthRespCode = TempData["AuthRespCode"]?.ToString() ?? "",
                AuthCode = TempData["AuthCode"]?.ToString() ?? "",
                TransId = TempData["TransId"]?.ToString() ?? "",
                Ref = TempData["Ref"]?.ToString() ?? "",
                Date = TempData["Date"]?.ToString() ?? "",
                TrackId = TempData["TrackId"]?.ToString() ?? "",
                Amount = TempData["Amt"]?.ToString() ?? "",
                Udf2 = TempData["Udf2"]?.ToString() ?? "",
                Udf3 = TempData["Udf3"]?.ToString() ?? "",
                Udf4 = TempData["Udf4"]?.ToString() ?? "",
                Udf5 = TempData["Udf5"]?.ToString() ?? "",
            };
            return View(model);
        }

        // ─── GET: /Payment/Declined ───────────────────────────────────────────
        // Final failure page shown to customer
        [HttpGet]
        public IActionResult Declined()
        {
            var model = new PaymentResultModel
            {
                Status = "declined",
                PaymentId = TempData["PaymentId"]?.ToString() ?? "",
                Result = TempData["Result"]?.ToString() ?? "",
                TrackId = TempData["TrackId"]?.ToString() ?? "",
                Amount = TempData["Amt"]?.ToString() ?? "",
                ErrorText = TempData["ErrorText"]?.ToString() ?? "",
                ErrorCode = TempData["ErrorCode"]?.ToString() ?? "",
                ResponseMsg = TempData["ResponseMsg"]?.ToString() ?? "Transaction was not successful.",
                Udf2 = TempData["Udf2"]?.ToString() ?? "",
                Udf3 = TempData["Udf3"]?.ToString() ?? "",
            };
            return View(model);
        }

        // ─── GET: /Payment/Error ──────────────────────────────────────────────
        // Error page (shown when BENEFIT PG sends to errorURL)
        [HttpGet]
        public IActionResult Error()
        {
            var errorText = Request.Query["ErrorText"].ToString();
            var paymentId = Request.Query["paymentid"].ToString();
            var error = Request.Query["Error"].ToString();

            var model = new PaymentResultModel
            {
                Status = "error",
                PaymentId = paymentId,
                ErrorText = string.IsNullOrEmpty(errorText)
                    ? (TempData["ErrorText"]?.ToString() ?? "An error occurred during payment processing.")
                    : errorText,
                ErrorCode = error,
            };
            return View(model);
        }
    }
}