using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using samiacraft.Models.BLL;
using samiacraft.Models.Service;
using System.Net.Mail;

namespace samiacraft.Controllers
{
    /// <summary>
    /// HomeController handles main website pages including home, about, contact, and settings.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        // Configuration keys
        private const string LocationIdConfigKey = "LocationId";
        private const string MaintenanceTemplatePath = "Template/contact.txt";
        private const string NewsletterTemplatePath = "Template/newsletter.txt";
        private const string DateReplacementKey = "#Date#";
        private const string NameReplacementKey = "#Name#";
        private const string EmailReplacementKey = "#Email#";
        private const string ContactReplacementKey = "#Contact#";
        private const string SubjectReplacementKey = "#Subject#";
        private const string MessageReplacementKey = "#Message#";
        private const string NewsletterEmailReplacementKey = "#email#";

        public HomeController(
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            ILogger<HomeController> logger)
        {
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the home page with featured products, categories, deals, and banners.
        /// </summary>
        public ActionResult Index()
        {
            try
            {
                var settings = new settingBLL().GetSettings();

                // Check maintenance mode before processing
                if (settings == null || settings.IsMaintenance == 1)
                {
                    return RedirectToAction(nameof(Maintenance));
                }

                int locationId = GetLocationId();
                InitializeDefaultViewBagValues();
                PopulateThemeSettings(settings);
                PopulateLocationAndCityData();
                PopulateProductData(locationId, settings);
                PopulateDealsAndBanners();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                return View();
            }
        }

        /// <summary>
        /// Extracts location ID from configuration with proper type conversion.
        /// </summary>
        private int GetLocationId()
        {
            var locationIdValue = _configuration[LocationIdConfigKey];
            return int.TryParse(locationIdValue, out int locationId) ? locationId : 0;
        }

        /// <summary>
        /// Initializes ViewBag with empty default collections.
        /// </summary>
        private void InitializeDefaultViewBagValues()
        {
            ViewBag.CategoryList = new List<categoryBLL>();
            ViewBag.Category = new List<categoryBLL>();
            ViewBag.ItemList = new List<itemBLL>();
            ViewBag.FeaturedItems = new List<itemBLL>();
            ViewBag.NewArrivals = new List<itemBLL>();
            ViewBag.TenItems = new List<itemBLL>();
            ViewBag.PopularProducts = new List<itemBLL>();
            ViewBag.Deal = new List<dealBLL>();
            ViewBag.Banner = new List<bannerBLL>();
            ViewBag.Reviews = new List<reviewBLL>();
        }

        /// <summary>
        /// Populates theme and styling settings from the database.
        /// </summary>
        private void PopulateThemeSettings(settingBLL settings)
        {
            var dynamicSettings = settings?.DynamicList?.FirstOrDefault();

            const string defaultColor = "#1C3D5A";
            ViewBag.TopheaderArea = defaultColor;
            ViewBag.AddButton = defaultColor;
            ViewBag.BgWorkflow = dynamicSettings?.BgWorkflow ?? string.Empty;
            ViewBag.BgFeaturedProduct = dynamicSettings?.BgFeaturedProduct ?? string.Empty;
            ViewBag.BgPopularProduct = dynamicSettings?.BgPopularProduct ?? string.Empty;
            ViewBag.BgNewArrivals = dynamicSettings?.BgNewArrivals ?? string.Empty;
            ViewBag.BgNewsletter = dynamicSettings?.BgNewsletter ?? string.Empty;
            ViewBag.BgTestimonials = dynamicSettings?.BgTestimonials ?? string.Empty;
            ViewBag.ShopButtonURL = settings?.ShopUrl ?? "#";
            ViewBag.Logo = dynamicSettings?.Logo ?? string.Empty;
        }

        /// <summary>
        /// Loads city data and populates ViewBag.
        /// </summary>
        private void PopulateLocationAndCityData()
        {
            var cities = new cityService().GetAll();
            ViewBag.City = cities ?? new List<cityBLL>();
        }

        /// <summary>
        /// Loads products, categories, and related data from the database.
        /// </summary>
        private void PopulateProductData(int locationId, settingBLL settings)
        {
            if (locationId <= 0)
            {
                _logger.LogWarning("Invalid location ID: {LocationId}", locationId);
                return;
            }

            var items = new itemService().GetAll(locationId);
            if (items?.Count > 0)
            {
                PopulateItemCollections(items);
            }

            var categories = new categoryBLL().GetAll();
            PopulateCategories(categories);
        }

        /// <summary>
        /// Organizes items into different collections (featured, new arrivals, etc.).
        /// </summary>
        private void PopulateItemCollections(List<itemBLL> items)
        {
            ViewBag.ItemList = items
                .Where(x => x.DisplayOrder > 0)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            ViewBag.FeaturedItems = items
                .Where(x => x.IsFeatured == true)
                .OrderByDescending(x => x.DisplayOrder)
                .OrderBy(c => Guid.NewGuid())
                .Take(8)
                .ToList();

            ViewBag.NewArrivals = items
                .OrderByDescending(c => c.LastUpdatedDate)
                .Take(8)
                .ToList();

            ViewBag.TenItems = items.Take(40).ToList();
        }

        /// <summary>
        /// Populates category collections into ViewBag.
        /// </summary>
        private void PopulateCategories(List<categoryBLL> categories)
        {
            if (categories?.Count > 0)
            {
                ViewBag.CategoryList = categories.Take(6).ToList();
                ViewBag.Category = categories.ToList();
            }
            else
            {
                ViewBag.Category = new List<categoryBLL>();
            }
        }

        /// <summary>
        /// Loads deals, banners, and customer reviews.
        /// </summary>
        private void PopulateDealsAndBanners()
        {
            var bannerService = new bannerBLL();
            ViewBag.Deal = new dealBLL().GetAll();
            ViewBag.Banner = bannerService.GetBanner("Home");
            ViewBag.Reviews = bannerService.GetReviews();
        }

        /// <summary>
        /// Displays the maintenance page.
        /// </summary>
        public ActionResult Maintenance()
        {
            var cities = new cityService().GetAll();
            ViewBag.City = cities ?? new List<cityBLL>();
            return View();
        }

        /// <summary>
        /// Displays the about page with company information and banners.
        /// </summary>
        public ActionResult About()
        {
            var cities = new cityService().GetAll();
            ViewBag.City = cities ?? new List<cityBLL>();
            ViewBag.Banner = new bannerBLL().GetBanner("About");
            return View();
        }

        /// <summary>
        /// Displays the contact form (GET).
        /// </summary>
        [HttpGet]
        public ActionResult Contact()
        {
            var cities = new cityService().GetAll();
            ViewBag.City = cities ?? new List<cityBLL>();
            ViewBag.Banner = new bannerBLL().GetBanner("Contact");
            return View();
        }

        /// <summary>
        /// Handles contact form submission and sends email to admin (POST).
        /// </summary>
        [HttpPost]
        public ActionResult Contact(contactBLL contactInfo)
        {
            try
            {
                var cities = new cityService().GetAll();
                ViewBag.City = cities ?? new List<cityBLL>();

                if (contactInfo == null)
                {
                    ViewBag.Contact = "Invalid contact information provided.";
                    return View();
                }

                ViewBag.Contact = SendContactEmail(contactInfo)
                    ? "Your Query is received. Our support department will contact you soon."
                    : "Error sending message. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing contact form submission");
                ViewBag.Contact = "Error sending message. Please try again later.";
            }

            return View();
        }

