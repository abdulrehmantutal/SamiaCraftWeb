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
    //public class categoryBLL
    //{
    //    public int CategoryID { get; set; }
    //    public string Title { get; set; }
    //    public string Image { get; set; }
    //    public string Description { get; set; }
    //    public Nullable<System.DateTime> CreationDate { get; set; }
    //    public int CreationID { get; set; }
    //    public Nullable<System.DateTime> UpdatedDate { get; set; }
    //    public int UpdatedID { get; set; }
    //    public int IsActive { get; set; }
    //    public string ArabicTitle { get; set; }
    //    public string CategoryType { get; set; }
    //    public int Row_Counter { get; set; }

    //    public static DataTable _dt;
    //    public static DataSet _ds;

    //    //public List<categoryBLL> GetAll()
    //    //{
    //    //    try
    //    //    {
    //    //        var lst = new List<categoryBLL>();
    //    //        _dt = (new DBHelper().GetTableFromSP)("sp_GetCategoryList");
    //    //        if (_dt != null)
    //    //        {
    //    //            if (_dt.Rows.Count > 0)
    //    //            {
    //    //                lst = _dt.DataTableToList<categoryBLL>();
    //    //            }
    //    //        }
    //    //        return lst;
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        return null;
    //    //    }
    //    //}
    //    public List<categoryBLL> GetAll()
    //    {
    //        try
    //        {
    //            var lst = new List<categoryBLL>();
    //            SqlParameter[] p = new SqlParameter[1];
    //            p[0] = new SqlParameter("@LocationID", 2148);
    //            _ds = (new DBHelper().GetDatasetFromSP)("sp_GetCategory_menu", p);
    //            if (_ds != null)
    //            {
    //                if (_ds.Tables.Count > 0)
    //                {
    //                    lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<categoryBLL>>().ToList();
    //                    //lst = _dt.DataTableToList<categoryBLL>();
    //                }
    //            }
    //            return lst;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }
    //}
        public enum Location
    {
        LocationID = 2195
    }
    public class categoryBLL
    {
        public int ID { get; set; }
        public Location LocationID { get; set; }
        public string Name { get; set; }
        public string ArabicName { get; set; }
        public string Description { get; set; }
        public string ArabicDescription { get; set; }
        public string Image { get; set; }
        public int? DisplayOrder { get; set; }
        //public bool SortByAlpha { get; set; }
        public string LastUpdatedBy { get; set; }
        public Nullable<System.DateTime> LastUpdatedDate { get; set; }
        public int? StatusID { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public string CreatedBy { get; set; }

        public static DataTable _dt;
        public static DataSet _ds;
        public List<categoryBLL> GetAll()
        {
            try
            {
                var lst = new List<categoryBLL>();
                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@LocationID", 2148);
                _ds = (new DBHelper().GetDatasetFromSP)("sp_GetCategory_menu", p);
                if (_ds != null)
                {
                    if (_ds.Tables.Count > 0)
                    {
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<categoryBLL>>().ToList();
                        //lst = _dt.DataTableToList<categoryBLL>();
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