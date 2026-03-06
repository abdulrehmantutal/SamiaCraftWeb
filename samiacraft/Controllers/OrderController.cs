using samiacraft.Models.BLL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Mail;
using System.Net;
using samiacraft.Helpers;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using samiacraft.Services;

namespace samiacraft.Controllers
{
    public class OrderController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly BenefitGatewayService _benefitGatewayService;
        private readonly BenefitPayGatewayService _benefitPayGatewayService;

        public OrderController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, BenefitGatewayService benefitGatewayService)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _benefitGatewayService = benefitGatewayService;

            string tranportalId = _configuration["BenefitPay:TranportalId"] ?? "";
            string tranportalPassword = _configuration["BenefitPay:TranportalPassword"] ?? "";
            string resourceKey = _configuration["BenefitPay:ResourceKey"] ?? "";
            string responseUrl = _configuration["BenefitPay:ResponseUrl"] ?? "";
            string errorUrl = _configuration["BenefitPay:ErrorUrl"] ?? "";
            bool isTestMode = bool.Parse(_configuration["BenefitPay:IsTestMode"] ?? "false");

            _benefitPayGatewayService = new BenefitPayGatewayService(
                tranportalId,
                tranportalPassword,
                resourceKey,
                responseUrl,
                errorUrl,
                isTestMode
            );
        }

        public IActionResult Cart()
        {
            try
            {
                ViewBag.ImageUrl = _configuration["Image"] ?? "https://retail.premium-pos.com";
                int UserId = Convert.ToInt32(_configuration["UserId"]);

                var settng = new settingBLL().GetSettings(UserId);
                if (settng?.DynamicList?.Count > 0)
                {
                    ViewBag.Logo = settng.DynamicList[0].Logo;
                    ViewBag.TopheaderArea = settng.DynamicList[0].HeaderToparea;
                    ViewBag.AddButton = settng.DynamicList[0].AddButton;
                }
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        public IActionResult Checkout(int id = -1)
        {
            try
            {
                ViewBag.ImageUrl = _configuration["Image"] ?? "https://retail.premium-pos.com";
                int UserId = Convert.ToInt32(_configuration["UserId"]);

                var settng = new settingBLL().GetSettings(UserId);
                if (settng?.DynamicList?.Count > 0)
                {
                    ViewBag.Logo = settng.DynamicList[0].Logo;
                    ViewBag.TopheaderArea = settng.DynamicList[0].HeaderToparea;
                    ViewBag.AddButton = settng.DynamicList[0].AddButton;
                }

                int CustomerID = id;
                if (CustomerID == -1)
                {
                    // Direct checkout from "Proceed to Checkout" button - allow guest checkout
                    HttpContext.Session.SetInt32("CustomerID", 0);
                    return View();
                }
                else if (CustomerID == 0)
                {
                    HttpContext.Session.SetInt32("CustomerID", 0);
                    return View();
                }
                else
                {
                    var sessionCustomerId = HttpContext.Session.GetInt32("CustomerID");
                    if (sessionCustomerId != null && sessionCustomerId != 0)
                    {
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("Login_Register", "Account");
                    }
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [HttpPost]
        public JsonResult PunchOrder([FromBody] checkoutBLL data)
        {
            try
            {
                var currDate = DateTime.UtcNow.AddMinutes(300);
                int rtn = 0;

                checkoutBLL _service = new checkoutBLL();

                if (!string.IsNullOrEmpty(data.OrderDetailString))
                {
                    string json = JsonConvert.SerializeObject(JArray.Parse(data.OrderDetailString));
                    JArray jsonResponse = JArray.Parse(json);
                    data.OrderDetail = jsonResponse.ToObject<List<checkoutBLL.OrderDetails>>();
                }

                rtn = _service.InsertOrder(data);

                if (rtn > 0)
                {
                    SendOrderEmails(rtn, data);
                }

                if (data.PaymentMethodID == 1)
                {
                    return Json(new { data = rtn });
                }
                else
                {
                    return Json(new { data = "DebitCreditCard", OrderID = rtn });
                }
            }
            catch (Exception ex)
            {
                return Json(new { data = 0, error = ex.Message });
            }
        }

        // ============================================================================
        // PLACE ORDER - MAIN ENTRY POINT
        // ============================================================================

        [HttpPost]
        [Route("Order/PlaceOrder")]
        public async Task<JsonResult> PlaceOrder()
        {
            try
            {
                var checkoutData = ExtractCheckoutData();

                if (checkoutData == null)
                    return Json(new { Success = false, Message = "Invalid order data" });

                var validationResult = ValidateCheckoutData(checkoutData);
                if (!validationResult.IsValid)
                    return Json(new { Success = false, Message = validationResult.Message });

                ParseOrderItems(checkoutData);

                if (checkoutData.OrderDetail == null || checkoutData.OrderDetail.Count == 0)
                    return Json(new { Success = false, Message = "No items in order" });

                var checkoutService = new checkoutBLL();

                if (checkoutData.PaymentMethodID == 1 || checkoutData.PaymentMethodID == 5)
                {
                    checkoutData.DeliveryStatus = 101;
                    checkoutData.StatusID = 2;
                }
                else if (checkoutData.PaymentMethodID == 2 || checkoutData.PaymentMethodID == 3)
                {
                    checkoutData.DeliveryStatus = 104;
                    checkoutData.StatusID = 3;
                }

                int OrderID = checkoutService.InsertOrder(checkoutData);

                if (OrderID <= 0)
                    return Json(new { Success = false, Message = "Failed to create order" });

                int paymentType = checkoutData.PaymentMethodID ?? 1;

                switch (paymentType)
                {
                    case 1: // Cash on Delivery
                        {
                            var giftDataJson = HttpContext.Items["GiftDataJson"]?.ToString() ?? "[]";
                            SendCompleteOrderEmails(OrderID, checkoutData, giftDataJson);
                            return Json(new
                            {
                                Success = true,
                                orderid = OrderID,
                                orderno = OrderID.ToString(),
                                redirectUrl = $"/Order/OrderComplete?OrderID={OrderID}"
                            });
                        }

                    case 2: // Mastercard (AFS)
                        return await HandleAfsPayment(OrderID, checkoutData);

                    case 3: // BenefitPay
                        return await HandleBenefitPayment(OrderID, checkoutData);

                    case 4: // Bank Transfer
                        {
                            var giftDataJson = HttpContext.Items["GiftDataJson"]?.ToString() ?? "[]";
                            SendPendingPaymentEmails(OrderID, checkoutData, giftDataJson);
                            return Json(new
                            {
                                Success = true,
                                orderid = OrderID,
                                orderno = OrderID.ToString(),
                                paymentPending = true
                            });
                        }

                    case 5: // COD / Bank Transfer
                        {
                            var giftDataJson = HttpContext.Items["GiftDataJson"]?.ToString() ?? "[]";
                            SendCompleteOrderEmails(OrderID, checkoutData, giftDataJson);
                            return Json(new
                            {
                                Success = true,
                                orderid = OrderID,
                                orderno = OrderID.ToString(),
                                redirectUrl = $"/Order/OrderComplete?OrderID={OrderID}"
                            });
                        }

                    default:
                        return Json(new { Success = false, Message = "Unknown payment method" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        // ============================================================================
        // MASTERCARD (AFS) PAYMENT
        // ============================================================================

        //private async Task<JsonResult> HandleAfsPayment(int OrderID, checkoutBLL data)
        //{
        //    try
        //    {
        //        var giftDataJson = HttpContext.Items["GiftDataJson"]?.ToString() ?? "[]";
        //        SendPendingPaymentEmails(OrderID, data, giftDataJson);

        //        string sessionUrl = "https://afs.gateway.mastercard.com/api/rest/version/72/merchant/100538314/session";
        //        string credentials = "merchant.100538314:b671b94a10177ff07386518e6b1aef86";
        //        string base64Creds = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

        //        // ✅ Dynamic base URL - localhost ya production automatically detect
        //        string baseUrl = $"{Request.Scheme}://{Request.Host}";

        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //        using (var client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Clear();
        //            client.DefaultRequestHeaders.Add("Authorization", $"Basic {base64Creds}");

        //            var payload = new
        //            {
        //                apiOperation = "INITIATE_CHECKOUT",
        //                order = new
        //                {
        //                    amount = Math.Round(data.GrandTotal ?? 0, 3),
        //                    id = OrderID,
        //                    currency = "BHD"
        //                },
        //                interaction = new
        //                {
        //                    operation = "PURCHASE",
        //                    merchant = new { name = "HANDCRAFTED HEAVEN TRADING BY SAMIA WLL" },
        //                    displayControl = new { billingAddress = "HIDE" },
        //                    // ✅ returnUrl - AFS success ke baad yahan aayega
        //                    returnUrl = $"{baseUrl}/Order/MastercardCallback?OrderID={OrderID}"
        //                }
        //            };

        //            var json = JsonConvert.SerializeObject(payload,
        //                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        //            System.Diagnostics.Debug.WriteLine($"[AFS] Payload: {json}");

        //            var content = new StringContent(json, Encoding.UTF8, "application/json");
        //            var response = await client.PostAsync(sessionUrl, content);
        //            var responseContent = await response.Content.ReadAsStringAsync();

        //            System.Diagnostics.Debug.WriteLine($"[AFS] Response: {responseContent}");

        //            if (response.IsSuccessStatusCode)
        //            {
        //                dynamic responseData = JsonConvert.DeserializeObject(responseContent);
        //                string sessionID = responseData["session"]["id"];

        //                return Json(new
        //                {
        //                    Success = true,
        //                    orderid = OrderID,
        //                    orderno = OrderID.ToString(),
        //                    sessionID = sessionID,
        //                    redirectUrl = $"https://afs.gateway.mastercard.com/checkout/pay/{sessionID}?checkoutVersion=1.0.0"
        //                });
        //            }
        //            else
        //            {
        //                return Json(new { Success = false, Message = $"AFS session failed: {responseContent}" });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Success = false, Message = $"AFS error: {ex.Message}" });
        //    }
        //}
        private async Task<JsonResult> HandleAfsPayment(int OrderID, checkoutBLL data)
        {
            try
            {
                var giftDataJson = HttpContext.Items["GiftDataJson"]?.ToString() ?? "[]";
                SendPendingPaymentEmails(OrderID, data, giftDataJson);

                string sessionUrl = "https://afs.gateway.mastercard.com/api/rest/version/72/merchant/100538314/session";
                string credentials = "merchant.100538314:b671b94a10177ff07386518e6b1aef86";
                string base64Creds = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

                string baseUrl = $"{Request.Scheme}://{Request.Host}";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {base64Creds}");

                    var payload = new
                    {
                        apiOperation = "INITIATE_CHECKOUT",
                        order = new
                        {
                            amount = Math.Round(data.GrandTotal ?? 0, 3),
                            id = OrderID,
                            currency = "BHD"
                        },
                        interaction = new
                        {
                            operation = "PURCHASE",
                            merchant = new { name = "HANDCRAFTED HEAVEN TRADING BY SAMIA WLL" },
                            displayControl = new { billingAddress = "HIDE" },
                            // ✅ Success redirect
                            returnUrl = $"{baseUrl}/Order/MastercardCallback?OrderID={OrderID}",
                            // ✅ Back/Cancel button click pe checkout page pe wapas
                            cancelUrl = $"{baseUrl}/Order/Checkout"
                        }
                    };

                    var json = JsonConvert.SerializeObject(payload,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    System.Diagnostics.Debug.WriteLine($"[AFS] Payload: {json}");

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(sessionUrl, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine($"[AFS] Response: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic responseData = JsonConvert.DeserializeObject(responseContent);
                        string sessionID = responseData["session"]["id"];

                        return Json(new
                        {
                            Success = true,
                            orderid = OrderID,
                            orderno = OrderID.ToString(),
                            sessionID = sessionID,
                            redirectUrl = $"https://afs.gateway.mastercard.com/checkout/pay/{sessionID}?checkoutVersion=1.0.0"
                        });
                    }
                    else
                    {
                        return Json(new { Success = false, Message = $"AFS session failed: {responseContent}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"AFS error: {ex.Message}" });
            }
        }

        // ============================================================================
        // MASTERCARD CALLBACK
        // Payment success → Alert → CompleteOrder (status+email) → Home
        // ============================================================================

        [HttpGet]
        public IActionResult MastercardCallback(int OrderID = 0, string resultIndicator = "")
        {
            if (OrderID <= 0 || string.IsNullOrEmpty(resultIndicator))
            {
                TempData["PaymentFailed"] = true;
                return Redirect("/");
            }

            // ✅ Payment success - alert → status update + email → localStorage clear → home
            return Content($@"
        <!DOCTYPE html>
        <html>
        <body>
        <script>
            alert('✅ Payment Successful!\n\nYour order #{OrderID} has been placed successfully.\nYou will receive a confirmation email shortly. Thank you!');
            
            fetch('/Order/CompleteOrder?OrderID={OrderID}')
                .then(function() {{
                    // ✅ Payment complete hone ke BAAD clear karo
                    localStorage.removeItem('_cartitems');
                    localStorage.removeItem('_giftitems');
                    sessionStorage.removeItem('_pendingOrderID');
                    window.location.href = '/';
                }})
                .catch(function() {{
                    localStorage.removeItem('_cartitems');
                    localStorage.removeItem('_giftitems');
                    sessionStorage.removeItem('_pendingOrderID');
                    window.location.href = '/';
                }});
        </script>
        </body>
        </html>
    ", "text/html");
        }

        // ============================================================================
        // COMPLETE ORDER - Status Update + Email (AJAX se call hota hai)
        // ============================================================================

        [HttpGet]
        public IActionResult CompleteOrder(int OrderID = 0)
        {
            try
            {
                // ✅ 101 = Approved/Completed
                var checkoutService = new checkoutBLL();
                checkoutService.OrderUpdate(OrderID, 101);

                var orderDetails = new myorderBLL().GetDetails(OrderID);
                if (orderDetails != null)
                {
                    var checkoutData = new checkoutBLL
                    {
                        OrderNo = orderDetails.OrderID ?? 0,
                        Email = orderDetails.Email ?? "",
                        CustomerName = orderDetails.CustomerName ?? "",
                        ContactNo = orderDetails.ContactNo ?? "",
                        GrandTotal = orderDetails.GrandTotal ?? 0,
                        Tax = orderDetails.Tax ?? 0,
                        PaymentMethodID = 2 // Mastercard
                    };
                    SendCompleteOrderEmails(OrderID, checkoutData, "[]");
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================================================
        // BENEFITPAY PAYMENT
        // ============================================================================

        private async Task<JsonResult> HandleBenefitPayment(int OrderID, checkoutBLL data)
        {
            try
            {
                var giftDataJson = HttpContext.Items["GiftDataJson"]?.ToString() ?? "[]";
                SendPendingPaymentEmails(OrderID, data, giftDataJson);

                // ✅ Production pe AppSettings:BaseUrl use karo, localhost pe Request se lo
                string baseUrl = _configuration["AppSettings:BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host}";
                }
                // Trailing slash hata do
                baseUrl = baseUrl.TrimEnd('/');

                var result = _benefitPayGatewayService.InitiatePayment(
                    trackId: OrderID.ToString(),
                    amount: (decimal)(data.GrandTotal ?? 0),
                    currency: "048",   // BHD
                    cardType: "D",
                    udf1: OrderID.ToString(),
                    udf2: data.CustomerID?.ToString() ?? "",
                    udf3: data.CustomerName ?? "",
                    udf4: data.Email ?? "",
                    udf5: data.GrandTotal?.ToString("F2") ?? "",
                    overrideResponseUrl: $"{baseUrl}/Order/BenefitPayResponse?OrderID={OrderID}",
                    overrideErrorUrl: $"{baseUrl}/Order/BenefitPayResponse?OrderID=0"
                );

                if (result.IsSuccessful)
                {
                    return Json(new
                    {
                        Success = true,
                        orderid = OrderID,
                        orderno = OrderID.ToString(),
                        redirectUrl = result.RedirectUrl,
                        paymentType = "BenefitPay"
                    });
                }
                else
                {
                    return Json(new
                    {
                        Success = false,
                        Message = $"BenefitPay initialization failed: {result.ErrorMessage}"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"BenefitPay error: {ex.Message}" });
            }
        }

        // ============================================================================
        // BENEFITPAY RESPONSE - Gateway se callback
        // ============================================================================

        [HttpPost]
        [HttpGet]
        public IActionResult BenefitPayResponse()
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host}";

            if (Request.Method == "GET")
            {
                return Redirect("/");
            }

            try
            {
                int orderID = 0;
                if (Request.Query.ContainsKey("OrderID"))
                    int.TryParse(Request.Query["OrderID"], out orderID);

                if (orderID == 0)
                {
                    return Redirect("/Order/Checkout");
                }

                string encryptedResponse = "";
                if (Request.Form.ContainsKey("trandata"))
                    encryptedResponse = Request.Form["trandata"].ToString();

                var paymentRequest = new PaymentResponseRequest
                {
                    OrderID = orderID,
                    EncryptedResponse = encryptedResponse
                };

                var paymentResult = _benefitGatewayService.ProcessPaymentResponse(paymentRequest);

                if (paymentResult.IsSuccessful && paymentResult.Result == "CAPTURED")
                {
                    return Content($"REDIRECT={baseUrl}/Order/BenefitPayApproved?OrderID={paymentResult.OrderID}");
                }
                else
                {
                    // ✅ Failed/Cancelled - home pe bhejo
                    return Content($"REDIRECT={baseUrl}/");
                }
            }
            catch (Exception ex)
            {
                LogPaymentError(0, "BenefitPay Response Exception", ex.Message);
                return Redirect("/");
            }
        }

        // ============================================================================
        // BENEFITPAY APPROVED
        // Payment success → Alert → CompleteOrder (status+email) → Home
        // ============================================================================

        [HttpGet]
        public IActionResult BenefitPayApproved(int OrderID)
        {
            try
            {
                if (OrderID <= 0)
                {
                    return Redirect("/");
                }

                // ✅ Payment success - alert → status update + email → localStorage clear → home
                return Content($@"
            <!DOCTYPE html>
            <html>
            <body>
            <script>
                alert('✅ Payment Successful!\n\nYour order #{OrderID} has been placed successfully.\nYou will receive a confirmation email shortly. Thank you!');
                
                fetch('/Order/BenefitCompleteOrder?OrderID={OrderID}')
                    .then(function() {{
                        // ✅ Payment complete hone ke BAAD clear karo
                        localStorage.removeItem('_cartitems');
                        localStorage.removeItem('_giftitems');
                        sessionStorage.removeItem('_pendingOrderID');
                        window.location.href = '/';
                    }})
                    .catch(function() {{
                        localStorage.removeItem('_cartitems');
                        localStorage.removeItem('_giftitems');
                        sessionStorage.removeItem('_pendingOrderID');
                        window.location.href = '/';
                    }});
            </script>
            </body>
            </html>
        ", "text/html");
            }
            catch (Exception ex)
            {
                LogPaymentError(OrderID, "BenefitPay Approval Failed", ex.Message);
                return Redirect("/");
            }
        }

        // ============================================================================
        // BENEFIT COMPLETE ORDER - Status Update + Email (AJAX se call hota hai)
        // ============================================================================

        [HttpGet]
        public IActionResult BenefitCompleteOrder(int OrderID = 0)
        {
            try
            {
                // ✅ 101 = Approved/Completed
                var checkoutService = new checkoutBLL();
                checkoutService.OrderUpdate(OrderID, 101);

                var orderDetails = new myorderBLL().GetDetails(OrderID);
                if (orderDetails != null)
                {
                    var checkoutData = new checkoutBLL
                    {
                        OrderNo = orderDetails.OrderID ?? 0,
                        Email = orderDetails.Email ?? "",
                        CustomerName = orderDetails.CustomerName ?? "",
                        ContactNo = orderDetails.ContactNo ?? "",
                        GrandTotal = orderDetails.GrandTotal ?? 0,
                        Tax = orderDetails.Tax ?? 0,
                        PaymentMethodID = 3 // BenefitPay
                    };
                    SendCompleteOrderEmails(OrderID, checkoutData, "[]");
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================================================
        // PAYMENT CALLBACK (Generic)
        // ============================================================================

        [HttpGet]
        public IActionResult PaymentCallback(int OrderID = 0, string status = "")
        {
            try
            {
                if (status == "success" && OrderID > 0)
                {
                    var checkoutService = new checkoutBLL();
                    checkoutService.OrderUpdate(OrderID, 101);
                    return RedirectToAction("OrderComplete", new { OrderID = OrderID });
                }
                else
                {
                    return RedirectToAction("OrderComplete", new { OrderID = OrderID, OrderNo = "Reject" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("OrderComplete", new { OrderNo = "Reject" });
            }
        }

        // ============================================================================
        // LOG PAYMENT ERROR
        // ============================================================================

        private void LogPaymentError(int orderID, string message, string details)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "PaymentErrors");
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                string logFile = Path.Combine(logPath, $"payment_error_{DateTime.Now:yyyyMMdd}.txt");
                using (StreamWriter writer = new StreamWriter(logFile, true))
                {
                    writer.WriteLine("=============================================================================");
                    writer.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"OrderID: {orderID}");
                    writer.WriteLine($"Message: {message}");
                    writer.WriteLine($"Details: {details}");
                    writer.WriteLine("=============================================================================");
                    writer.WriteLine();
                }
            }
            catch
            {
                // Silent fail
            }
        }

        // ============================================================================
        // EXTRACT CHECKOUT DATA
        // ============================================================================

        private checkoutBLL ExtractCheckoutData()
        {
            try
            {
                string paymentType = Request.Form["payment_type"];

                var data = new checkoutBLL
                {
                    CustomerName = Request.Form["SenderName"].ToString().Trim(),
                    Email = Request.Form["SenderEmailAddress"].ToString().Trim(),
                    ContactNo = Request.Form["SenderMobileNumber"].ToString().Trim(),
                    Address = Request.Form["RecipientAddress"].ToString().Trim(),
                    City = Request.Form["City"].ToString().Trim(),
                    Country = Request.Form["Country"].ToString() ?? "Bahrain",
                    NearestPlace = Request.Form["NearestPlace"].ToString() ?? "",
                    PlaceType = Request.Form["PlaceType"].ToString(),
                    CardNotes = Request.Form["Notes"].ToString().Trim(),
                    DeliveryTime = Request.Form["DeliveryTime"].ToString() ?? "",
                    AmountTotal = ParseDouble(Request.Form["SubTotal"]),
                    DiscountAmount = ParseDouble(Request.Form["AmountDiscount"]),
                    Tax = ParseDouble(Request.Form["Tax"]),
                    DeliveryAmount = ParseDouble(Request.Form["DeliveryCharges"]),
                    GrandTotal = ParseDouble(Request.Form["GrandTotal"]),
                    PaymentMethodID = MapPaymentType(paymentType),
                    OrderDetailString = GetSessionDataString(Request.Form["SessionData"].ToString()),
                    CustomerID = HttpContext.Session.GetInt32("CustomerID") ?? 0
                };

                HttpContext.Items["GiftDataJson"] = Request.Form["GiftData"].ToString();

                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string GetSessionDataString(string sessionData)
        {
            System.Diagnostics.Debug.WriteLine($"GetSessionDataString received: '{sessionData}'");

            if (string.IsNullOrEmpty(sessionData) || sessionData.Trim() == "null" || sessionData.Trim() == "")
            {
                System.Diagnostics.Debug.WriteLine("Returning empty array for SessionData");
                return "[]";
            }

            var trimmed = sessionData.Trim();
            System.Diagnostics.Debug.WriteLine($"Returning SessionData: {trimmed}");
            return trimmed;
        }

        private void ParseOrderItems(checkoutBLL data)
        {
            if (string.IsNullOrEmpty(data.OrderDetailString) || data.OrderDetailString == "null" || data.OrderDetailString == "[]")
                return;

            try
            {
                var jsonArray = JArray.Parse(data.OrderDetailString);
                data.OrderDetail = jsonArray.ToObject<List<checkoutBLL.OrderDetails>>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ParseOrderItems Error: {ex.Message}");
            }
        }

        private ValidationResult ValidateCheckoutData(checkoutBLL data)
        {
            if (string.IsNullOrEmpty(data.CustomerName))
                return new ValidationResult { IsValid = false, Message = "Sender name is required" };
            if (string.IsNullOrEmpty(data.Email))
                return new ValidationResult { IsValid = false, Message = "Email is required" };
            if (string.IsNullOrEmpty(data.ContactNo))
                return new ValidationResult { IsValid = false, Message = "Contact number is required" };
            if (string.IsNullOrEmpty(data.Address))
                return new ValidationResult { IsValid = false, Message = "Delivery address is required" };
            if (string.IsNullOrEmpty(data.City))
                return new ValidationResult { IsValid = false, Message = "City is required" };
            if (string.IsNullOrEmpty(data.PlaceType))
                return new ValidationResult { IsValid = false, Message = "Place type is required" };
            if (data.PaymentMethodID == null || data.PaymentMethodID <= 0)
                return new ValidationResult { IsValid = false, Message = "Payment method is required" };
            if (data.GrandTotal <= 0)
                return new ValidationResult { IsValid = false, Message = "Order total must be greater than zero" };

            return new ValidationResult { IsValid = true };
        }

        private double ParseDouble(string value)
        {
            return string.IsNullOrEmpty(value) ? 0 : double.Parse(value);
        }

        private int MapPaymentType(string paymentTypeString)
        {
            if (string.IsNullOrEmpty(paymentTypeString))
                return 1;

            string cleanValue = paymentTypeString.Trim().ToLower();

            return cleanValue switch
            {
                "cash" => 1,
                "banktransfer" => 5,
                "benefitpay" => 3,
                "mastercard" => 2,
                _ => 1
            };
        }

        // ============================================================================
        // EMAIL METHODS
        // ============================================================================

        private void SendCompleteOrderEmails(int OrderID, checkoutBLL data, string giftDataJson = "[]")
        {
            try
            {
                string customerTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(
                    _webHostEnvironment.ContentRootPath, "Template", "emailpattern.txt"));
                string adminTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(
                    _webHostEnvironment.ContentRootPath, "Template", "emailpattern-admin.txt"));

                string itemsHtml = BuildItemsHtml(OrderID, data);
                string giftsHtml = BuildGiftsHtml(giftDataJson, data);

                customerTemplate = ReplaceEmailPlaceholders(customerTemplate, OrderID, data, itemsHtml, giftsHtml);
                adminTemplate = ReplaceEmailPlaceholders(adminTemplate, OrderID, data, itemsHtml, giftsHtml, isAdmin: true);

                SendEmail(_configuration["AppSettings:EmailSender"], data.Email,
                    $"Thank You For Order #{OrderID}", customerTemplate);

                SendEmail(_configuration["AppSettings:EmailSender"], _configuration["AppSettings:EmailReceivers"],
                    $"New Order #{OrderID}", adminTemplate);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendCompleteOrderEmails Error: {ex.Message}");
            }
        }

        private void SendPendingPaymentEmails(int OrderID, checkoutBLL data, string giftDataJson = "[]")
        {
            try
            {
                string customerTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(
                    _webHostEnvironment.ContentRootPath, "Template", "emailpattern.txt"));
                string adminTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(
                    _webHostEnvironment.ContentRootPath, "Template", "emailpattern-admin.txt"));

                string itemsHtml = BuildItemsHtml(OrderID, data);
                string giftsHtml = BuildGiftsHtml(giftDataJson, data);

                customerTemplate = ReplaceEmailPlaceholders(customerTemplate, OrderID, data, itemsHtml, giftsHtml);
                adminTemplate = ReplaceEmailPlaceholders(adminTemplate, OrderID, data, itemsHtml, giftsHtml, isAdmin: true);

                SendEmail(_configuration["AppSettings:EmailSender"], data.Email,
                    $"Order #{OrderID} - Payment Pending", customerTemplate);

                SendEmail(_configuration["AppSettings:EmailSender"], _configuration["AppSettings:EmailReceivers"],
                    $"New Order #{OrderID} - Payment Pending", adminTemplate);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendPendingPaymentEmails Error: {ex.Message}");
            }
        }

        private string ReplaceEmailPlaceholders(string template, int OrderID, checkoutBLL data,
            string itemsHtml, string giftsHtml, bool isAdmin = false)
        {
            template = template.Replace("#items#", itemsHtml);
            template = template.Replace("#gifts#", giftsHtml);
            template = template.Replace("#OrderNo#", OrderID.ToString());
            template = template.Replace("#ReceiverName#", data.CustomerName ?? "");
            template = template.Replace("#Customer#", data.CustomerName ?? "");
            template = template.Replace("#CustomerAddress#", data.Address ?? "");
            template = template.Replace("#CustomerContact#", data.ContactNo ?? "");
            template = template.Replace("#Contact#", data.ContactNo ?? "");
            template = template.Replace("#Address#", data.Address ?? "");
            template = template.Replace("#Description#", data.CardNotes ?? "");
            template = template.Replace("#OrderDate#", DateTime.Now.ToString("dd/MMM/yyyy"));
            template = template.Replace("#DeliveryDate#", "");
            template = template.Replace("#SelectedTime#", "");
            template = template.Replace("#PaymentType#", GetPaymentTypeName(data.PaymentMethodID) ?? "Cash");
            template = template.Replace("#PaymentMethod#", GetPaymentTypeName(data.PaymentMethodID) ?? "Cash");
            template = template.Replace("#TotalItems#", (data.OrderDetail?.Count ?? 0).ToString());
            template = template.Replace("#SubTotal#", (data.AmountTotal ?? 0).ToString("0.00"));
            template = template.Replace("#Discount#", (data.DiscountAmount ?? 0).ToString("0.00"));
            template = template.Replace("#Tax#", (data.Tax ?? 0).ToString("0.00"));
            template = template.Replace("#DeliveryAmount#", (data.DeliveryAmount ?? 0).ToString("0.00"));
            template = template.Replace("#GrandTotal#", (data.GrandTotal ?? 0).ToString("0.00"));
            return template;
        }

        public class EmailOrderItem
        {
            public int id { get; set; }
            public string title { get; set; }
            public string image { get; set; }
            public int Qty { get; set; }
            public double? Price { get; set; }
            public double? NewPrice { get; set; }
            public string gifts { get; set; }
        }

        private string BuildItemsHtml(int OrderID, checkoutBLL data)
        {
            StringBuilder items = new StringBuilder();

            if (string.IsNullOrEmpty(data?.OrderDetailString))
                return "";

            var orderItems = JsonConvert.DeserializeObject<List<EmailOrderItem>>(data.OrderDetailString);
            if (orderItems == null || orderItems.Count == 0)
                return "";

            string baseUrl = _configuration["WebsiteImageURL"] ?? "";

            foreach (var item in orderItems)
            {
                string imageUrl = "";
                if (!string.IsNullOrEmpty(item.image))
                {
                    imageUrl = item.image.StartsWith("http")
                        ? item.image
                        : baseUrl.TrimEnd('/') + "/" + item.image.TrimStart('/');
                }

                items.Append("<table width='100%' style='padding:20px;'><tr>");
                items.Append("<td width='120' valign='top'>");
                if (!string.IsNullOrEmpty(imageUrl))
                    items.Append($"<img src='{imageUrl}' width='100' style='border-radius:6px;' />");
                items.Append("</td>");
                items.Append("<td valign='top'>");
                items.Append($"<div><strong>{item.title}</strong></div>");
                items.Append($"<div>Qty: {item.Qty}</div>");
                items.Append($"<div><strong>BHD {item.Price:0.000}</strong></div>");
                items.Append("</td></tr></table>");
            }

            return items.ToString();
        }

        private string BuildGiftsHtml(string giftDataJson, checkoutBLL data)
        {
            try
            {
                if (string.IsNullOrEmpty(giftDataJson) || giftDataJson == "[]")
                    return "";

                var gifts = JsonConvert.DeserializeObject<List<dynamic>>(giftDataJson);
                if (gifts == null || gifts.Count == 0)
                    return "";

                string baseUrl = _configuration["WebsiteImageURL"] ?? "";
                StringBuilder giftsHtml = new StringBuilder();

                giftsHtml.Append("<div style='margin-top:20px; padding:15px; background:#f9f9f9; border-radius:4px;'>");
                giftsHtml.Append("<h3 style='margin-top:0; color:#333;'>Addon Products / Gifts</h3>");

                foreach (var gift in gifts)
                {
                    string title = gift["Title"] ?? "";
                    double price = gift["DisplayPrice"] ?? 0;
                    int qty = gift["Qty"] ?? 1;
                    string image = gift["Image"] ?? "";
                    double total = qty * price;

                    string imageUrl = "";
                    if (!string.IsNullOrEmpty((string)image))
                    {
                        imageUrl = ((string)image).StartsWith("http")
                            ? image
                            : baseUrl.TrimEnd('/') + "/" + ((string)image).TrimStart('/');
                    }

                    giftsHtml.Append("<table width='100%' style='margin-bottom:15px;'><tr>");
                    giftsHtml.Append("<td width='100' valign='top' style='padding-right:10px;'>");
                    if (!string.IsNullOrEmpty(imageUrl))
                        giftsHtml.Append($"<img src='{imageUrl}' width='80' style='border-radius:6px;' />");
                    giftsHtml.Append("</td>");
                    giftsHtml.Append("<td valign='top'>");
                    giftsHtml.Append($"<div style='font-weight:bold;'>{title}</div>");
                    giftsHtml.Append($"<div>Qty: {qty}</div>");
                    giftsHtml.Append($"<div><strong>BHD {total:0.000}</strong></div>");
                    giftsHtml.Append("</td></tr></table>");
                }

                giftsHtml.Append("</div>");
                return giftsHtml.ToString();
            }
            catch
            {
                return "";
            }
        }

        private string GetPaymentTypeName(int? paymentMethodID)
        {
            return paymentMethodID switch
            {
                1 => "Cash on Delivery",
                2 => "Mastercard",
                3 => "BenefitPay",
                4 => "Mastercard",
                5 => "Bank Transfer",
                _ => "Cash"
            };
        }

        private void SendOrderEmails(int OrderID, checkoutBLL data)
        {
            SendCompleteOrderEmails(OrderID, data, "[]");
        }

        private void SendEmail(string from, string to, string subject, string body)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(from);
                    mail.To.Add(to);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential(
                            _configuration["AppSettings:FromAddress"],
                            _configuration["AppSettings:EmailSenderPassword"]
                        );
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendEmail Error: {ex.Message}");
            }
        }

        // ============================================================================
        // ORDER COMPLETE PAGE
        // ============================================================================

        public IActionResult OrderComplete(int OrderID = 0, string OrderNo = "")
        {
            try
            {
                int UserId = Convert.ToInt32(_configuration["UserId"]);
                var settng = new settingBLL().GetSettings(UserId);
                if (settng?.DynamicList?.Count > 0)
                {
                    ViewBag.Logo = settng.DynamicList[0].Logo;
                    ViewBag.TopheaderArea = settng.DynamicList[0].HeaderToparea;
                    ViewBag.AddButton = settng.DynamicList[0].AddButton;
                }

                checkoutBLL check = new checkoutBLL();
                if (OrderNo == "Reject" || OrderID == 0)
                {
                    ViewBag.OrderNo = "Reject";
                }
                else
                {
                    var data = new myorderBLL().GetDetails(OrderID);
                    if (data != null && data.PaymentMethodTitle == "DebitCreditCard")
                    {
                        check.OrderUpdate(OrderID, 101);
                    }
                    ViewBag.OrderNo = data?.OrderNo ?? "Unknown";
                }
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.OrderNo = "Error";
                return View();
            }
        }

        [HttpGet]
        public JsonResult Coupon(string coupon)
        {
            try
            {
                couponBLL couponData = new couponBLL();
                var result = couponData.Get(coupon);
                return Json(new { data = result, error = "" });
            }
            catch (Exception ex)
            {
                return Json(new { data = (object)null, error = ex.Message });
            }
        }

        public IActionResult MyOrders()
        {
            try
            {
                ViewBag.ImageUrl = _configuration["Image"];

                var sessionCustomerId = HttpContext.Session.GetInt32("CustomerID");
                if (sessionCustomerId != null && sessionCustomerId != 0)
                {
                    return View(new myorderBLL().GetAll(sessionCustomerId.Value));
                }
                else
                {
                    return RedirectToAction("Login_Register", "Account");
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        public IActionResult OrderDetails(int OrderID)
        {
            try
            {
                ViewBag.ImageUrl = _configuration["Image"];
                int UserId = Convert.ToInt32(_configuration["UserId"]);

                var settng = new settingBLL().GetSettings(UserId);
                if (settng?.DynamicList?.Count > 0)
                {
                    ViewBag.Logo = settng.DynamicList[0].Logo;
                    ViewBag.TopheaderArea = settng.DynamicList[0].HeaderToparea;
                    ViewBag.AddButton = settng.DynamicList[0].AddButton;
                }
                return View(new myorderBLL().GetDetails(OrderID));
            }
            catch (Exception ex)
            {
                return View();
            }
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}