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
            
            // Initialize BenefitPayGatewayService with configuration
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
                //ViewBag.Banner = new bannerBLL().GetBanner("Cart");
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

                //ViewBag.Banner = new bannerBLL().GetBanner("Checkout");

                int CustomerID = id;
                if (CustomerID == 0)
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

                // Parse OrderDetail from string
                if (!string.IsNullOrEmpty(data.OrderDetailString))
                {
                    string json = JsonConvert.SerializeObject(JArray.Parse(data.OrderDetailString));
                    JArray jsonResponse = JArray.Parse(json);
                    data.OrderDetail = jsonResponse.ToObject<List<checkoutBLL.OrderDetails>>();
                }

                // Insert Order
                rtn = _service.InsertOrder(data);

                if (rtn > 0)
                {
                    // Send Emails
                    SendOrderEmails(rtn, data);
                }

                // Return based on payment type
                if (data.PaymentMethodID == 1) // Cash
                {
                    return Json(new { data = rtn });
                }
                else // Other payment methods
                {
                    return Json(new { data = "DebitCreditCard", OrderID = rtn });
                }
            }
            catch (Exception ex)
            {
                return Json(new { data = 0, error = ex.Message });
            }
        }

        [HttpPost]
        [Route("Order/PlaceOrder")]
        public async Task<JsonResult> PlaceOrder()
        {
            try
            {
                // Build checkout data from form submission
                var checkoutData = ExtractCheckoutData();
                
                if (checkoutData == null)
                {
                    return Json(new { Success = false, Message = "Invalid order data" });
                }

                // Validate required fields
                var validationResult = ValidateCheckoutData(checkoutData);
                if (!validationResult.IsValid)
                {
                    return Json(new { Success = false, Message = validationResult.Message });
                }

                // Parse order items from SessionData
                ParseOrderItems(checkoutData);

                if (checkoutData.OrderDetail == null || checkoutData.OrderDetail.Count == 0)
                {
                    return Json(new { Success = false, Message = "No items in order" });
                }

                // Insert order to database
                var checkoutService = new checkoutBLL();
                if (checkoutData.PaymentMethodID == 1 || checkoutData.PaymentMethodID == 5)
                {
                    checkoutData.DeliveryStatus = 101;
                }
                else if (checkoutData.PaymentMethodID == 2 || checkoutData.PaymentMethodID == 3) 
                {
                    checkoutData.DeliveryStatus = 104;
                }

                int OrderID = checkoutService.InsertOrder(checkoutData);

                if (OrderID <= 0)
                {
                    return Json(new { Success = false, Message = "Failed to create order" });
                }

                // Determine payment type and handle accordingly
                int paymentType = checkoutData.PaymentMethodID ?? 1;

                switch (paymentType)
                {
                    case 1: // Cash
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

                    case 2: // Credimax (Mastercard Gateway)
                        return await HandleCredimaxPayment(OrderID, checkoutData);

                    case 3: // BenefitPay
                        return await HandleBenefitPayment(OrderID, checkoutData);

                    case 4: // Mastercard Direct
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

                    case 5: // Bank Transfer
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

        private async Task<JsonResult> HandleCredimaxPayment(int OrderID, checkoutBLL data)
        {
            try
            {
                var giftDataJson = HttpContext.Items["GiftDataJson"]?.ToString() ?? "[]";
                SendPendingPaymentEmails(OrderID, data, giftDataJson);

                string sessionUrl = "https://credimax.gateway.mastercard.com/api/rest/version/60/merchant/E10561950/session";
                string credentials = "bWVyY2hhbnQuRTEwNTYxOTUwOjhhYTlhZmI5OTg0ODZhMjA0ZjI0ODY0YzIyOTY1OGNh";
                string baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7051";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
                    
                    var payload = new
                    {
                        apiOperation = "CREATE_CHECKOUT_SESSION",
                        order = new
                        {
                            amount = (data.GrandTotal ?? 0).ToString("0.00"),
                            currency = "BHD",
                            id = OrderID.ToString(),
                            description = "Unique, handmade décor and gifts to bring warmth and personality into your home.",
                            reference = $"Order#{OrderID}"
                        },
                        interaction = new
                        {
                            operation = "PURCHASE",
                            returnUrl = $"{baseUrl}/Order/PaymentCallback?OrderID={OrderID}&status=success",
                            cancelUrl = $"{baseUrl}/Order/PaymentCallback?OrderID={OrderID}&status=cancel",
                            merchant = new
                            {
                                name = "HANDCRAFTED HEAVEN TRADING BY SAMIA WLL",
                            }
                        }
                    };
                    
                    var json = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    System.Diagnostics.Debug.WriteLine($"Credimax Payload: {json}");
                    
                    var content = new StringContent(json, Encoding.UTF8, "text/plain");

                    var response = await client.PostAsync(sessionUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        dynamic responseData = JsonConvert.DeserializeObject(responseContent);
                        string sessionID = responseData["session"]["id"];

                        return Json(new
                        {
                            Success = true,
                            orderid = OrderID,
                            orderno = OrderID.ToString(),
                            sessionID = sessionID,
                            redirectUrl = $"https://credimax.gateway.mastercard.com/checkout/pay/{sessionID}?checkoutVersion=1.0.0"
                        });
                    }
                    else
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"Credimax Error: {responseContent}");
                        return Json(new
                        {
                            Success = false,
                            Message = $"Payment session creation failed: {responseContent}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HandleCredimaxPayment Exception: {ex.Message}");
                return Json(new { Success = false, Message = $"Payment error: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult PaymentCallback(int OrderID = 0, string status = "")
        {
            try
            {
                if (status == "success" && OrderID > 0)
                {
                    // Payment approved
                    var checkoutService = new checkoutBLL();
                    checkoutService.OrderUpdate(OrderID, 101); // Update status to completed
                    
                    return RedirectToAction("OrderComplete", new { OrderID = OrderID });
                }
                else
                {
                    // Payment cancelled or failed
                    return RedirectToAction("OrderComplete", new { OrderID = OrderID, OrderNo = "Reject" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("OrderComplete", new { OrderNo = "Reject" });
            }
        }

        // ============================================================================
        // BENEFITPAY INTEGRATION - NEW CODE
        // ============================================================================
        
        /// <summary>
        /// Handle BenefitPay payment initialization using BenefitGatewayService
        /// </summary>
        private async Task<JsonResult> HandleBenefitPayment(int OrderID, checkoutBLL data)
        {
            try
            {
                // Send pending payment email to admin
                var giftDataJson = HttpContext.Items["GiftDataJson"]?.ToString() ?? "[]";
                SendPendingPaymentEmails(OrderID, data, giftDataJson);

                // Initiate BenefitPay payment using the new BenefitPayGatewayService
                // Parameters: trackId (OrderID), amount, currency (048=BHD), cardType (D=Default), udf fields
                var result = _benefitPayGatewayService.InitiatePayment(
                    trackId: OrderID.ToString(),
                    amount: (decimal)(data.GrandTotal ?? 0),
                    currency: "048",  // BHD (Bahraini Dinar)
                    cardType: "D",     // Default card type
                    udf1: OrderID.ToString(),           // Order ID in UDF1
                    udf2: (data.CustomerID?.ToString() ?? ""),  // Customer ID in UDF2
                    udf3: (data.CustomerName ?? ""),    // Customer name in UDF3
                    udf4: (data.Email ?? ""),   // Customer email in UDF4
                    udf5: (data.GrandTotal?.ToString("F2") ?? "")  // Amount in UDF5
                );

                if (result.IsSuccessful)
                {
                    // Success - return redirect URL to payment gateway
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
                    // Failed to initialize payment
                    return Json(new
                    {
                        Success = false,
                        Message = $"BenefitPay initialization failed: {result.ErrorMessage}"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = $"BenefitPay error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// BenefitPay Response Handler - Called by BenefitPay gateway after payment
        /// IMPORTANT: Must return "REDIRECT=" for BenefitPay
        /// </summary>
        [HttpPost]
        [HttpGet]
        public IActionResult BenefitPayResponse()
        {
            try
            {
                int orderID = 0;

                // Get OrderID from query string
                if (Request.Query.ContainsKey("OrderID"))
                {
                    int.TryParse(Request.Query["OrderID"], out orderID);
                }

                // Handle GET request with OrderID=0 (error/cancel)
                if (Request.Method == "GET" && orderID == 0)
                {
                    string baseUrl = _configuration["AppSettings:WebsiteURL"] ?? $"{Request.Scheme}://{Request.Host}";
                    string cancelUrl = $"{baseUrl}/Order/OrderComplete?OrderNo=Reject";
                    return Content($"REDIRECT={cancelUrl}");
                }

                // Handle POST request (response from BenefitPay)
                if (Request.Method == "POST")
                {
                    // Get encrypted response from BenefitPay
                    string encryptedResponse = "";
                    if (Request.Form.ContainsKey("trandata"))
                    {
                        encryptedResponse = Request.Form["trandata"].ToString();
                    }

                    // Process payment response using BenefitGatewayService
                    var paymentRequest = new PaymentResponseRequest
                    {
                        OrderID = orderID,
                        EncryptedResponse = encryptedResponse
                    };

                    var paymentResult = _benefitGatewayService.ProcessPaymentResponse(paymentRequest);

                    string baseUrl = _configuration["AppSettings:WebsiteURL"] ?? $"{Request.Scheme}://{Request.Host}";

                    // Check payment result
                    if (paymentResult.IsSuccessful && paymentResult.Result == "CAPTURED")
                    {
                        // Payment successful
                        string successUrl = $"{baseUrl}/Order/BenefitPayApproved?OrderID={paymentResult.OrderID}";
                        return Content($"REDIRECT={successUrl}");
                    }
                    else
                    {
                        // Payment failed
                        string userMessage = paymentResult.ErrorMessage ?? "Payment processing failed";
                        
                        // Log error
                        LogPaymentError(paymentResult.OrderID, paymentResult.Result ?? "UNKNOWN", paymentResult.ErrorMessage ?? "");
                        
                        string failUrl = $"{baseUrl}/Order/OrderComplete?OrderNo=Reject&error={Uri.EscapeDataString(userMessage)}";
                        return Content($"REDIRECT={failUrl}");
                    }
                }

                // Fallback
                string fallbackUrl = _configuration["AppSettings:WebsiteURL"] ?? $"{Request.Scheme}://{Request.Host}";
                return Content($"REDIRECT={fallbackUrl}/Order/OrderComplete?OrderNo=Reject");
            }
            catch (Exception ex)
            {
                LogPaymentError(0, "BenefitPay Response Exception", ex.Message);
                
                string errorUrl = _configuration["AppSettings:WebsiteURL"] ?? $"{Request.Scheme}://{Request.Host}";
                return Content($"REDIRECT={errorUrl}/Order/OrderComplete?OrderNo=Reject");
            }
        }

        /// <summary>
        /// BenefitPay Approval Page - Final confirmation after successful payment
        /// </summary>
        [HttpGet]
        public IActionResult BenefitPayApproved(int OrderID)
        {
            try
            {
                if (OrderID <= 0)
                {
                    return RedirectToAction("OrderComplete", new { OrderNo = "Reject" });
                }

                // Update order status to approved (101)
                var checkoutService = new checkoutBLL();
                checkoutService.OrderUpdate(OrderID, 101);

                // Get order details and send confirmation email
                try
                {
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
                }
                catch (Exception emailEx)
                {
                    // Log but don't fail
                    LogPaymentError(OrderID, "Email sending failed", emailEx.Message);
                }

                // Redirect to thank you page
                return RedirectToAction("OrderComplete", new { OrderID = OrderID });
            }
            catch (Exception ex)
            {
                LogPaymentError(OrderID, "BenefitPay Approval Failed", ex.Message);
                return RedirectToAction("OrderComplete", new { OrderNo = "Reject" });
            }
        }

        /// <summary>
        /// Get user-friendly error message for BenefitPay response codes
        /// </summary>
        private string GetBenefitPayErrorMessage(string result, string responseCode)
        {
            if (result == "NOT CAPTURED")
            {
                return responseCode switch
                {
                    "05" => "Please contact your card issuer",
                    "14" => "Invalid card number",
                    "33" => "Expired card",
                    "36" => "Restricted card",
                    "38" => "Allowable PIN tries exceeded",
                    "51" => "Insufficient funds",
                    "54" => "Expired card",
                    "55" => "Incorrect PIN",
                    "61" => "Exceeds withdrawal amount limit",
                    "62" => "Restricted card",
                    "65" => "Exceeds withdrawal frequency limit",
                    "75" => "Allowable PIN tries exceeded",
                    "76" => "Ineligible account",
                    "78" => "Please contact your card issuer",
                    "91" => "Card issuer is inoperative",
                    _ => "Unable to process payment. Please try again or use another card."
                };
            }
            else if (result == "CANCELED")
            {
                return "Payment was cancelled";
            }
            else if (result == "DENIED BY RISK")
            {
                return "Maximum transaction limit exceeded";
            }
            else if (result == "HOST TIMEOUT")
            {
                return "Payment gateway timeout. Please try again";
            }

            return "Payment could not be processed. Please try again";
        }

        /// <summary>
        /// Log payment errors to file
        /// </summary>
        private void LogPaymentError(int orderID, string message, string details)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "PaymentErrors");

                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

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
                // Silent fail - don't crash on logging errors
            }
        }

        // ============================================================================
        // END OF BENEFITPAY INTEGRATION
        // ============================================================================

        private checkoutBLL ExtractCheckoutData()
        {
            try
            {
                // Get payment type from form - use null coalescing to handle missing value
                string paymentType = Request.Form["payment_type"];
                
                var data = new checkoutBLL
                {
                    // Sender Information
                    CustomerName = Request.Form["SenderName"].ToString().Trim(),
                    Email = Request.Form["SenderEmailAddress"].ToString().Trim(),
                    ContactNo = Request.Form["SenderMobileNumber"].ToString().Trim(),

                    // Recipient/Delivery Information
                    Address = Request.Form["RecipientAddress"].ToString().Trim(),
                    City = Request.Form["City"].ToString().Trim(),
                    Country = Request.Form["Country"].ToString() ?? "Bahrain",
                    NearestPlace = Request.Form["NearestPlace"].ToString() ?? "",
                    PlaceType = Request.Form["PlaceType"].ToString(),
                    CardNotes = Request.Form["Notes"].ToString().Trim(),
                    DeliveryTime = Request.Form["DeliveryTime"].ToString() ?? "",

                    // Financial Information
                    AmountTotal = ParseDouble(Request.Form["SubTotal"]),
                    DiscountAmount = ParseDouble(Request.Form["AmountDiscount"]),
                    Tax = ParseDouble(Request.Form["Tax"]),
                    DeliveryAmount = ParseDouble(Request.Form["DeliveryCharges"]),
                    GrandTotal = ParseDouble(Request.Form["GrandTotal"]),

                    // Payment Information - Explicitly map the payment type
                    PaymentMethodID = MapPaymentType(paymentType),
                    
                    // Order Items (as JSON string) - handle null/empty cases
                    OrderDetailString = GetSessionDataString(Request.Form["SessionData"].ToString()),

                    // Customer ID from session
                    CustomerID = HttpContext.Session.GetInt32("CustomerID") ?? 0
                };

                // Store GiftData for later use
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
            // Log what we received
            System.Diagnostics.Debug.WriteLine($"GetSessionDataString received: '{sessionData}'");
            
            // Handle null, empty, or "null" string cases
            if (string.IsNullOrEmpty(sessionData) || sessionData.Trim() == "null" || sessionData.Trim() == "")
            {
                System.Diagnostics.Debug.WriteLine("Returning empty array for SessionData");
                return "[]"; // Return empty JSON array as default
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
                // Parsing failed, leave OrderDetail empty
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
                return 1; // Default to Cash

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

        private void SendCompleteOrderEmails(int OrderID, checkoutBLL data, string giftDataJson = "[]")
        {
            try
            {
                // Load templates from files
                string customerTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(
                    _webHostEnvironment.ContentRootPath, "Template", "emailpattern.txt"));
                string adminTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(
                    _webHostEnvironment.ContentRootPath, "Template", "emailpattern-admin.txt"));

                // Build items HTML
                string itemsHtml = BuildItemsHtml(OrderID, data);
                
                // Build gifts HTML
                string giftsHtml = BuildGiftsHtml(giftDataJson, data);

                // Replace placeholders in customer email
                customerTemplate = customerTemplate.Replace("#items#", itemsHtml);
                customerTemplate = customerTemplate.Replace("#gifts#", giftsHtml);
                customerTemplate = customerTemplate.Replace("#OrderNo#", OrderID.ToString());
                customerTemplate = customerTemplate.Replace("#ReceiverName#", data.CustomerName ?? "");
                customerTemplate = customerTemplate.Replace("#Customer#", data.CustomerName ?? "");
                customerTemplate = customerTemplate.Replace("#CustomerAddress#", data.Address ?? "");
                customerTemplate = customerTemplate.Replace("#Address#", data.Address ?? "");
                customerTemplate = customerTemplate.Replace("#Contact#", data.ContactNo ?? "");
                customerTemplate = customerTemplate.Replace("#ReceiverContact#", data.ContactNo ?? "");
                customerTemplate = customerTemplate.Replace("#Description#", data.CardNotes ?? "");
                customerTemplate = customerTemplate.Replace("#OrderDate#", DateTime.Now.ToString("dd/MMM/yyyy"));
                customerTemplate = customerTemplate.Replace("#DeliveryDate#", "");
                customerTemplate = customerTemplate.Replace("#SelectedTime#", "");
                customerTemplate = customerTemplate.Replace("#PaymentType#", GetPaymentTypeName(data.PaymentMethodID) ?? "Cash");
                customerTemplate = customerTemplate.Replace("#PaymentMethod#", GetPaymentTypeName(data.PaymentMethodID) ?? "Cash");
                customerTemplate = customerTemplate.Replace("#TotalItems#", (data.OrderDetail?.Count ?? 0).ToString());
                customerTemplate = customerTemplate.Replace("#SubTotal#", (data.AmountTotal ?? 0).ToString("0.00"));
                customerTemplate = customerTemplate.Replace("#Discount#", (data.DiscountAmount ?? 0).ToString("0.00"));
                customerTemplate = customerTemplate.Replace("#Tax#", (data.Tax ?? 0).ToString("0.00"));
                customerTemplate = customerTemplate.Replace("#DeliveryAmount#", (data.DeliveryAmount ?? 0).ToString("0.00"));
                customerTemplate = customerTemplate.Replace("#GrandTotal#", (data.GrandTotal ?? 0).ToString("0.00"));

                // Replace placeholders in admin email
                adminTemplate = adminTemplate.Replace("#items#", itemsHtml);
                adminTemplate = adminTemplate.Replace("#gifts#", giftsHtml);
                adminTemplate = adminTemplate.Replace("#OrderNo#", OrderID.ToString());
                adminTemplate = adminTemplate.Replace("#ReceiverName#", data.CustomerName ?? "");
                adminTemplate = adminTemplate.Replace("#Customer#", data.CustomerName ?? "");
                adminTemplate = adminTemplate.Replace("#CustomerAddress#", data.Address ?? "");
                adminTemplate = adminTemplate.Replace("#CustomerContact#", data.ContactNo ?? "");
                adminTemplate = adminTemplate.Replace("#Contact#", data.ContactNo ?? "");
                adminTemplate = adminTemplate.Replace("#Address#", data.Address ?? "");
                adminTemplate = adminTemplate.Replace("#Description#", data.CardNotes ?? "");
                adminTemplate = adminTemplate.Replace("#OrderDate#", DateTime.Now.ToString("dd/MMM/yyyy"));
                adminTemplate = adminTemplate.Replace("#DeliveryDate#", "");
                adminTemplate = adminTemplate.Replace("#SelectedTime#", "");
                adminTemplate = adminTemplate.Replace("#PaymentType#", GetPaymentTypeName(data.PaymentMethodID) ?? "Cash");
                adminTemplate = adminTemplate.Replace("#PaymentMethod#", GetPaymentTypeName(data.PaymentMethodID) ?? "Cash");
                adminTemplate = adminTemplate.Replace("#Discount#", (data.DiscountAmount ?? 0).ToString("0.00"));
                adminTemplate = adminTemplate.Replace("#TotalItems#", (data.OrderDetail?.Count ?? 0).ToString());
                adminTemplate = adminTemplate.Replace("#SubTotal#", (data.AmountTotal ?? 0).ToString("0.00"));
                adminTemplate = adminTemplate.Replace("#Tax#", (data.Tax ?? 0).ToString("0.00"));
                adminTemplate = adminTemplate.Replace("#DeliveryAmount#", (data.DeliveryAmount ?? 0).ToString("0.00"));
                adminTemplate = adminTemplate.Replace("#GrandTotal#", (data.GrandTotal ?? 0).ToString("0.00"));

                // Send customer email
                SendEmail(
                    _configuration["AppSettings:EmailSender"],
                    data.Email,
                    $"Thank You For Order #{OrderID}",
                    customerTemplate
                );

                // Send admin email
                SendEmail(
                    _configuration["AppSettings:EmailSender"],
                    _configuration["AppSettings:EmailReceivers"],
                    $"New Order #{OrderID}",
                    adminTemplate
                );
            }
            catch (Exception ex)
            {
                // Log silently
            }
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

            var orderItems = Newtonsoft.Json.JsonConvert
                .DeserializeObject<List<EmailOrderItem>>(data.OrderDetailString);

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

                items.Append("<table width='100%' style='padding:20px;'>");
                items.Append("<tr>");

                // IMAGE
                items.Append("<td width='120' valign='top'>");
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    items.Append($"<img src='{imageUrl}' width='100' style='border-radius:6px;' />");
                }
                items.Append("</td>");

                // DETAILS
                items.Append("<td valign='top'>");
                items.Append($"<div><strong>{item.title}</strong></div>");
                items.Append($"<div>Qty: {item.Qty}</div>");
                items.Append($"<div><strong>BHD {item.Price:0.000}</strong></div>");
                items.Append("</td>");

                items.Append("</tr>");
                items.Append("</table>");
            }

            return items.ToString();
        }


        private string BuildAdminEmail(int OrderID, checkoutBLL data)
        {
            if (data?.OrderDetail == null || data.OrderDetail.Count == 0)
                return string.Empty;

            StringBuilder items = new StringBuilder();
            items.Append("<table style='width:100%; border-collapse:collapse;'>");
            items.Append("<tr style='background:#f5f5f5;'>");
            items.Append("<th style='border:1px solid #ddd;padding:8px;text-align:left;'>Product</th>");
            items.Append("<th style='border:1px solid #ddd;padding:8px;text-align:center;'>Qty</th>");
            items.Append("<th style='border:1px solid #ddd;padding:8px;text-align:right;'>Price</th>");
            items.Append("<th style='border:1px solid #ddd;padding:8px;text-align:right;'>Total</th>");
            items.Append("</tr>");

            string baseUrl = _configuration["WebsiteImageURL"] ?? "";

            foreach (var item in data.OrderDetail)
            {
                double itemTotal = item.Qty * (item.Price ?? 0);
                string imageUrl = "";
                if (!string.IsNullOrEmpty(item.Image))
                {
                    imageUrl = item.Image.StartsWith("http")
                        ? item.Image
                        : baseUrl.TrimEnd('/') + "/" + item.Image.TrimStart('/');
                }

                items.Append("<tr>");
                items.Append("<td style='border:1px solid #ddd;padding:8px;'>");

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    items.Append($"<img src='{imageUrl}' width='50' style='vertical-align:middle; margin-right:5px;' />");
                }

                items.Append($"{item.Name}</td>");
                items.Append($"<td style='border:1px solid #ddd;padding:8px;text-align:center;'>{item.Qty}</td>");
                items.Append($"<td style='border:1px solid #ddd;padding:8px;text-align:right;'>BHD {item.Price:0.000}</td>");
                items.Append($"<td style='border:1px solid #ddd;padding:8px;text-align:right;'>BHD {itemTotal:0.000}</td>");
                items.Append("</tr>");
            }

            items.Append("</table>");
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
                    if (!string.IsNullOrEmpty(image))
                    {
                        imageUrl = image.StartsWith("http")
                            ? image
                            : baseUrl.TrimEnd('/') + "/" + image.TrimStart('/');
                    }

                    giftsHtml.Append("<table width='100%' style='border-collapse:collapse; margin-bottom:15px;'>");
                    giftsHtml.Append("<tr>");

                    // IMAGE
                    giftsHtml.Append("<td width='100' valign='top' style='padding-right:10px;'>");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        giftsHtml.Append($"<img src='{imageUrl}' width='80' style='border-radius:6px;' />");
                    }
                    giftsHtml.Append("</td>");

                    // DETAILS
                    giftsHtml.Append("<td valign='top'>");
                    giftsHtml.Append($"<div style='font-weight:bold;'>{title}</div>");
                    giftsHtml.Append($"<div>Qty: {qty}</div>");
                    giftsHtml.Append($"<div><strong>BHD {total:0.000}</strong></div>");
                    giftsHtml.Append("</td>");

                    giftsHtml.Append("</tr>");
                    giftsHtml.Append("</table>");
                }

                giftsHtml.Append("</div>");
                return giftsHtml.ToString();
            }
            catch
            {
                return "";
            }
        }



        private void SendPendingPaymentEmails(int OrderID, checkoutBLL data, string giftDataJson = "[]")
        {
            try
            {
                // Load templates from files
                string customerTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(
                    _webHostEnvironment.ContentRootPath, "Template", "emailpattern.txt"));
                string adminTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(
                    _webHostEnvironment.ContentRootPath, "Template", "emailpattern-admin.txt"));

                // Build items HTML
                string itemsHtml = BuildItemsHtml(OrderID, data);
                
                // Build gifts HTML
                string giftsHtml = BuildGiftsHtml(giftDataJson, data);

                // Replace placeholders in customer email
                customerTemplate = customerTemplate.Replace("#items#", itemsHtml);
                customerTemplate = customerTemplate.Replace("#gifts#", giftsHtml);
                customerTemplate = customerTemplate.Replace("#OrderNo#", OrderID.ToString());
                customerTemplate = customerTemplate.Replace("#ReceiverName#", data.CustomerName ?? "");
                customerTemplate = customerTemplate.Replace("#Customer#", data.CustomerName ?? "");
                customerTemplate = customerTemplate.Replace("#CustomerAddress#", data.Address ?? "");
                customerTemplate = customerTemplate.Replace("#Address#", data.Address ?? "");
                customerTemplate = customerTemplate.Replace("#Contact#", data.ContactNo ?? "");
                customerTemplate = customerTemplate.Replace("#ReceiverContact#", data.ContactNo ?? "");
                customerTemplate = customerTemplate.Replace("#Description#", data.CardNotes ?? "");
                customerTemplate = customerTemplate.Replace("#OrderDate#", DateTime.Now.ToString("dd/MMM/yyyy"));
                customerTemplate = customerTemplate.Replace("#DeliveryDate#", "");
                customerTemplate = customerTemplate.Replace("#SelectedTime#", "");
                customerTemplate = customerTemplate.Replace("#PaymentType#", GetPaymentTypeName(data.PaymentMethodID) ?? "Cash");
                customerTemplate = customerTemplate.Replace("#PaymentMethod#", GetPaymentTypeName(data.PaymentMethodID) ?? "Cash");
                customerTemplate = customerTemplate.Replace("#TotalItems#", (data.OrderDetail?.Count ?? 0).ToString());
                customerTemplate = customerTemplate.Replace("#SubTotal#", (data.AmountTotal ?? 0).ToString("0.00"));
                customerTemplate = customerTemplate.Replace("#Discount#", (data.DiscountAmount ?? 0).ToString("0.00"));
                customerTemplate = customerTemplate.Replace("#Tax#", (data.Tax ?? 0).ToString("0.00"));
                customerTemplate = customerTemplate.Replace("#DeliveryAmount#", (data.DeliveryAmount ?? 0).ToString("0.00"));
                customerTemplate = customerTemplate.Replace("#GrandTotal#", (data.GrandTotal ?? 0).ToString("0.00"));

                // Replace placeholders in admin email
                adminTemplate = adminTemplate.Replace("#items#", itemsHtml);
                adminTemplate = adminTemplate.Replace("#gifts#", giftsHtml);
                adminTemplate = adminTemplate.Replace("#OrderNo#", OrderID.ToString());
                adminTemplate = adminTemplate.Replace("#ReceiverName#", data.CustomerName ?? "");
                adminTemplate = adminTemplate.Replace("#Customer#", data.CustomerName ?? "");
                adminTemplate = adminTemplate.Replace("#CustomerAddress#", data.Address ?? "");
                adminTemplate = adminTemplate.Replace("#CustomerContact#", data.ContactNo ?? "");
                adminTemplate = adminTemplate.Replace("#Contact#", data.ContactNo ?? "");
                adminTemplate = adminTemplate.Replace("#Address#", data.Address ?? "");
                adminTemplate = adminTemplate.Replace("#Description#", data.CardNotes ?? "");
                adminTemplate = adminTemplate.Replace("#OrderDate#", DateTime.Now.ToString("dd/MMM/yyyy"));
                adminTemplate = adminTemplate.Replace("#DeliveryDate#", "");
                adminTemplate = adminTemplate.Replace("#SelectedTime#", "");
                adminTemplate = adminTemplate.Replace("#PaymentType#", GetPaymentTypeName(data.PaymentMethodID) ?? "Cash");
                adminTemplate = adminTemplate.Replace("#PaymentMethod#", GetPaymentTypeName(data.PaymentMethodID) ?? "Cash");
                adminTemplate = adminTemplate.Replace("#Discount#", (data.DiscountAmount ?? 0).ToString("0.00"));
                adminTemplate = adminTemplate.Replace("#TotalItems#", (data.OrderDetail?.Count ?? 0).ToString());
                adminTemplate = adminTemplate.Replace("#SubTotal#", (data.AmountTotal ?? 0).ToString("0.00"));
                adminTemplate = adminTemplate.Replace("#Tax#", (data.Tax ?? 0).ToString("0.00"));
                adminTemplate = adminTemplate.Replace("#DeliveryAmount#", (data.DeliveryAmount ?? 0).ToString("0.00"));
                adminTemplate = adminTemplate.Replace("#GrandTotal#", (data.GrandTotal ?? 0).ToString("0.00"));

                // Send customer email
                SendEmail(
                    _configuration["AppSettings:EmailSender"],
                    data.Email,
                    $"Order #{OrderID} - Payment Pending",
                    customerTemplate
                );

                // Send admin email
                SendEmail(
                    _configuration["AppSettings:EmailSender"],
                    _configuration["AppSettings:EmailReceivers"],
                    $"New Order #{OrderID} - Payment Pending",
                    adminTemplate
                );
            }
            catch (Exception ex)
            {
                // Log silently
            }
        }

        private string GetPaymentTypeName(int? paymentMethodID)
        {
            return paymentMethodID switch
            {
                1 => "Cash on Delivery",
                2 => "Credimax",
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
                // Log error
            }
        }

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

    /// <summary>
    /// Helper class for form validation results
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}