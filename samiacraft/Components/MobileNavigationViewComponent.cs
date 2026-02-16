using Microsoft.AspNetCore.Mvc;
using samiacraft.Models.BLL;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace samiacraft.Components
{
    public class MobileNavigationViewComponent : ViewComponent
    {
        private readonly IConfiguration _configuration;
        public MobileNavigationViewComponent(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var navigationBLL = new navigationBLL();

                int LocationId = Convert.ToInt32(_configuration["LocationId"]);
                var categories = navigationBLL.GetSubCategory(LocationId);

                if (categories == null)
                {
                    categories = new List<navigationBLL>();
                }

                return View(categories);
            }
            catch (Exception ex)
            {
                return View(new List<navigationBLL>());
            }
        }
    }
}
