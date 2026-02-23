using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using samiacraft.Models.BLL;
using samiacraft.Models.Service;
using System.IO;
using System.Net.Mail;

namespace samiacraft.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public HomeController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        public ActionResult Index()
        {
            int LocationId = Convert.ToInt32(_configuration["LocationId"]);
            int UserId = Convert.ToInt32(_configuration["UserId"]);

            var settng = new settingBLL().GetSettings(UserId);
            var dyn = settng?.DynamicList?.FirstOrDefault();


            ViewBag.TopheaderArea = "#1C3D5A";
            //dyn?.HeaderToparea ?? string.Empty;
            ViewBag.AddButton = "#1C3D5A";
            //dyn?.AddButton ?? string.Empty;
            ViewBag.BgWorkflow = dyn?.BgWorkflow ?? string.Empty;
            ViewBag.BgFeaturedProduct = dyn?.BgFeaturedProduct ?? string.Empty;
            ViewBag.BgPopularProduct = dyn?.BgPopularProduct ?? string.Empty;
            ViewBag.BgNewArrivals = dyn?.BgNewArrivals ?? string.Empty;
            ViewBag.BgNewsletter = dyn?.BgNewsletter ?? string.Empty;
            ViewBag.BgTestimonials = dyn?.BgTestimonials ?? string.Empty;
            ViewBag.shopButtonURL = settng?.ShopUrl ?? "#";
            ViewBag.Logo = dyn?.Logo ?? string.Empty;
            
            ViewBag.categoryList = new List<categoryBLL>();
            ViewBag.Category = new List<categoryBLL>();
            ViewBag.itemList = new List<itemBLL>();
            ViewBag.Featureditems = new List<itemBLL>();
            ViewBag.NewArrivals = new List<itemBLL>();
            ViewBag.TenItems = new List<itemBLL>();
            ViewBag.PopularProducts = new List<itemBLL>();
            ViewBag.Deal = new List<dealBLL>();
            ViewBag.Banner = new List<bannerBLL>();
            ViewBag.Reviews = new List<reviewBLL>();
            
            if (settng == null)
            {
                return View();
            }
            ViewBag.shopButtonURL = settng.ShopUrl;
            if (settng.IsMaintenance == 1)
            {
                return RedirectToAction("Maintenance");
            }
            else
            {
                var itemData = new itemService().GetAll((int)LocationId);
                if (itemData != null)
                {
                    ViewBag.itemList = itemData.Where(x => x.DisplayOrder > 0).OrderBy(x => x.DisplayOrder).ToList();
                    ViewBag.Featureditems = itemData.OrderByDescending(x => x.DisplayOrder).Where(x => x.IsFeatured == true).OrderBy(c => Guid.NewGuid()).Take(8).ToList();
                    ViewBag.NewArrivals = itemData.OrderByDescending(c => c.LastUpdatedDate).Take(8).ToList();
                    ViewBag.TenItems = itemData.Take(40).ToList();
                }

                var catlist = new categoryBLL().GetAll(LocationId);
                if (catlist != null && catlist.Count > 0)
                {
                    ViewBag.categoryList = catlist.ToList();
                    ViewBag.Category = catlist.ToList();
                }
                else
                {
                    ViewBag.Category = new List<categoryBLL>();
                }

                //ViewBag.Deal = new dealBLL().GetAll();
                ViewBag.Banner = new bannerBLL().GetBannerHeader(LocationId);
                ViewBag.Reviews = new bannerBLL().GetReviews();
            }
            return View();
        }

        public ActionResult Maintenance()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.ImageUrl = _configuration["Image"] ?? "https://retail.premium-pos.com";
            //ViewBag.Banner = new bannerBLL().GetBanner("About");
            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.ImageUrl = _configuration["Image"] ?? "https://retail.premium-pos.com";
            //ViewBag.Banner = new bannerBLL().GetBanner("Contact");
            return View();
        }

        [HttpPost]
        public ActionResult Contact(contactBLL obj)
        {
            ViewBag.ImageUrl = _configuration["Image"] ?? "https://retail.premium-pos.com";
            ViewBag.Contact = "";
            
            try
            {
                string ToEmail = _configuration["AppSettings:From"] ?? "";
                string SubJect = "New Query From Customer";
                string templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Template", "contact.txt");
                
                if (!System.IO.File.Exists(templatePath))
                {
                    ViewBag.Contact = "Template file not found.";
                    return View();
                }
                
                string BodyEmail = System.IO.File.ReadAllText(templatePath);
                DateTime dateTime = DateTime.UtcNow.Date;
                
                BodyEmail = BodyEmail.Replace("#Date#", dateTime.ToString("dd/MMM/yyyy"))
                    .Replace("#Name#", obj.Name?.ToString() ?? "")
                    .Replace("#Email#", obj.Email?.ToString() ?? "")
                    .Replace("#Contact#", obj.Phone?.ToString() ?? "")
                    .Replace("#Subject#", obj.Subject?.ToString() ?? "")
                    .Replace("#Message#", obj.Message?.ToString() ?? "");

                MailMessage mail = new MailMessage();
                mail.To.Add(ToEmail);
                mail.From = new MailAddress(_configuration["AppSettings:From"] ?? "");
                mail.Subject = SubJect;
                mail.Body = BodyEmail;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.UseDefaultCredentials = false;
                smtp.Port = int.Parse(_configuration["AppSettings:SmtpPort"] ?? "587");
                smtp.Host = _configuration["AppSettings:SmtpServer"] ?? "";
                smtp.Credentials = new System.Net.NetworkCredential(
                    _configuration["AppSettings:From"] ?? "",
                    _configuration["AppSettings:Password"] ?? "");
                smtp.EnableSsl = true;
                smtp.Send(mail);
                
                ViewBag.Contact = "Your Query is received. Our support department will contact you soon.";
            }
            catch (Exception ex)
            {
                ViewBag.Contact = "Error sending message. Please try again later.";
            }
            
            return View();
        }

        public JsonResult Subscribe(string email)
        {
            try
            {
                string ToEmail = _configuration["AppSettings:From"] ?? "";
                string SubJect = "New Subscription at Al Hilal";
                string templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Template", "newsletter.txt");
                
                if (!System.IO.File.Exists(templatePath))
                {
                    return Json(new { success = false, message = "Template not found" });
                }
                
                string BodyEmail = System.IO.File.ReadAllText(templatePath);
                BodyEmail = BodyEmail.Replace("#email#", email?.ToString() ?? "");

                MailMessage mail = new MailMessage();
                mail.To.Add(ToEmail);
                mail.From = new MailAddress(_configuration["AppSettings:From"] ?? "");
                mail.Subject = SubJect;
                mail.Body = BodyEmail;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.UseDefaultCredentials = false;
                smtp.Port = int.Parse(_configuration["AppSettings:SmtpPort"] ?? "587");
                smtp.Host = _configuration["AppSettings:SmtpServer"] ?? "";
                smtp.Credentials = new System.Net.NetworkCredential(
                    _configuration["AppSettings:From"] ?? "",
                    _configuration["AppSettings:Password"] ?? "");
                smtp.EnableSsl = true;
                smtp.Send(mail);
                
                return Json(new { success = true, message = "Successfully subscribed!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Subscription failed" });
            }
        }

        public ActionResult GetSetting()
        {
            int UserId = Convert.ToInt32(_configuration["UserId"]);
            return Json(new settingBLL().GetSettings(UserId));
        }

        public ActionResult Policy()
        {
            return View();
        }
    }
}