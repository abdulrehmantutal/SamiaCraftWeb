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
    public class productBLL
    {
        public int ItemID { get; set; }
        public int? Stars { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string ArabicTitle { get; set; }
        public double? Cost { get; set; }
        public double? Price { get; set; }
        public double? DiscountedPrice { get; set; }
        public double? DoublePrice { get; set; }
        public bool? InStock { get; set; }
        public int? StatusID { get; set; }
        public int? StockQty { get; set; }
        public bool? IsDoubleQty { get; set; }
        public List<ItemImages> ImgList = new List<ItemImages>();
        public List<ReviewsBLL> Reviews= new List<ReviewsBLL>();

        public class ItemImages
        {
            public string Image { get; set; }
            public int Row_Counter { get; set; }
        }
        public class ReviewsBLL
        {
            public int ReviewID { get; set; }

            public int? Stars { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string Email { get; set; }

            public string Contact { get; set; }

            public int? StatusID { get; set; }

            public int? ItemID { get; set; }
        }


        public static DataTable _dt;
        public static DataSet _ds;
        public productBLL GetAll(int ItemID)
        {
            try
            {
                // Using mock data for development
                var mockService = new MockDataService();
                return mockService.GetProductDetails(ItemID);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public int InsertProductReview(ReviewsBLL data)
        {

            try
            {
                SqlParameter[] p = new SqlParameter[7];
                p[0] = new SqlParameter("@Name", data.Name);
                p[1] = new SqlParameter("@Description", data.Description);
                p[2] = new SqlParameter("@Contact", data.Contact);
                p[3] = new SqlParameter("@Email", data.Email);
                p[4] = new SqlParameter("@Stars", data.Stars);
                p[5] = new SqlParameter("@StatusID", data.StatusID);
                p[6] = new SqlParameter("@ItemID", data.ItemID);
                return int.Parse(new DBHelper().GetTableFromSP("sp_InsertReview", p).Rows[0]["ID"].ToString());
                
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}