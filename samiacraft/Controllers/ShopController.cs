using samiacraft.Models.BLL;
using samiacraft.Models.Service;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace samiacraft.Controllers
{
    public class ShopController : Controller
    {
        private readonly shopService _shopService;
        private readonly filterService _filterService;
        private readonly blogfilterService _blogfilterService;
        private readonly IConfiguration _configuration;

        public ShopController(IConfiguration configuration)
        {
            _configuration = configuration;
            _shopService = new shopService();
            _filterService = new filterService();
            _blogfilterService = new blogfilterService();
        }

        public IActionResult BlogCategory(
            string category = "", 
            string categoryIds = "", 
            string subCategoryIds = "", 
            string searchText = "", 
            int sortId = 0, 
            string minPrice = "", 
            string maxPrice = "")
        {
            try
            {
                ViewBag.TopheaderArea = "#1C3D5A";
                ViewBag.AddButton = "#1C3D5A";
                SetupViewBagDefaults();

                int locationId = (int)Location.LocationID;
                var categoryList = new blogCategoryBLL().GetAll(locationId);
                ViewBag.Category = categoryList?.Take(9).ToList() ?? new List<blogCategoryBLL>();

                var blogData = new itemService().GetAllBlog(locationId);
                var filteredBlogs = blogData?
                    .Where(x => x.StatusID > 0)
                    .OrderBy(x => x.StatusID)
                    .Take(48)
                    .ToList() ?? new List<blogBLL>();
                
                ViewBag.ItemList = filteredBlogs;
                ViewBag.BestProduct = filteredBlogs.Take(4).ToList();

                var specialItems = new blogfilterBLL().GetAll();
                ViewBag.TodaysSpecial = specialItems?.Take(4).ToList() ?? new List<blogfilterBLL>();

                StoreTempData(category, categoryIds, subCategoryIds, searchText, minPrice, maxPrice, sortId);
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        public IActionResult Shop(
            string category = "", 
            string categoryIds = "", 
            string subCategoryIds = "", 
            string searchText = "", 
            int sortId = 0, 
            string minPrice = "", 
            string maxPrice = "")
        {
            try
            {
                SetupViewBagDefaults();

                int locationId = (int)Location.LocationID;
                var categoryList = new categoryBLL().GetAll(locationId);
                ViewBag.Category = categoryList?.Take(9).ToList() ?? new List<categoryBLL>();

                var itemData = new itemService().GetAll(locationId);
                var filteredItems = itemData?
                    .Where(x => x.StatusID > 0)
                    .OrderBy(x => x.StatusID)
                    .Take(48)
                    .ToList() ?? new List<itemBLL>();
                
                ViewBag.ItemList = filteredItems;
                ViewBag.BestProduct = filteredItems.Take(4).ToList();

                var filterItems = new filterBLL().GetAll();
                ViewBag.TodaysSpecial = filterItems?.Take(4).ToList() ?? new List<filterBLL>();

                StoreTempData(category, categoryIds, subCategoryIds, searchText, minPrice, maxPrice, sortId);
                
                // Pass selected category IDs to the view for pre-selection
                ViewBag.SelectedCategoryIds = categoryIds;

                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [HttpPost]
        public JsonResult Filter([FromBody] filterBLL filterData)
        {
            try
            {
                var filteredProducts = _filterService.GetAll(filterData) ?? new List<filterBLL>();
                return Json(new { success = true, data = filteredProducts });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Products(List<filterBLL> products)
        {
            try
            {
                SetupViewBagDefaults();

                string message = string.Empty;
                List<filterBLL> shopList = new();

                if (products != null && products.Count > 0)
                {
                    shopList = products;
                    message = shopList.Count < 1 ? "No Product Found" : string.Empty;
                }
                else if (TempData.Count > 0)
                {
                    var filterData = BuildFilterFromTempData();
                        shopList = _filterService.GetAll(filterData) ?? new();
                        message = shopList.Count < 1 ? "No Product Found" : string.Empty;
                }
                else
                {
                    message = "No Product Found";
                }

                ViewBag.ShopList = shopList;
                ViewBag.Message = message;
                return PartialView("AllProducts");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error loading products";
                return PartialView("AllProducts");
            }
        }

        [HttpPost]
        public JsonResult BlogFilter([FromBody] blogfilterBLL filterData)
        {
            try
            {
                var filteredBlogs = _blogfilterService.GetAll(filterData) ?? new List<blogfilterBLL>();
                return Json(new { success = true, data = filteredBlogs });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult BlogProducts(List<blogfilterBLL> products)
        {
            try
            {
                SetupViewBagDefaults();

                string message = string.Empty;
                List<blogfilterBLL> blogList = new();

                if (products != null && products.Count > 0)
                {
                    blogList = products;
                    message = blogList.Count < 1 ? "No Product Found" : string.Empty;
                }
                else if (TempData.Count > 0)
                {
                    var filterData = BuildBlogFilterFromTempData();
                    if (filterData != null)
                    {
                        blogList = _blogfilterService.GetAll(filterData) ?? new();
                        
                        if (blogList.Count > 0)
                        {
                            ViewBag.BlogCatHeading = blogList.FirstOrDefault()?.BCatName ?? string.Empty;
                            ViewBag.BlogCatArHeading = blogList.FirstOrDefault()?.BCatArName ?? string.Empty;
                        }
                        
                        message = blogList.Count < 1 ? "No Product Found" : string.Empty;
                    }
                }
                else
                {
                    message = "No Product Found";
                }

                ViewBag.ShopList = blogList;
                ViewBag.Message = message;
                return PartialView("AllBlogs");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error loading blogs";
                return PartialView("AllBlogs");
            }
        }

        private void SetupViewBagDefaults()
        {
            ViewBag.ImageUrl = _configuration["Image"] ?? "https://retail.premium-pos.com";
        }

        private void StoreTempData(
            string category, 
            string categoryIds, 
            string subCategoryIds, 
            string searchText, 
            string minPrice, 
            string maxPrice, 
            int sortId)
        {
            TempData["Category"] = category;
            TempData["CategoryIDs"] = categoryIds;
            TempData["SubCategoryIDs"] = subCategoryIds;
            TempData["Searchtext"] = searchText;
            TempData["MaxPrice"] = maxPrice;
            TempData["MinPrice"] = minPrice;
            TempData["SortID"] = sortId.ToString();
        }

        private filterBLL BuildFilterFromTempData()
        {
            var locationId = _configuration["LocationId"];

            var categoryIds = TempData["CategoryIDs"]?.ToString() ?? string.Empty;
            var subCategoryIds = TempData["SubCategoryIDs"]?.ToString() ?? string.Empty;
            var searchText = TempData["Searchtext"]?.ToString() ?? string.Empty;
            var minPrice = TempData["MinPrice"]?.ToString() ?? string.Empty;
            var maxPrice = TempData["MaxPrice"]?.ToString() ?? string.Empty;
            var sortId = int.TryParse(TempData["SortID"]?.ToString(), out int id) ? id : 0;

            return new filterBLL
            {
                Category = categoryIds,
                SubCategory = subCategoryIds,
                Searchtxt = searchText,
                MaxPrice = maxPrice,
                MinPrice = minPrice,
                SortID = sortId,
                LocationId = locationId.Length > 0 ? int.Parse(locationId) : 0
            };
        }

        private blogfilterBLL BuildBlogFilterFromTempData()
        {
            var categoryIds = TempData["CategoryIDs"]?.ToString() ?? string.Empty;
            var subCategoryIds = TempData["SubCategoryIDs"]?.ToString() ?? string.Empty;
            var searchText = TempData["Searchtext"]?.ToString() ?? string.Empty;
            var minPrice = TempData["MinPrice"]?.ToString() ?? string.Empty;
            var maxPrice = TempData["MaxPrice"]?.ToString() ?? string.Empty;
            var sortId = int.TryParse(TempData["SortID"]?.ToString(), out int id) ? id : 0;

            if (string.IsNullOrEmpty(categoryIds) && 
                string.IsNullOrEmpty(subCategoryIds) && 
                string.IsNullOrEmpty(searchText) && 
                string.IsNullOrEmpty(minPrice) && 
                string.IsNullOrEmpty(maxPrice) && 
                sortId != 5)
            {
                return new blogfilterBLL
                {
                    Category = categoryIds,
                    SubCategory = subCategoryIds,
                    Searchtxt = searchText,
                    MaxPrice = maxPrice,
                    MinPrice = minPrice,
                    SortID = sortId
                };
            }

            return null;
        }
    }
}