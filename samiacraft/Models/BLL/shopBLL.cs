using samiacraft.Models;
using samiacraft.Models.Service;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using samiacraft.Helpers;

namespace samiacraft.Models.BLL
{
    public class shopBLL
    {
        public int ItemID { get; set; }
        public string Title { get; set; }
        public string ArabicTitle { get; set; }
        public string SKU { get; set; }
        public string Description { get; set; }
        public string Cost { get; set; }
        public double? Price { get; set; }
        public double? DiscountedPrice { get; set; }
        public double? Barcode { get; set; }
        public bool? InStock { get; set; }
        public string Image { get; set; }
        public string HoveredImage { get; set; }
        public int? StatusID { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsFeatured { get; set; }
        public int? StockQty { get; set; }
        public Nullable<System.DateTime> LastUpdatedDate { get; set; }
        public int? LastUpdatedBy { get; set; }
        public int? Row_Counter { get; set; }
        public int? Stars { get; set; }

        public static DataTable _dt;
        public static DataSet _ds;
        public List<shopBLL> GetAll(string Category)
        {
            try
            {
                // Using mock data for development
                var mockService = new MockDataService();
                var items = mockService.GetAllItems();
                
                // Convert itemBLL to shopBLL
                var lst = items.Select(i => new shopBLL
                {
                    ItemID = i.ID,
                    Title = i.Name,
                    ArabicTitle = i.ArabicName,
                    Description = i.Description,
                    Price = i.Price,
                    DiscountedPrice = i.Price,
                    Image = i.Image,
                    Stars = i.Stars,
                    StatusID = i.StatusID,
                    DisplayOrder = i.DisplayOrder,
                    IsFeatured = i.IsFeatured
                }).ToList();
                
                // Filter by category if provided
                if (!string.IsNullOrEmpty(Category))
                {
                    lst = lst.Where(x => x.Title.Contains(Category) || Category == "All").ToList();
                }
                
                return lst;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<shopBLL> BestProducts(int? cityID)
        {
            try
            {
                // Using mock data for development
                var mockService = new MockDataService();
                var items = mockService.GetPopularProducts();
                
                // Convert itemBLL to shopBLL
                var lst = items.Select(i => new shopBLL
                {
                    ItemID = i.ID,
                    Title = i.Name,
                    ArabicTitle = i.ArabicName,
                    Description = i.Description,
                    Price = i.Price,
                    DiscountedPrice = i.Price,
                    Image = i.Image,
                    Stars = i.Stars,
                    StatusID = i.StatusID,
                    DisplayOrder = i.DisplayOrder,
                    IsFeatured = i.IsFeatured
                }).ToList();
                
                return lst;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}