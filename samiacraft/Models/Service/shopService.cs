using samiacraft.Models.Service;
using samiacraft.Models.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace samiacraft.Models.Service
{
    public class shopService : baseService
    {
        shopBLL _service;
        public shopService()
        {
            _service = new shopBLL();
        }

        public List<shopBLL> GetAll(string Category)
        {
            try
            {
                return _service.GetAll(Category);
            }
            catch (Exception ex)
            {
                return new List<shopBLL>();
            }
        }
        public List<shopBLL> BestProducts(int? cityID)
        {
            try
            {
                return _service.BestProducts(cityID);
            }
            catch (Exception ex)
            {
                return new List<shopBLL>();
            }
        }
    }
}