using samiacraft.Models;
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
    public class couponBLL
    {
        public int CouponID { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public double? Amount { get; set; }
        public string CouponCode { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public int? LastUpdatedBy { get; set; }
        public int? StatusID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public static DataSet _ds;
        public couponBLL Get(string coupon)
        {
            try
            {
                var obj = new couponBLL();
                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@coupon", coupon);
                _ds = (new DBHelper().GetDatasetFromSP)("sp_GetcouponDiscount", p);
                if (_ds != null)
                {
                    if (_ds.Tables.Count > 0)
                    {
                        if (_ds.Tables[0] != null)
                        {
                            obj = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<couponBLL>>().FirstOrDefault();
                        }
                    }
                }
                return obj;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}