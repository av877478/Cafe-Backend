using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Cafe.Service
{
    public class CustomerHomeService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly SettingService _settingService;

        public CustomerHomeService(
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

        public async Task<CustomerHomeDto> GetCustomerHomeDataAsync()
        {
            // 1. Fetch Active Categories
            var categories = await _categoryService.GetAllCategoriesAsync("Active");

            // 2. Fetch Active Products
            var allActiveProducts = await _productService.GetAllProductsAsync(null, "Active");

            // 3. Featured Products
            var featured = allActiveProducts.Where(p => p.IsFeatured).ToList();
            if (featured.Count == 0 && allActiveProducts.Count > 0)
            {
                featured = allActiveProducts.Take(4).ToList();
            }

            // 4. Popular Products
            var popular = allActiveProducts.Where(p => p.IsPopular).ToList();
            if (popular.Count == 0 && allActiveProducts.Count > 0)
            {
                popular = allActiveProducts.Skip(featured.Count > 0 ? 1 : 0).Take(6).ToList();
                if (popular.Count == 0) popular = allActiveProducts.Take(6).ToList();
            }

            // 5. Fetch Settings
            var settings = await _settingService.GetSettingsAsync();

            // 6. Calculate Open/Closed status
            var (isOpen, statusText) = EvaluateOpenStatus(settings.Hours);

            var currencySymbol = GetCurrencySymbol(settings.Financial?.Currency);

            return new CustomerHomeDto
            {
                Categories = categories,
                FeaturedProducts = featured,
                PopularProducts = popular,
                GeneralSettings = settings.General ?? new GeneralSettingsDto(),
                CurrencySymbol = currencySymbol,
                OperatingStatusText = statusText,
                IsOpen = isOpen
            };
        }

        public async Task<List<ProductDto>> GetFeaturedProductsAsync()
        {
            var allActive = await _productService.GetAllProductsAsync(null, "Active");
            var featured = allActive.Where(p => p.IsFeatured).ToList();
            return featured.Count > 0 ? featured : allActive.Take(4).ToList();
        }

        public async Task<List<ProductDto>> GetPopularProductsAsync()
        {
            var allActive = await _productService.GetAllProductsAsync(null, "Active");
            var popular = allActive.Where(p => p.IsPopular).ToList();
            return popular.Count > 0 ? popular : allActive.Take(6).ToList();
        }

        private static (bool IsOpen, string StatusText) EvaluateOpenStatus(List<OperatingHourDto>? hours)
        {
            if (hours == null || hours.Count == 0)
            {
                return (true, "Open");
            }

            var now = DateTime.UtcNow; // Or local cafe time
            var currentDayName = now.DayOfWeek.ToString();

            var todayHour = hours.FirstOrDefault(h => h.Day.Equals(currentDayName, StringComparison.OrdinalIgnoreCase));
            if (todayHour == null || !todayHour.IsOpen)
            {
                return (false, "Closed Today");
            }

            if (TimeSpan.TryParse(todayHour.OpenTime, out var openTime) &&
                TimeSpan.TryParse(todayHour.CloseTime, out var closeTime))
            {
                var currentTime = now.TimeOfDay;
                if (currentTime >= openTime && currentTime <= closeTime)
                {
                    return (true, $"Open until {FormatTimeSpan(closeTime)}");
                }
                else if (currentTime < openTime)
                {
                    return (false, $"Opens at {FormatTimeSpan(openTime)}");
                }
                else
                {
                    return (false, "Closed for the day");
                }
            }

            return (true, "Open");
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            int hour12 = ts.Hours % 12 == 0 ? 12 : ts.Hours % 12;
            string ampm = ts.Hours >= 12 ? "PM" : "AM";
            return $"{hour12}:{ts.Minutes:D2} {ampm}";
        }

        private static string GetCurrencySymbol(string? currency)
        {
            return (currency ?? "USD").ToUpperInvariant() switch
            {
                "EUR" => "€",
                "GBP" => "£",
                "INR" => "₹",
                "JPY" => "¥",
                _ => "$"
            };
        }
    }
}
