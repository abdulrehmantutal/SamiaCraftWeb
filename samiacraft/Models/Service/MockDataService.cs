using samiacraft.Models.BLL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace samiacraft.Models.Service
{
    /// <summary>
    /// MockDataService provides dummy/sample data for development and demonstration purposes.
    /// This service is structured to be easily replaceable with real API calls in the future.
    /// Simply inject this service in your controllers, and later replace it with a real service layer.
    /// </summary>
    public class MockDataService
    {
        public List<itemBLL> GetAllItems()
        {
            return new List<itemBLL>
            {
                new itemBLL
                {
                    ID = 1,
                    Name= "Elegant Cushion Cover",
                    ArabicName = "غطاء وسادة أنيق",
                    Description = "Beautiful handmade cushion cover with traditional patterns",
                    Price = 2500,
                    Image = "/images/product-1.jpg",
                    Stars = 5,
                    DisplayOrder = 1,
                    IsFeatured = true,
                    StatusID = 1
                },
            };
        }

        //public List<categoryBLL> GetAllCategories()
        //{
        //    return new List<categoryBLL>
        //    {
        //        new categoryBLL
        //        {
        //            CategoryID = 1,
        //            Title = "Home Decor",
        //            ArabicTitle = "ديكور المنزل",
        //            Description = "Beautiful home decoration items",
        //            Image = "/images/category-1.jpg",
        //            IsActive = 1,
        //            CreationDate = DateTime.Now,
        //            CreationID = 1,
        //            UpdatedDate = DateTime.Now,
        //            UpdatedID = 1
        //        },
        //        new categoryBLL
        //        {
        //            CategoryID = 2,
        //            Title = "Apparel",
        //            ArabicTitle = "الملابس",
        //            Description = "Traditional and modern clothing",
        //            Image = "/images/category-2.jpg",
        //            IsActive = 1,
        //            CreationDate = DateTime.Now,
        //            CreationID = 1,
        //            UpdatedDate = DateTime.Now,
        //            UpdatedID = 1
        //        },
        //        new categoryBLL
        //        {
        //            CategoryID = 3,
        //            Title = "Bedding",
        //            ArabicTitle = "الفراش",
        //            Description = "Premium bedding collections",
        //            Image = "/images/category-3.jpg",
        //            IsActive = 1,
        //            CreationDate = DateTime.Now,
        //            CreationID = 1,
        //            UpdatedDate = DateTime.Now,
        //            UpdatedID = 1
        //        },
        //        new categoryBLL
        //        {
        //            CategoryID = 4,
        //            Title = "Kitchenware",
        //            ArabicTitle = "أدوات المطبخ",
        //            Description = "Kitchen essentials and tools",
        //            Image = "/images/category-4.jpg",
        //            IsActive = 1,
        //            CreationDate = DateTime.Now,
        //            CreationID = 1,
        //            UpdatedDate = DateTime.Now,
        //            UpdatedID = 1
        //        },
        //        new categoryBLL
        //        {
        //            CategoryID = 5,
        //            Title = "Decor",
        //            ArabicTitle = "الديكور",
        //            Description = "Decorative pieces and accessories",
        //            Image = "/images/category-5.jpg",
        //            IsActive = 1,
        //            CreationDate = DateTime.Now,
        //            CreationID = 1,
        //            UpdatedDate = DateTime.Now,
        //            UpdatedID = 1
        //        }
        //    };
        //}

        public List<bannerBLL> GetAllBanners()
        {
            return new List<bannerBLL>
            {
                new bannerBLL
                {
                    BannerID = 1,
                    Title = "Welcome to Samia Crafts",
                    ArabicTitle = "مرحبا بك في سامية كرافتس",
                    MainHeading = "Discover Handmade Beauty",
                    ArabicMainHeading = "اكتشف الجمال المصنوع يدويا",
                    Description = "Explore our collection of traditional and modern handcrafted items",
                    ArabicDescription = "استكشف مجموعتنا من العناصر المصنوعة يدويا التقليدية والحديثة",
                    Image = "/images/banner-1.jpg",
                    StatusID = 1,
                    FormName = "Home",
                    DeviceType = "Desktop"
                },
                new bannerBLL
                {
                    BannerID = 2,
                    Title = "Summer Collection",
                    ArabicTitle = "مجموعة الصيف",
                    MainHeading = "Fresh Designs for Summer",
                    ArabicMainHeading = "تصاميم طازجة للصيف",
                    Description = "New summer collection with vibrant colors and patterns",
                    ArabicDescription = "مجموعة صيفية جديدة بألوان وأنماط نابضة بالحياة",
                    Image = "/images/banner-2.jpg",
                    StatusID = 1,
                    FormName = "Shop",
                    DeviceType = "Desktop"
                },
                new bannerBLL
                {
                    BannerID = 3,
                    Title = "Special Offer",
                    ArabicTitle = "عرض خاص",
                    MainHeading = "Up to 50% Off",
                    ArabicMainHeading = "خصم يصل إلى 50٪",
                    Description = "Limited time offer on selected items",
                    ArabicDescription = "عرض محدود الوقت على العناصر المختارة",
                    Image = "/images/banner-3.jpg",
                    StatusID = 1,
                    FormName = "Home",
                    DeviceType = "Mobile"
                }
            };
        }

        public List<dealBLL> GetAllDeals()
        {
            return new List<dealBLL>
            {
                new dealBLL
                {
                    DealID = 1,
                    ItemID = 1,
                    Title = "Flash Sale - Cushion Cover",
                    Description = "Limited time offer on cushion covers",
                    DealImage = "/images/deal-1.jpg",
                    DiscountedPrice = 1599,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"),
                    NumberOfDays = 7,
                    DisplayOrder = 1
                },
                new dealBLL
                {
                    DealID = 2,
                    ItemID = 6,
                    Title = "Super Deal - Silk Bedsheet",
                    Description = "Premium silk bedsheet at unbeatable price",
                    DealImage = "/images/deal-2.jpg",
                    DiscountedPrice = 3999,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(5).ToString("yyyy-MM-dd"),
                    NumberOfDays = 5,
                    DisplayOrder = 2
                },
                new dealBLL
                {
                    DealID = 3,
                    ItemID = 3,
                    Title = "Weekly Deal - Embroidered Shawl",
                    Description = "Beautiful shawl at special price",
                    DealImage = "/images/deal-3.jpg",
                    DiscountedPrice = 2899,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"),
                    NumberOfDays = 7,
                    DisplayOrder = 3
                }
            };
        }

        public List<cityBLL> GetAllCities()
        {
            return new List<cityBLL>
            {
                new cityBLL { CityID = 1, Title = "Karachi", ArabicTitle = "كراتشي" },
                new cityBLL { CityID = 2, Title = "Lahore", ArabicTitle = "لاهور" },
                new cityBLL { CityID = 3, Title = "Islamabad", ArabicTitle = "إسلام آباد" },
                new cityBLL { CityID = 4, Title = "Peshawar", ArabicTitle = "پشاور" },
                new cityBLL { CityID = 5, Title = "Quetta", ArabicTitle = "کویٹہ" },
                new cityBLL { CityID = 6, Title = "Faisalabad", ArabicTitle = "فیصل آباد" },
                new cityBLL { CityID = 7, Title = "Multan", ArabicTitle = "ملتان" },
                new cityBLL { CityID = 8, Title = "Hyderabad", ArabicTitle = "حیدرآباد" }
            };
        }

        public List<colorBLL> GetAllColors()
        {
            return new List<colorBLL>
            {
                new colorBLL { colorID = 1, Title = "Red" },
                new colorBLL { colorID = 2, Title = "Blue" },
                new colorBLL { colorID = 3, Title = "Green" },
                new colorBLL { colorID = 4, Title = "Gold" },
                new colorBLL { colorID = 5, Title = "Purple" },
                new colorBLL { colorID = 6, Title = "White" },
                new colorBLL { colorID = 7, Title = "Black" },
                new colorBLL { colorID = 8, Title = "Orange" },
                new colorBLL { colorID = 9, Title = "Cream" }
            };
        }

        public settingBLL GetSettings()
        {
            return new settingBLL
            {
                SettingID = 1,
                ShopUrl = "/Shop/Shop",
                DynamicList = new List<DynamicCssBLL>
                {
                    new DynamicCssBLL
                    {
                        DynamicCssID = 1,
                        HeaderToparea = "#1C3D5A",
                        Logo = "/images/logo.png",
                        AddButton = "#1C3D5A",
                        BgWorkflow = "/images/bg-workflow.jpg",
                        BgFeaturedProduct = "/images/bg-featured.jpg",
                        BgPopularProduct = "/images/bg-popular.jpg",
                        BgNewArrivals = "/images/bg-new-arrivals.jpg",
                        BgNewsletter = "/images/bg-newsletter.jpg",
                        BgTestimonials = "/images/bg-testimonials.jpg"
                    }
                }
            };
        }

        //public productBLL GetProductDetails(int itemId)
        //{
        //    var item = GetAllItems().FirstOrDefault(x => x.ID == itemId);
        //    if (item == null)
        //        return new productBLL();

        //    return new productBLL
        //    {
        //        ItemID = item.ID,
        //        Title = item.Name,
        //        ArabicTitle = item.ArabicName,
        //        Description = item.Description,
        //        Image = item.Image,
        //        Price = item.Price,
        //        Stars = item.Stars,
        //        StatusID = item.StatusID,
        //        ImgList = new List<productBLL.ItemImages>
        //        {
        //            new productBLL.ItemImages { Image = item.Image, Row_Counter = 1 },
        //        },
        //        Reviews = new List<productBLL.ReviewsBLL>
        //        {
        //            new productBLL.ReviewsBLL
        //            {
        //                ReviewID = 1,
        //                Name = "Ahmed Khan",
        //                Email = "ahmed@example.com",
        //                Stars = 5,
        //                Description = "Excellent quality! Highly recommended.",
        //                StatusID = 1,
        //                ItemID = itemId
        //            },
        //            new productBLL.ReviewsBLL
        //            {
        //                ReviewID = 2,
        //                Name = "Fatima Ali",
        //                Email = "fatima@example.com",
        //                Stars = 4,
        //                Description = "Very good product, fast delivery.",
        //                StatusID = 1,
        //                ItemID = itemId
        //            },
        //            new productBLL.ReviewsBLL
        //            {
        //                ReviewID = 3,
        //                Name = "Muhammad Hassan",
        //                Email = "hassan@example.com",
        //                Stars = 5,
        //                Description = "Amazing craftsmanship and design!",
        //                StatusID = 1,
        //                ItemID = itemId
        //            }
        //        }
        //    };
        //}

        public List<itemBLL> GetFeaturedItems()
        {
            return GetAllItems().Where(x => x.IsFeatured == true).ToList();
        }

        public List<itemBLL> GetNewArrivals()
        {
            return GetAllItems().OrderByDescending(x => x.ID).Take(4).ToList();
        }

        public List<itemBLL> GetPopularProducts()
        {
            return GetAllItems().OrderBy(x => x.Stars).Reverse().Take(4).ToList();
        }
    }
}
