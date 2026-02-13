using samiacraft.Models.BLL;
using samiacraft.Models.Service;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace samiacraft.Controllers
{
    public class ShopController : Controller
    {
        private readonly shopService _service;
        private readonly filterService _filterService;
        
        public ShopController()
        {
            _service = new shopService();
            _filterService = new filterService();
        }

        // GET: Shop
        public IActionResult Shop(string Category = "", string CategoryIDs = "", string SubCategoryIDs = "", string ColorIDs = "", string MinPrice = "", string MaxPrice = "", string Searchtext = "", int SortID = 0)
        {
            try
            {
                // Set canonical URL (shop page without query string)
                ViewBag.CanonicalUrl = "https://www.karachiflora.com/Shop/Shop";
                
                var settng = new settingBLL().GetSettings();
                if (settng?.DynamicList?.Count > 0)
                {
                    ViewBag.Logo = settng.DynamicList[0].Logo;
                    ViewBag.TopheaderArea = settng.DynamicList[0].HeaderToparea;
                    ViewBag.AddButton = settng.DynamicList[0].AddButton;
                }
                
                int cityID = 2;
                var cityData = new cityService().GetAll();
                ViewBag.City = cityData ?? new List<cityBLL>();
                ViewBag.BestProduct = _service.BestProducts(cityID);
                ViewBag.Category = new categoryBLL().GetAll();
                ViewBag.SubCategory = new subcategoryBLL().GetAll();
                ViewBag.Color = new colorBLL().GetAll();
                ViewBag.Banner = new bannerBLL().GetBanner("Shop");
                
                TempData["Category"] = Category;
                TempData["CategoryIDs"] = CategoryIDs;
                TempData["SubCategoryIDs"] = SubCategoryIDs;
                TempData["ColorIDs"] = ColorIDs;
                TempData["MinPrice"] = MinPrice;
                TempData["MaxPrice"] = MaxPrice;
                TempData["Searchtext"] = Searchtext;
                TempData["SortID"] = SortID.ToString();
                
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }
        
        public IActionResult Products(List<filterBLL> Products)
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
                
                ViewBag.Message = "";
                
                if (Products != null && Products.Count > 0)
                {
                    ViewBag.shopList = Products;
                    return PartialView("AllProducts");
                }
                else
                {
                    if (TempData.Count > 1)
                    {
                        var categoryIds = TempData["CategoryIDs"]?.ToString() ?? "";
                        var subCategoryIds = TempData["SubCategoryIDs"]?.ToString() ?? "";
                        var colorIds = TempData["ColorIDs"]?.ToString() ?? "";
                        var minPrice = TempData["MinPrice"]?.ToString() ?? "";
                        var maxPrice = TempData["MaxPrice"]?.ToString() ?? "";
                        var searchText = TempData["Searchtext"]?.ToString() ?? "";
                        var sortId = TempData["SortID"]?.ToString() ?? "0";

                        if (!string.IsNullOrEmpty(categoryIds) ||
                            !string.IsNullOrEmpty(subCategoryIds) ||
                            !string.IsNullOrEmpty(colorIds) ||
                            !string.IsNullOrEmpty(minPrice) ||
                            !string.IsNullOrEmpty(maxPrice) ||
                            !string.IsNullOrEmpty(searchText) ||
                            sortId != "0")
                        {
                            filterBLL data = new filterBLL();
                            data.Category = categoryIds;
                            data.SubCategory = subCategoryIds;
                            data.Color = colorIds;
                            data.MinPrice = string.IsNullOrEmpty(minPrice) ? "RS0" : minPrice;
                            data.MaxPrice = string.IsNullOrEmpty(maxPrice) ? "RS50000" : maxPrice;
                            data.Searchtxt = searchText;
                            data.SortID = Convert.ToInt32(sortId);

                            int cityID = 2;
                            var shopList = _filterService.GetAll(data, cityID);
                            ViewBag.shopList = shopList ?? new List<filterBLL>();
                            
                            if (shopList == null || shopList.Count == 0)
                            {
                                ViewBag.Message = "No Product Found";
                            }
                        }
                        else
                        {
                            ViewBag.shopList = new List<filterBLL>();
                            ViewBag.Message = "No Product Found";
                        }
                    }
                    else
                    {
                        ViewBag.shopList = new List<filterBLL>();
                        ViewBag.Message = "No Product Found";
                    }
                    
                    return PartialView("AllProducts");
                }
            }
            catch (Exception ex)
            {
                ViewBag.shopList = new List<filterBLL>();
                ViewBag.Message = "Error loading products";
                return PartialView("AllProducts");
            }
        }

        public JsonResult Filter(filterBLL data)
        {
            try
            {
                int cityID = 2;
                var shopList = _filterService.GetAll(data, cityID);
                return Json(new { data = shopList ?? new List<filterBLL>() });
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<filterBLL>(), error = ex.Message });
            }
        }
    }
}