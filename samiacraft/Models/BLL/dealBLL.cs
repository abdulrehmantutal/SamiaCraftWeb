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
    public class dealBLL
    {
        public int DealID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DealImage { get; set; }
        public double DiscountedPrice { get; set; }
        public DateTime StartDate { get; set; }
        public string EndDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public int NumberOfDays { get; set; }
        public int ItemID { get; set; }
        public int DisplayOrder { get; set; }
        public int LastUpdatedBy { get; set; }
        public int Row_Counter { get; set; }

        public static DataTable _dt;
        public static DataSet _ds;

        public List<dealBLL> GetAll()
        {
            try
            {
                // Using mock data for development
                var mockService = new MockDataService();
                return mockService.GetAllDeals();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}