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
    }
}