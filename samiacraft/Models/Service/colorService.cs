using samiacraft.Models.Service;
using samiacraft.Models.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace samiacraft.Models.Service
{
    public class colorService : baseService
    {
        colorBLL _service;
        public colorService()
        {
            _service = new colorBLL();
        }
    }
}