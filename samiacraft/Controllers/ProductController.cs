using samiacraft.Models.BLL;
using samiacraft.Models.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace samiacraft.Controllers
{
    public class ProductController : Controller
    {
        private readonly productService _service;
        private readonly IConfiguration _configuration;
        
        public ProductController(IConfiguration configuration)
        {
            _configuration = configuration;
            _service = new productService();
        }

        // GET: Product
        public IActionResult ProductDetails(int ItemID)
        {
            int LocationId = Convert.ToInt32(_configuration["LocationId"]);

            try
            {
                ViewBag.CanonicalUrl = $"https://www.karachiflora.com/Product/ProductDetails?ItemID={ItemID}";
                
                ViewBag.ImageUrl = _configuration["Image"] ?? "https://retail.premium-pos.com";
                
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
                
                var productDetails = _service.GetAll(ItemID, LocationId);
                ViewBag.ProductDetails = productDetails;
                ViewBag.Gift = new giftService().GetAll();
                
                return View(productDetails);
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        public IActionResult Wishlist()
        {
            try
            {
                ViewBag.ImageUrl = _configuration["Image"] ?? "https://retail.premium-pos.com";
                
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
                
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [HttpPost]
        public JsonResult PostProductReview(productBLL.ReviewsBLL data)
        {
            try
            {
                var result = new productBLL().InsertProductReview(data);
                return Json(new { data = result });
            }
            catch (Exception ex)
            {
                return Json(new { data = 0, error = ex.Message });
            }
        }
    }
}