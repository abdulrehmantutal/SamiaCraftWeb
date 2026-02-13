using Newtonsoft.Json.Linq;
using samiacraft.Helpers;
using System.Data;
using System.Data.SqlClient;

namespace samiacraft.Models.BLL
{
    public class blogCategoryBLL
    {
        public int BlogCategoryID { get; set; }
        public Location LocationID { get; set; }
        public string Name { get; set; }
        public string ArabicName { get; set; }
        public string Description { get; set; }
        public string ArabicDescription { get; set; }
        public string Image { get; set; }
        public int? DisplayOrder { get; set; }

        public int? StatusID { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }

        public static DataTable _dt;
        public static DataSet _ds;
        public List<blogCategoryBLL> GetAll(int LocationID)
        {
            try
            {
                var lst = new List<blogCategoryBLL>();
                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@LocationID", LocationID);
                _ds = (new DBHelper().GetDatasetFromSP)("sp_GetBlogCategory_menu", p);
                if (_ds != null)
                {
                    if (_ds.Tables.Count > 0)
                    {
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<blogCategoryBLL>>().ToList();
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
