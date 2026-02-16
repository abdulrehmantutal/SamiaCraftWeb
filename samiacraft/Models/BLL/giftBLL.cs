using samiacraft.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using samiacraft.Helpers;

namespace samiacraft.Models.BLL
{
    public class giftBLL
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string ArabicName { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Barcode { get; set; }
        public string SKU { get; set; }
        public double? Price { get; set; }
        public int? StatusID { get; set; }
        public int? DisplayOrder { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public int? LastUpdatedBy { get; set; }
        public int? UserID { get; set; }
        public string Image { get; set; }

        public static DataTable _dt;

        public List<giftBLL> GetAll(int userId)
        {
            try
            {
                var lst = new List<giftBLL>();

                var parameters = new Dictionary<string, object>();
                parameters.Add("@UserId", userId);

                _dt = (new DBHelper().GetTableFromSP)("sp_GiftList", parameters);

                if (_dt != null && _dt.Rows.Count > 0)
                {
                    lst = JArray
                        .Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt))
                        .ToObject<List<giftBLL>>();
                }

                return lst;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
