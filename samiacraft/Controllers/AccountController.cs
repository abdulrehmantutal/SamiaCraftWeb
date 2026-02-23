using samiacraft.Models.BLL;
using samiacraft.Models.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace samiacraft.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: Account
        [HttpGet]
        public IActionResult Login_Register(int id = 0)
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
                HttpContext.Session.SetInt32("LoginRoute", id);
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [HttpPost]
        public IActionResult Login_Register(loginBLL service)
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

                if (!string.IsNullOrEmpty(service.ContactNo))
                {
                    // Registration
                    service.Register();
                    HttpContext.Session.SetString("LoginNote", "Login Now");
                    return RedirectToAction("Login_Register", "Account");
                }
                else
                {
                    // Login
                    HttpContext.Session.SetString("LoginNote", null);
                    service = service.login();
                    
                    if (service != null && service.CustomerID > 0)
                    {
                        HttpContext.Session.SetInt32("CustomerID", service.CustomerID);
                        HttpContext.Session.SetString("CustomerEmail", service.Email ?? "");
                        HttpContext.Session.SetString("CustomerContactNo", service.ContactNo ?? "");
                        HttpContext.Session.SetString("CustomerName", (service.FirstName + " " + service.LastName) ?? "");
                        HttpContext.Session.SetInt32("IsVerified", service.IsVerified);

                        if (service.IsVerified != 0)
                        {
                            if (!string.IsNullOrEmpty(service.Email))
                            {
                                HttpContext.Session.SetString("LoginNote", "Successfully Login");
                                var loginRoute = HttpContext.Session.GetInt32("LoginRoute");
                                if (loginRoute == 1)
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                                else
                                {
                                    return RedirectToAction("Checkout", "Order");
                                }
                            }
                            HttpContext.Session.SetString("LoginNote", "User is not verified");
                            return RedirectToAction("Login_Register", "Account");
                        }
                        else
                        {
                            HttpContext.Session.SetString("CustomerName", null);
                            HttpContext.Session.SetString("LoginNote", "Invalid Email or Password");
                            return RedirectToAction("Login_Register", "Account");
                        }
                    }
                    else
                    {
                        HttpContext.Session.SetString("LoginNote", "Invalid Email or Password");
                        return RedirectToAction("Login_Register", "Account");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred: " + ex.Message;
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("LoginNote");
            HttpContext.Session.Remove("CustomerID");
            HttpContext.Session.Remove("CustomerEmail");
            HttpContext.Session.Remove("CustomerContactNo");
            HttpContext.Session.Remove("CustomerName");
            HttpContext.Session.Remove("IsVerified");
            HttpContext.Session.Remove("LoginRoute");
            return RedirectToAction("Index", "Home");
        }
    }
}