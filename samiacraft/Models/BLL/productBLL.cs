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
    //public class productBLL
    //{
    //    public int ItemID { get; set; }
    //    public int? Stars { get; set; }
    //    public string Title { get; set; }
    //    public string Description { get; set; }
    //    public string Image { get; set; }
    //    public string ArabicTitle { get; set; }
    //    public double? Cost { get; set; }
    //    public double? Price { get; set; }
    //    public double? DiscountedPrice { get; set; }
    //    public double? DoublePrice { get; set; }
    //    public bool? InStock { get; set; }
    //    public int? StatusID { get; set; }
    //    public int? StockQty { get; set; }
    //    public bool? IsDoubleQty { get; set; }
    //    public List<ItemImages> ImgList = new List<ItemImages>();
    //    public List<ReviewsBLL> Reviews= new List<ReviewsBLL>();

    //    public class ItemImages
    //    {
    //        public string Image { get; set; }
    //        public int Row_Counter { get; set; }
    //    }
    //    public class ReviewsBLL
    //    {
    //        public int ReviewID { get; set; }

    //        public int? Stars { get; set; }

    //        public string Name { get; set; }

    //        public string Description { get; set; }

    //        public string Email { get; set; }

    //        public string Contact { get; set; }

    //        public int? StatusID { get; set; }

    //        public int? ItemID { get; set; }
    //    }


    //    public static DataTable _dt;
    //    public static DataSet _ds;
    //    public productBLL GetAll(int ItemID)
    //    {
    //        try
    //        {
    //            // Using mock data for development
    //            var mockService = new MockDataService();
    //            return mockService.GetProductDetails(ItemID);
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }

    //    public int InsertProductReview(ReviewsBLL data)
    //    {

    //        try
    //        {
    //            SqlParameter[] p = new SqlParameter[7];
    //            p[0] = new SqlParameter("@Name", data.Name);
    //            p[1] = new SqlParameter("@Description", data.Description);
    //            p[2] = new SqlParameter("@Contact", data.Contact);
    //            p[3] = new SqlParameter("@Email", data.Email);
    //            p[4] = new SqlParameter("@Stars", data.Stars);
    //            p[5] = new SqlParameter("@StatusID", data.StatusID);
    //            p[6] = new SqlParameter("@ItemID", data.ItemID);
    //            return int.Parse(new DBHelper().GetTableFromSP("sp_InsertReview", p).Rows[0]["ID"].ToString());

    //        }
    //        catch (Exception ex)
    //        {
    //            return 0;
    //        }
    //    }
    //}
    public class productBLL
    {
        public int ID { get; set; }
        public int? Stars { get; set; }
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
        public double? CurrentStock { get; set; }

        public bool? IsStockOut { get; set; }

        public List<ItemImages> ImgList = new List<ItemImages>();

        public List<ReviewsBLL> Reviews = new List<ReviewsBLL>();

        public class ItemImages
        {
            public string Image { get; set; }
            //public int Row_Counter { get; set; }
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

        public productBLL GetAll(int ID, int LocationID)
        {
            try
            {
                var obj = new productBLL();
                List<ItemImages> lstIM = new List<ItemImages>();
                List<ReviewsBLL> lstR = new List<ReviewsBLL>();
                SqlParameter[] p = new SqlParameter[2];
                p[0] = new SqlParameter("@ID", ID);
                p[1] = new SqlParameter("@LocationID", LocationID);
                _ds = (new DBHelper().GetDatasetFromSP)("sp_ProductVitamito_V2", p);
                if (_ds != null)
                {
                    if (_ds.Tables.Count > 0)
                    {
                        if (_ds.Tables[0] != null)
                        {
                            obj = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<productBLL>>().FirstOrDefault();
                        }

                        if (_ds.Tables[1] != null)
                        {
                            lstIM = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[1])).ToObject<List<ItemImages>>().ToList();
                        }

                        if (_ds.Tables[2] != null)
                        {
                            lstR = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[2])).ToObject<List<ReviewsBLL>>();
                        }
                        obj.ImgList = lstIM;
                        obj.Reviews = lstR;
                    }
                }
                return obj;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<productBLL> GetAllRelated(int ID)
        {
            try
            {
                var obj = new List<productBLL>();
                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@ID", ID);
                _dt = (new DBHelper().GetTableFromSP)("sp_GetRelatedProducts_Vitamito", p);
                if (_ds.Tables[0] != null)
                {
                    obj = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt)).ToObject<List<productBLL>>();
                }
                return obj;
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
                SqlParameter[] p = new SqlParameter[8];
                p[0] = new SqlParameter("@Name", data.Name);
                p[1] = new SqlParameter("@Description", data.Description);
                p[2] = new SqlParameter("@Contact", data.Contact);
                p[3] = new SqlParameter("@Email", data.Email);
                p[4] = new SqlParameter("@Stars", data.Stars);
                p[5] = new SqlParameter("@StatusID", 1);
                p[6] = new SqlParameter("@ItemID", data.ItemID);
                p[7] = new SqlParameter("@LocationID", 2195);

                return int.Parse(new DBHelper().GetTableFromSP("sp_InsertReview", p).Rows[0]["ID"].ToString());

            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        //public productBLL GetAll(int ID)
        //{
        //    try
        //    {
        //        var obj = new productBLL();
        //        List<ItemImages> lstIM = new List<ItemImages>();
        //        //List<ReviewsBLL> lstR = new List<ReviewsBLL>();
        //        SqlParameter[] p = new SqlParameter[1];
        //        p[0] = new SqlParameter("@ID", ID);
        //        _ds = (new DBHelper().GetDatasetFromSP)("sp_ProductVitamito", p);
        //        if (_ds != null)
        //        {
        //            if (_ds.Tables.Count > 0)
        //            {
        //                if (_ds.Tables[0] != null)
        //                {
        //                    obj = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<productBLL>>().FirstOrDefault();
        //                }
        //                if (_ds.Tables[1] != null)
        //                {
        //                    lstIM = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[1])).ToObject<List<ItemImages>>();
        //                }
        //                //  if (_ds.Tables[2] != null)
        //                //   {
        //                //      lstR = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[2])).ToObject<List<ReviewsBLL>>();
        //                // }
        //                obj.ImgList = lstIM;
        //                //obj.Reviews = lstR;
        //            }
        //        }
        //        return obj;

        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

    }
}