using samiacraft.Models.Service;
using samiacraft.Models.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace samiacraft.Models.Service
{
    public class giftService : baseService
    {
        giftBLL _service;
        public giftService()
        {
            _service = new giftBLL();
        }

        public List<giftBLL> GetAll()
        {
            try
            {
                return _service.GetAll();
            }
            catch (Exception ex)
            {
                return new List<giftBLL>();
            }
        }
    }
}