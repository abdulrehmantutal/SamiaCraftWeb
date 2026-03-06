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
    public class itemBLL
    {
        public int ID { get; set; }
        public int SubCategoryID { get; set; }
        public int? UnitID { get; set; }
        public string Name { get; set; }
        public string ArabicName { get; set; }
        public string NameOnReceipt { get; set; }
        public string Description { get; set; }
        public string ArabicDescription { get; set; }
        public string Image { get; set; }
        public double? Barcode { get; set; }
        public string SKU { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? SortByAlpha { get; set; }
        public double? Price { get; set; }
        public double? NewPrice { get; set; }
        public double? Cost { get; set; }
        public string ItemType { get; set; }
        public string LastUpdatedBy { get; set; }
        public Nullable<System.DateTime> LastUpdatedDate { get; set; }
        public int? StatusID { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public bool? HasVariant { get; set; }
        public bool? IsVATApplied { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsStockOut { get; set; }
        public double? CurrentStock { get; set; }
        public int? Stars { get; set; }
        public List<ReviewsBLL> Reviews = new List<ReviewsBLL>();
        public static DataTable _dt;
        public static DataSet _ds;
        public class ReviewsBLL
        {
            public int ReviewID { get; set; }

            public int? Stars { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string Email { get; set; }

            public string Contact { get; set; }

            public int? StatusID { get; set; }

            public int? ID { get; set; }
        }


        public List<itemBLL> GetAll(int LocationID)
        {
            try
            {
                var lst = new List<itemBLL>();
                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@LocationID", LocationID);
                _dt = (new DBHelper().GetTableFromSP)("sp_GetItem_Vitamito", p);

                if (_dt != null)
                {
                    if (_dt.Rows.Count > 0)
                    {
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt)).ToObject<List<itemBLL>>();
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
        public List<itemBLL> GetAllByCategories(string CategoryIds)
        {
            try
            {
                var lst = new List<itemBLL>();
                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@CategoryIds", CategoryIds);
                _dt = (new DBHelper().GetTableFromSP)("sp_Website_GetItemByCategories", p);

                if (_dt != null)
                {
                    if (_dt.Rows.Count > 0)
                    {
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt)).ToObject<List<itemBLL>>();
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public itemBLL GetSelecteditems(int? ID, int LocationID)
        {
            try
            {
                var lst = new itemBLL();
                List<ReviewsBLL> lstR = new List<ReviewsBLL>();
                SqlParameter[] p = new SqlParameter[2];
                p[0] = new SqlParameter("@ID", ID);
                p[1] = new SqlParameter("@LocationID", LocationID);
                _ds = (new DBHelper().GetDatasetFromSP)("sp_itemListselected_Vitamito", p);
                if (_ds != null)
                {
                    if (_ds.Tables.Count > 0)
                    {
                        if (_ds.Tables[0] != null)
                        {
                            lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<itemBLL>>().FirstOrDefault();
                        }
                        if (_ds.Tables[1] != null)
                        {
                            lstR = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[1])).ToObject<List<ReviewsBLL>>();
                        }

                        lst.Reviews = lstR;
                    }
                    //if (_dt.Rows.Count > 0)
                    //{
                    //    lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt)).ToObject<itemBLL>();
                    //}
                }
                return lst;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public List<itemBLL> GetAllFeatured()
        {
            try
            {
                var lst = new List<itemBLL>();
                _dt = (new DBHelper().GetTableFromSP)("sp_Featureditems");
                if (_dt != null)
                {
                    if (_dt.Rows.Count > 0)
                    {
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt)).ToObject<List<itemBLL>>();
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

        public List<itemBLL> GetAllPopular()
        {
            try
            {
                var lst = new List<itemBLL>();
                _dt = (new DBHelper().GetTableFromSP)("sp_PopularProducts");
                if (_dt != null)
                {
                    if (_dt.Rows.Count > 0)
                    {
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt)).ToObject<List<itemBLL>>();
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
        public List<itemBLL> GetAllValentineDay()
        {
            try
            {
                var lst = new List<itemBLL>();
                _dt = (new DBHelper().GetTableFromSP)("sp_ValentineDaySpecial");
                if (_dt != null)
                {
                    if (_dt.Rows.Count > 0)
                    {
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt)).ToObject<List<itemBLL>>();
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