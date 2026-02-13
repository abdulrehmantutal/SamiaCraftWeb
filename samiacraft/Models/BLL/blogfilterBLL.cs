using Newtonsoft.Json.Linq;
using samiacraft.Helpers;
using System.Data;
using System.Data.SqlClient;

namespace samiacraft.Models.BLL
{
    public class blogfilterBLL
    {
        public string Category { get; set; }
        public string Title { get; set; }
        public string Color { get; set; }
        public string SubCategory { get; set; }
        public string Searchtxt { get; set; }
        public string MinPrice { get; set; }
        public string MaxPrice { get; set; }
        public int SortID { get; set; }
        public int ID { get; set; }
        public int BlogID { get; set; }
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
        public string BCatName { get; set; }
        public string BCatArName { get; set; }
        public string ImageSmall { get; set; }
        public string HoveredImage { get; set; }
        public int StatusID { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsStockOut { get; set; }
        public int StockQty { get; set; }
        public Nullable<System.DateTime> LastUpdatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public double? DoublePrice { get; set; }
        public int? Stars { get; set; }
        public static DataTable _dt;
        public static DataSet _ds;
        public List<blogfilterBLL> GetAll(blogfilterBLL data)
        {
            try
            {
                var lst = new List<blogfilterBLL>();
                if (!data.Category.Equals("") && !data.Category.Equals(" "))
                {
                    SqlParameter[] p = new SqlParameter[6];
                    p[0] = new SqlParameter("@BlogCategory", data.Category == "" ? null : data.Category);
                    p[1] = new SqlParameter("@Searchtxt", data.Searchtxt == "" ? null : data.Searchtxt);
                    p[2] = new SqlParameter("@MinPrice", "");
                    p[3] = new SqlParameter("@MaxPrice", data.MaxPrice == "" ? null : data.MaxPrice);
                    p[4] = new SqlParameter("@SortID", data.SortID);
                    p[5] = new SqlParameter("@SubCategory", data.SubCategory == "" ? null : data.SubCategory);
                    _ds = (new DBHelper().GetDatasetFromSP)("sp_CategoryfilterBlogProduct_Vitamito", p);

                    if (_ds != null)
                    {
                        if (_ds.Tables.Count > 0)
                        {
                            lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<blogfilterBLL>>();
                        }
                    }
                }

                else
                {

                    SqlParameter[] p = new SqlParameter[6];
                    p[0] = new SqlParameter("@Category", data.Category == "" ? null : data.Category);
                    p[1] = new SqlParameter("@Searchtxt", data.Searchtxt == "" ? null : data.Searchtxt);
                    p[2] = new SqlParameter("@MinPrice", "");
                    p[3] = new SqlParameter("@MaxPrice", data.MaxPrice == "" ? null : data.MaxPrice);
                    p[4] = new SqlParameter("@SortID", data.SortID);
                    p[5] = new SqlParameter("@SubCategory", data.SubCategory == "" ? null : data.SubCategory);
                    _ds = (new DBHelper().GetDatasetFromSP)("sp_filterBlogProduct_Vitamito", p);

                    if (_ds != null)
                    {
                        if (_ds.Tables.Count > 0)
                        {
                            lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<blogfilterBLL>>();
                        }
                    }
                }
                return lst;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<filterBLL> GetAllCat(filterBLL data)
        {
            try
            {
                var lst = new List<filterBLL>();
                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@Category", data.Category == "" ? null : data.Category);
                _ds = (new DBHelper().GetDatasetFromSP)("sp_filterCategory_Web", p);


                if (_ds != null)
                {
                    if (_ds.Tables.Count > 0)
                    {
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<filterBLL>>();
                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<blogfilterBLL> GetAll()
        {
            try
            {
                var lst = new List<blogfilterBLL>();
                _dt = (new DBHelper().GetTableFromSP)("sp_GetBlogList_Vitamito");
                if (_dt != null)
                {
                    if (_dt.Rows.Count > 0)
                    {
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt)).ToObject<List<blogfilterBLL>>();
                        //lst = _dt.DataTableToList<itemBLL>();
                    }
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
