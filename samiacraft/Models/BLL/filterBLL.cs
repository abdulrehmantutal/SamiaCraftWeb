using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using samiacraft.Helpers;

namespace samiacraft.Models.BLL
{
    public class filterBLL
    {
        public string Category { get; set; }
        public string Color { get; set; }
        public string SubCategory { get; set; }
        public string Searchtxt { get; set; }
        public string MinPrice { get; set; }
        public string MaxPrice { get; set; }
        public int SortID { get; set; }
        public int LocationId { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string ArabicName { get; set; }
        public string ArabicTitle { get; set; }
        public string SKU { get; set; }
        public string ArabicDescription { get; set; }
        public string Description { get; set; }
        public double Cost { get; set; }
        public double Price { get; set; }
        public double? NewPrice { get; set; }
        public double DiscountedPrice { get; set; }
        public string Barcode { get; set; }
        public bool InStock { get; set; }
        public string Image { get; set; }
        public string HoveredImage { get; set; }
        public int StatusID { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsStockOut { get; set; }
        public int StockQty { get; set; }
        public Nullable<System.DateTime> LastUpdatedDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public double? DoublePrice { get; set; }
        public int? Stars { get; set; }

        public static DataTable _dt;
        public static DataSet _ds;

        public List<filterBLL> GetAll(filterBLL data)
        {
            try
            {
                var lst = new List<filterBLL>();


                if (data == null)
                {
                    return lst;
                }

                string category = data.Category ?? "";
                string subCategory = data.SubCategory ?? "";
                string searchTxt = data.Searchtxt ?? "";
                string maxPrice = data.MaxPrice ?? "";
                string minPrice = data.MinPrice ?? "";
                int sortID = data.SortID;
                int LocationId = data.LocationId;

                SqlParameter[] p = new SqlParameter[7];
                p[0] = new SqlParameter("@Category", string.IsNullOrWhiteSpace(category) ? (object)DBNull.Value : category);
                p[1] = new SqlParameter("@Searchtxt", string.IsNullOrWhiteSpace(searchTxt) ? (object)DBNull.Value : searchTxt);
                p[2] = new SqlParameter("@MinPrice", string.IsNullOrWhiteSpace(minPrice) ? (object)DBNull.Value : minPrice);
                p[3] = new SqlParameter("@MaxPrice", string.IsNullOrWhiteSpace(maxPrice) ? (object)DBNull.Value : maxPrice);
                p[4] = new SqlParameter("@SortID", sortID);
                p[5] = new SqlParameter("@SubCategory", string.IsNullOrWhiteSpace(subCategory) ? (object)DBNull.Value : subCategory);
                p[6] = new SqlParameter("@LocationId", LocationId);

                string spName = "";

                if (!string.IsNullOrWhiteSpace(maxPrice))
                {
                    spName = "sp_Website_PricefilterProduct";
                }



                else if (!string.IsNullOrWhiteSpace(category) || !string.IsNullOrWhiteSpace(subCategory))
                {
                    spName = "sp_Website_CategoryfilterProduct";
                }
                else if (sortID >= 1 && sortID <= 4)
                {
                    spName = "sp_Website_SortfilterProduct";
                }
                else
                {
                    spName = "sp_Website_filterProduct";
                }

                _ds = (new DBHelper().GetDatasetFromSP)(spName, p);

                if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                {
                    lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0]))
                        .ToObject<List<filterBLL>>();
                }

                return lst;
            }
            catch (Exception ex)
            {
                return new List<filterBLL>();
            }
        }

        public List<filterBLL> GetAllCat(filterBLL data)
        {
            try
            {
                var lst = new List<filterBLL>();

                if (data == null)
                {
                    return lst;
                }

                string category = data.Category ?? "";

                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@Category", string.IsNullOrWhiteSpace(category) ? (object)DBNull.Value : category);

                _ds = (new DBHelper().GetDatasetFromSP)("sp_filterCategory_Web", p);

                if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                {
                    lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0]))
                        .ToObject<List<filterBLL>>();
                }

                return lst;
            }
            catch (Exception ex)
            {
                return new List<filterBLL>();
            }
        }

        public List<filterBLL> GetAll()
        {
            try
            {
                var lst = new List<filterBLL>();
                _dt = (new DBHelper().GetTableFromSP)("sp_GetItemsList");

                if (_dt != null && _dt.Rows.Count > 0)
                {
                    lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt))
                        .ToObject<List<filterBLL>>();
                }

                return lst;
            }
            catch (Exception ex)
            {
                return new List<filterBLL>();
            }
        }
    }
}