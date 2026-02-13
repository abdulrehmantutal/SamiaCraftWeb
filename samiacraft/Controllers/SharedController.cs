using samiacraft.Models.BLL;
using samiacraft.Models.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace samiacraft.Controllers
{
    public class SharedController : Controller
    {
        private readonly IConfiguration _configuration;
        public SharedController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ActionResult Header()
        {
            int LocationId = Convert.ToInt32(_configuration["LocationId"]);
            return PartialView("Header", new navigationBLL().GetSubCategory(LocationId));
        }
        public ActionResult MobileNavigation()
        {
            int LocationId = Convert.ToInt32(_configuration["LocationId"]);
            return PartialView("MobileNavigation", new navigationBLL().GetSubCategory(LocationId));
        }
    }
}