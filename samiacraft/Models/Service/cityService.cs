using samiacraft.Models.Service;
using samiacraft.Models.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace samiacraft.Models.Service
{
    public class cityService : baseService
    {
        cityBLL _service;
        public cityService()
        {
            _service = new cityBLL();
        }
    }
}