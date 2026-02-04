using samiacraft.Models.BLL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using static samiacraft.Models.BLL.checkoutBLL;
using samiacraft.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using samiacraft.Models.Service;
using samiacraft.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace samiacraft.Controllers
{
    public class OrderController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public OrderController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Order
        public IActionResult Cart()
        {
            try
            {
                var settng = new settingBLL().GetSettings();
                if (settng?.DynamicList?.Count > 0)
                {
                    ViewBag.Logo = settng.DynamicList[0].Logo;
                    ViewBag.TopheaderArea = settng.DynamicList[0].HeaderToparea;
                    ViewBag.AddButton = settng.DynamicList[0].AddButton;
                }
                var cityData = new cityService().GetAll();
                ViewBag.City = cityData ?? new List<cityBLL>();
                ViewBag.Banner = new bannerBLL().GetBanner("Cart");
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
                var settng = new settingBLL().GetSettings();
                if (settng?.DynamicList?.Count > 0)
                {
                    ViewBag.Logo = settng.DynamicList[0].Logo;
                    ViewBag.TopheaderArea = settng.DynamicList[0].HeaderToparea;
                    ViewBag.AddButton = settng.DynamicList[0].AddButton;
                }
                var cityData = new cityService().GetAll();
                ViewBag.City = cityData ?? new List<cityBLL>();
                ViewBag.Banner = new bannerBLL().GetBanner("Checkout");
                
                int CustomerID = id;
                if (CustomerID == 0)
                {
                    var DAList = checkoutBLL.GetDeliveryArea();
                    ViewBag.DeliveryAreaList = new SelectList(DAList, "Amount", "Name");
                    HttpContext.Session.SetInt32("CustomerID", 0);
                    return View();
                }
                else
                {
                    var sessionCustomerId = HttpContext.Session.GetInt32("CustomerID");
                    if (sessionCustomerId != null && sessionCustomerId != 0)
                    {
                        var DAList = checkoutBLL.GetDeliveryArea();
                        ViewBag.DeliveryAreaList = new SelectList(DAList, "Amount", "Name");
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

        public IActionResult OrderComplete(int OrderID = 0, string OrderNo = "")
        {
            try
            {
                var settng = new settingBLL().GetSettings();
                if (settng?.DynamicList?.Count > 0)
                {
                    ViewBag.Logo = settng.DynamicList[0].Logo;
                    ViewBag.TopheaderArea = settng.DynamicList[0].HeaderToparea;
                    ViewBag.AddButton = settng.DynamicList[0].AddButton;
                }
                var cityData = new cityService().GetAll();
                ViewBag.City = cityData ?? new List<cityBLL>();
                
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

        //Coupon
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

        //Order
        public JsonResult PunchOrder(checkoutBLL data)
        {
            try
            {
                var currDate = DateTime.UtcNow.AddMinutes(300);
                var isAllowcheckout = false;
                int rtn = 0;

                if (data.OpenTime != null && data.CloseTime != null)
                {
                    var t1 = int.Parse(TimeSpan.Parse(data.OpenTime).ToString("hhmm"));
                    var t2 = 2359;
                    var t3 = 0001;
                    var t4 = int.Parse(TimeSpan.Parse(data.CloseTime).ToString("hhmm"));
                    var currTimeint = int.Parse(Convert.ToDateTime(currDate).ToString("HHmm"));
                    
                    isAllowcheckout = t1 > t4 ? ((currTimeint > t1 && currTimeint < t2) || (currTimeint > t3 && currTimeint < t4) ? true : false) : (currTimeint > t1 && currTimeint < t4);
                }

                checkoutBLL _service = new checkoutBLL();
                
                //orderdetails
                if (!string.IsNullOrEmpty(data.OrderDetailString))
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(JArray.Parse(data.OrderDetailString));
                    JArray jsonResponse = JArray.Parse(json);
                    data.OrderDetail = jsonResponse.ToObject<List<OrderDetails>>();
                }
                
                //gifts
                try
                {
                    if (!string.IsNullOrEmpty(data.OrderGiftsString))
                    {
                        string jsonGift = Newtonsoft.Json.JsonConvert.SerializeObject(JArray.Parse(data.OrderGiftsString));
                        JArray jsonResponseGift = JArray.Parse(jsonGift);
                        data.OrderGifts = jsonResponseGift.ToObject<List<OrderGiftDetails>>();
                    }
                }
                catch (Exception ex)
                { }

                if (!isAllowcheckout)
                {
                    ViewBag.CheckoutMsg = "Sorry, Delivery Time is from " + Convert.ToDateTime(data.OpenTime).ToString("hh:mm tt") + " to " + Convert.ToDateTime(data.CloseTime).ToString("hh:mm tt");
                }
                else
                {
                    rtn = _service.InsertOrder(data);
                }

                if (data.PaymentMethodID == 7)
                {
                    return Json(new { data = "DebitCreditCard", OrderID = rtn });
                }
                return Json(new { data = rtn });
            }
            catch (Exception ex)
            {
                return Json(new { data = 0, error = ex.Message });
            }
        }

        public IActionResult MyOrders()
        {
            try
            {
                var cityData = new cityService().GetAll();
                ViewBag.City = cityData ?? new List<cityBLL>();
                ViewBag.Banner = new bannerBLL().GetBanner("Other");
                
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
                var settng = new settingBLL().GetSettings();
                if (settng?.DynamicList?.Count > 0)
                {
                    ViewBag.Logo = settng.DynamicList[0].Logo;
                    ViewBag.TopheaderArea = settng.DynamicList[0].HeaderToparea;
                    ViewBag.AddButton = settng.DynamicList[0].AddButton;
                }
                var cityData = new cityService().GetAll();
                ViewBag.City = cityData ?? new List<cityBLL>();
                ViewBag.Banner = new bannerBLL().GetBanner("Other");
                return View(new myorderBLL().GetDetails(OrderID));
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        public IActionResult BenefitPay(string OrderNo, int OrderID, double GrandTotal)
        {
            try
            {
                // Placeholder for BenefitPay integration
                // TODO: Implement iPayBenefitPipe payment gateway integration
                ViewBag.Message = "Payment gateway integration required";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error: " + ex.Message;
                return View();
            }
        }

        public IActionResult BenefitPayResponse(int OrderID = 0)
        {
            try
            {
                // Placeholder for BenefitPay response handling
                ViewBag.Message = "Payment response processing";
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        public IActionResult BenefitPayApproved(int OrderID = 0)
        {
            try
            {
                if (OrderID != 0)
                {
                    return RedirectToAction("OrderComplete", "Order", new { OrderID = OrderID });
                }
                else
                {
                    return RedirectToAction("OrderComplete", "Order", new { OrderNo = "Reject" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("OrderComplete", "Order", new { OrderNo = "Reject" });
            }
        }

        public IActionResult HBLPaymentApproved(string data = "", int OrderID = 0)
        {
            try
            {
                // Placeholder for HBL Payment Gateway response
                // TODO: Implement HBL payment gateway integration and RSA decryption
                ViewBag.Message = "HBL payment gateway integration required";
                return RedirectToAction("OrderComplete", "Order", new { OrderNo = "Reject" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("OrderComplete", "Order", new { OrderNo = "Reject" });
            }
        }
    }

    public class HBLPayResponseModel
    {
        public SessionIdContainer Data { get; set; }
        public bool IsSuccess { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }

    public class SessionIdContainer
    {
        public string SESSION_ID { get; set; }
    }
}
