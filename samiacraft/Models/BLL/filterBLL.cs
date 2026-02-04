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
        public string MinPrice { get; set; }
        public string MaxPrice { get; set; }
        public string SubCategory { get; set; }
        public string Searchtxt { get; set; }
        public int SortID { get; set; }
        public int ItemID { get; set; }
        public string Title { get; set; }
        public string ArabicTitle { get; set; }
        public string SKU { get; set; }
        public string Description { get; set; }
        public double Cost { get; set; }
        public double Price { get; set; }
        public double DiscountedPrice { get; set; }
        public string Barcode { get; set; }
        public bool InStock { get; set; }
        public string Image { get; set; }
        public string HoveredImage { get; set; }
        public int StatusID { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; }
        public int StockQty { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public int? LastUpdatedBy { get; set; }
        public double? DoublePrice { get; set; }
        public int? Stars { get; set; }
        public static DataTable _dt;
        public static DataSet _ds;
        public List<filterBLL> GetAll(filterBLL data, int? cityID)
        {
            try
            {
                var lst = new List<filterBLL>();
                SqlParameter[] p = new SqlParameter[8];
                p[0] = new SqlParameter("@Category", data.Category == "" ? null : data.Category);
                p[1] = new SqlParameter("@Color", data.Color == "" ? null : data.Color);
                p[2] = new SqlParameter("@SubCategory", data.SubCategory == "" ? null : data.SubCategory);
                p[3] = new SqlParameter("@MinPrice", float.Parse(data.MinPrice.Replace("RS.","").Replace("RS", "")));
                p[4] = new SqlParameter("@MaxPrice", float.Parse(data.MaxPrice.Replace("RS.", "").Replace("RS", "")));
                p[5] = new SqlParameter("@Searchtxt", data.Searchtxt == "" ? null : data.Searchtxt);
                p[6] = new SqlParameter("@SortID", data.SortID);
                p[7] = new SqlParameter("@CityID", cityID);
               _ds = (new DBHelper().GetDatasetFromSP)("sp_filterProduct_V3", p);
                if (_ds != null)
                {   
                    if (_ds.Tables.Count > 0)
                    {
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<filterBLL>>();
                    }
                }
                if (data.SortID == 2)
                {
                    lst = lst.OrderByDescending(x => x.Title).ToList();
                }
                else if (data.SortID == 3)
                {
                    lst = lst.OrderBy(x => x.Price).ToList();
                }
                else if (data.SortID == 4)
                {
                    lst = lst.OrderByDescending(x => x.Price).ToList();
                }
                else if (data.SortID == 1)
                {
                    lst = lst.OrderBy(x => x.Title).ToList();
                }
                else
                {

                }
                return lst;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}