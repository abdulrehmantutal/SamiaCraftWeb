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
    public class colorBLL
    {
        public int colorID { get; set; }
        public string Title { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public int CreationID { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public int UpdatedID { get; set; }

        public static DataTable _dt;
        public List<colorBLL> GetAll()
        {
            try
            {
                // Using mock data for development
                var mockService = new MockDataService();
                return mockService.GetAllColors();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}