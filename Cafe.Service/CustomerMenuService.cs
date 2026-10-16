using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;

namespace Cafe.Service
{
    public class CustomerMenuService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly SettingService _settingService;

        public CustomerMenuService(
            ApplicationDbContext dbContext,
            ProductService productService,
            CategoryService categoryService,
            SettingService settingService)
        {
            _dbContext = dbContext;
            _productService = productService;
            _categoryService = categoryService;
            _settingService = settingService;
        }

        /// <summary>
        /// Retrieves aggregated menu data for the customer menu page, including active categories,
        /// customer-visible products (Active and Out-of-Stock, excluding Inactive), featured products,
        /// and operational settings.
        /// </summary>
        public async Task<CustomerMenuDto> GetCustomerMenuAsync(string? categoryId = null, string? search = null)
        {
            // 1. Fetch Active Categories
            var categories = await _categoryService.GetAllCategoriesAsync("Active");

            // 2. Fetch Customer Products (Active + Out of Stock, Inactive excluded)
            var products = await _productService.GetCustomerMenuProductsAsync(categoryId, search);

            // 3. Featured Products
            var featured = products.Where(p => p.IsFeatured && p.IsAvailable).Take(4).ToList();
            if (featured.Count == 0 && products.Count > 0)
            {
                featured = products.Where(p => p.IsAvailable).Take(4).ToList();
            }

            // 4. Cafe Settings & Operating Status
            var settings = await _settingService.GetSettingsAsync();
            var (isOpen, statusText) = EvaluateOpenStatus(settings.Hours);
            var currencySymbol = GetCurrencySymbol(settings.Financial?.Currency);

            return new CustomerMenuDto
            {
                Categories = categories,
                Products = products,
                FeaturedProducts = featured,
                TotalItems = products.Count,
                CurrencySymbol = currencySymbol,
                GeneralSettings = settings.General ?? new GeneralSettingsDto(),
                OperatingStatusText = statusText,
                IsOpen = isOpen
            };
        }

        public async Task<List<ProductDto>> GetCustomerProductsAsync(string? categoryId = null, string? search = null)
        {
            return await _productService.GetCustomerMenuProductsAsync(categoryId, search);
        }

        public async Task<ProductDto?> GetCustomerProductByIdAsync(int id)
        {
            return await _productService.GetProductByIdAsync(id);
        }

        public async Task<List<CategoryDto>> GetCustomerCategoriesAsync()
        {
            return await _categoryService.GetAllCategoriesAsync("Active");
        }

        private static (bool IsOpen, string StatusText) EvaluateOpenStatus(List<OperatingHourDto>? hours)
        {
            if (hours == null || hours.Count == 0)
            {
                return (true, "Open");
            }

            var now = DateTime.UtcNow;
            var currentDayName = now.DayOfWeek.ToString();

            var todayHour = hours.FirstOrDefault(h => h.Day.Equals(currentDayName, StringComparison.OrdinalIgnoreCase));
            if (todayHour == null || !todayHour.IsOpen)
            {
                return (false, "Closed Today");
            }

            if (TimeSpan.TryParse(todayHour.OpenTime, out var open) && TimeSpan.TryParse(todayHour.CloseTime, out var close))
            {
                var currentTime = now.TimeOfDay;
                if (currentTime >= open && currentTime <= close)
                {
                    return (true, $"Open until {DateTime.Today.Add(close):hh:mm tt}");
                }
                else if (currentTime < open)
                {
                    return (false, $"Opens today at {DateTime.Today.Add(open):hh:mm tt}");
                }
                else
                {
                    return (false, "Closed for the day");
                }
            }

            return (true, "Open");
        }

        private static string GetCurrencySymbol(string? currency)
        {
            return (currency?.ToUpperInvariant()) switch
            {
                "EUR" => "€",
                "GBP" => "£",
                "INR" => "₹",
                "CAD" => "CA$",
                "AUD" => "AU$",
                "JPY" => "¥",
                _ => "$"
            };
        }
    }
}