        /// <summary>
        /// Sends contact form email to the admin.
        /// </summary>
        private bool SendContactEmail(contactBLL contactInfo)
        {
            string templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, MaintenanceTemplatePath);

            if (!System.IO.File.Exists(templatePath))
            {
                _logger.LogWarning("Contact template file not found at {TemplatePath}", templatePath);
                return false;
            }

            try
            {
                string emailBody = System.IO.File.ReadAllText(templatePath);
                emailBody = ReplaceContactEmailPlaceholders(emailBody, contactInfo);

                SendEmail(
                    _configuration["AppSettings:From"] ?? "",
                    "New Query From Customer",
                    emailBody);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact email");
                return false;
            }
        }

        /// <summary>
        /// Replaces email template placeholders with contact information.
        /// </summary>
        private string ReplaceContactEmailPlaceholders(string emailTemplate, contactBLL contactInfo)
        {
            return emailTemplate
                .Replace(DateReplacementKey, DateTime.UtcNow.Date.ToString("dd/MMM/yyyy"))
                .Replace(NameReplacementKey, contactInfo.Name ?? string.Empty)
                .Replace(EmailReplacementKey, contactInfo.Email ?? string.Empty)
                .Replace(ContactReplacementKey, contactInfo.Phone ?? string.Empty)
                .Replace(SubjectReplacementKey, contactInfo.Subject ?? string.Empty)
                .Replace(MessageReplacementKey, contactInfo.Message ?? string.Empty);
        }

        /// <summary>
        /// Handles newsletter subscription via AJAX.
        /// </summary>
        public JsonResult Subscribe(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, message = "Email is required" });
            }

            try
            {
                string templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, NewsletterTemplatePath);

                if (!System.IO.File.Exists(templatePath))
                {
                    _logger.LogWarning("Newsletter template file not found at {TemplatePath}", templatePath);
                    return Json(new { success = false, message = "Template not found" });
                }

                string emailBody = System.IO.File.ReadAllText(templatePath);
                emailBody = emailBody.Replace(NewsletterEmailReplacementKey, email);

                SendEmail(
                    _configuration["AppSettings:From"] ?? "",
                    "New Subscription at Al Hilal",
                    emailBody);

                return Json(new { success = true, message = "Successfully subscribed!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Newsletter subscription failed for email {Email}", email);
                return Json(new { success = false, message = "Subscription failed" });
            }
        }

        /// <summary>
        /// Retrieves application settings as JSON.
        /// </summary>
        public ActionResult GetSetting()
        {
            return Json(new settingBLL().GetSettings());
        }

        /// <summary>
        /// Displays the privacy policy page.
        /// </summary>
        public ActionResult Policy()
        {
            var cities = new cityService().GetAll();
            ViewBag.City = cities ?? new List<cityBLL>();
            return View();
        }

        /// <summary>
        /// Sends an email using configured SMTP settings.
        /// </summary>
        private void SendEmail(string toAddress, string subject, string body)
        {
            using (var mail = new MailMessage())
            {
                mail.To.Add(toAddress);
                mail.From = new MailAddress(_configuration["AppSettings:From"] ?? "");
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Port = int.TryParse(_configuration["AppSettings:SmtpPort"], out int port) ? port : 587;
                    smtp.Host = _configuration["AppSettings:SmtpServer"] ?? "";
                    smtp.Credentials = new System.Net.NetworkCredential(
                        _configuration["AppSettings:From"] ?? "",
                        _configuration["AppSettings:Password"] ?? "");
                    smtp.EnableSsl = true;

                    smtp.Send(mail);
                }
            }
        }
    }
}