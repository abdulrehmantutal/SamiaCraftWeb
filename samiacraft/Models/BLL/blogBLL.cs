using Newtonsoft.Json.Linq;
using samiacraft.Helpers;
using System.Data;
using System.Data.SqlClient;

namespace samiacraft.Models.BLL
{
    public class blogBLL
    {
        public int BlogID { get; set; }
        public int? BlogCategoryID { get; set; }
        public int? LocationID { get; set; }
        public string Label { get; set; }
        public string BCName { get; set; }
        public string BCArName { get; set; }
        public string Title { get; set; }
        public string ArabicTitle { get; set; }
        public string Description { get; set; }
        public string ArabicDescription { get; set; }
        public string ImageSmall { get; set; }
        public string ImageLarge { get; set; }

        public int? DisplayOrder { get; set; }
        public bool? IsPublish { get; set; }

        public Nullable<System.DateTime> PostedOn { get; set; }
        public int? StatusID { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }

        public static DataTable _dt;
        public static DataSet _ds;

        public List<blogBLL> GetAll(int LocationID)
        {
            try
            {
                var lst = new List<blogBLL>();
                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@LocationID", LocationID);
                _dt = (new DBHelper().GetTableFromSP)("sp_GetBlogs_Vitamito", p);

                if (_dt != null)
                {
                    if (_dt.Rows.Count > 0)
                    {
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt)).ToObject<List<blogBLL>>();
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
        public blogBLL GetByID(int BlogID, int LocationID)
        {
            try
            {
                var lst = new blogBLL();
                SqlParameter[] p = new SqlParameter[2];
                p[0] = new SqlParameter("@ID", BlogID);
                p[1] = new SqlParameter("@LocationID", LocationID);
                _dt = (new DBHelper().GetTableFromSP)("sp_BlogDetailVitamito", p);

                if (_dt != null)
                {
                    if (_dt.Rows.Count > 0)
                    {
                        //lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt)).ToObject<blogBLL>();
                        lst = _dt.DataTableToList<blogBLL>().FirstOrDefault();
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<blogBLL> GetAllRelated(int BlogID)
        {
            try
            {
                var obj = new List<blogBLL>();
                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@ID", BlogID);
                _dt = (new DBHelper().GetTableFromSP)("sp_GetRelatedBlog_Vitamito", p);
                if (_dt.Rows.Count > 0)
                {
                    obj = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_dt)).ToObject<List<blogBLL>>();
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
