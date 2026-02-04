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
   
    public class cityBLL
    {
        public int CityID { get; set; }
        public string CityName { get; set; }
        public string Title { get; set; }
        public string ArabicTitle { get; set; }
        public string StatusID { get; set; }
     

        public static DataTable _dt;
        public static DataSet _ds;
        public List<cityBLL> GetAll()
        {
            try
            {
                // Using mock data for development
                var mockService = new MockDataService();
                var mockCities = mockService.GetAllCities();
                
                // Convert mock data to cityBLL format
                var lst = mockCities.Select(c => new cityBLL 
                { 
                    CityID = c.CityID,
                    Title = c.Title,
                    CityName = c.Title,
                    ArabicTitle = c.ArabicTitle
                }).ToList();
                
                return lst;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
    }
}