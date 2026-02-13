using samiacraft.Models.Service;
using samiacraft.Models.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace samiacraft.Models.Service
{
    public class productService : baseService
    {
        productBLL _service;
        public productService()
        {
            _service = new productBLL();
        }

        public productBLL GetAll(int ItemID, int LocationID)
        {
            try
            {
                return _service.GetAll(ItemID, LocationID);
            }
            catch (Exception ex)
            {
                return new productBLL();
            }
        }

    }
}