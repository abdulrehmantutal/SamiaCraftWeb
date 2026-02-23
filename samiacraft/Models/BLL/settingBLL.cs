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
    public class notificationBLL
    {
        public int StatusID { get; set; }
        public int NotificationID { get; set; }

        public string Title { get; set; }

        public string ButtonURL { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }
    }
    public class DynamicCssBLL
    {
        public int DynamicCssID { get; set; }
        public string Logo { get; set; }
        public string HeaderToparea { get; set; }
        public string BgWorkflow { get; set; }
        public string BgFeaturedProduct { get; set; }
        public string BgPopularProduct { get; set; }
        public string BgNewArrivals { get; set; }
        public string BgTestimonials { get; set; }
        public string BgNewsletter { get; set; }
        public string AddButton { get; set; }
    }
    public class settingBLL
    {
        public int SettingID { get; set; }
        public double DeliveryCharges { get; set; }
        public double ServiceCharges { get; set; }
        public double OtherCharges { get; set; }
        public double TaxPercentage { get; set; }
        public double MinimumOrderValue { get; set; }
        public double COD { get; set; }
        public int? IsDeliveryAllow { get; set; }
        public double Credimax { get; set; }
        public double PayPal { get; set; }
        public double BenefitPay { get; set; }
        public string TopHeaderText { get; set; }
        public string FacebookUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string ShopUrl { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
        public int? Facebook { get; set; }
        public int? Instagram { get; set; }
        public int? IsMaintenance { get; set; }
        public int? Twitter { get; set; }
        public List<notificationBLL> NotificationsList { get; set; }
        public List<DynamicCssBLL> DynamicList { get; set; }
        public static DataTable _dt;
        public static DataSet _ds;
        public settingBLL GetSettings(int userId)
        {
            try
            {
                var obj = new settingBLL();
                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@UserID", userId);

                _ds = (new DBHelper().GetDatasetFromSP)("sp_GetSettings_Vitamito", p);
                if (_ds != null)
                {
                    if (_ds.Tables.Count > 0)
                    {
                        if (_ds.Tables[0] != null)
                        {
                            obj = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<settingBLL>>().FirstOrDefault();
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