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
    public class reviewBLL
    {
        public int ReviewID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class bannerBLL
    {
        public int BannerID { get; set; }
        public string Title { get; set; }
        public string MainHeading { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int StatusID { get; set; }
        public int Row_Counter { get; set; }
        public string ArabicTitle { get; set; }
        public string ArabicMainHeading { get; set; }
        public string ArabicDescription { get; set; }
        public string FormName { get; set; }

        public string DeviceType { get; set; }

        public static DataTable _dt;
        public static DataSet _ds;
        public List<bannerBLL> GetBanner(string FormName)
        {
            try
            {
                // Using mock data for development
                var mockService = new MockDataService();
                var banners = mockService.GetAllBanners();
                
                // Filter by form name if needed
                if (!string.IsNullOrEmpty(FormName))
                {
                    banners = banners.Where(b => b.FormName == FormName).ToList();
                }
                
                return banners ?? new List<bannerBLL>();
            }
            catch (Exception ex)
            {
                return new List<bannerBLL>();
            }
        }
        
        public List<reviewBLL> GetReviews()
        {
            try
            {
                var lst = new List<reviewBLL>();
                _dt = new DBHelper().GetTableFromSP("sp_GetReviewsWeb");

                if (_dt != null && _dt.Rows.Count > 0)
                {
                    lst = _dt.DataTableToList<reviewBLL>();
                    
                    // Filter out null or invalid items
                    lst = lst
                        .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Description))
                        .ToList();
                }
                
                return lst ?? new List<reviewBLL>();
            }
            catch (Exception ex)
            {
                return new List<reviewBLL>();
            }
        }
    }
}