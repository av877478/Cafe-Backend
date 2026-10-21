using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Cafe.Service
{
    public class ReportService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OrderService _orderService;

        public ReportService(ApplicationDbContext dbContext, OrderService orderService)
        {
            _dbContext = dbContext;
            _orderService = orderService;
        }

        #region Get Report Data

        public async Task<ReportDashboardDto> GetReportDataAsync(string? range)
        {
            var normalizedRange = NormalizeRange(range);
            var (startUtc, endUtc, priorStartUtc, priorEndUtc, multiplier) = GetRangeBounds(normalizedRange);

            // Fetch orders in current period
            var currentOrders = await _dbContext.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .Where(o => o.OrderTime >= startUtc && o.OrderTime <= endUtc && o.Status != "Cancelled" && o.Status != "Rejected")
                .ToListAsync();

            // Fetch orders in prior period for growth calculation
            var priorOrders = await _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.OrderTime >= priorStartUtc && o.OrderTime <= priorEndUtc && o.Status != "Cancelled" && o.Status != "Rejected")
                .ToListAsync();

            // 1. Sales Summary
            decimal currentRevenue = currentOrders.Sum(o => o.Total);
            int currentTotalOrders = currentOrders.Count;

            decimal priorRevenue = priorOrders.Sum(o => o.Total);
            int priorTotalOrders = priorOrders.Count;

            decimal avgOrderValue = currentTotalOrders > 0
                ? Math.Round(currentRevenue / currentTotalOrders, 2)
                : 0m;

            decimal revenueGrowth = priorRevenue > 0
                ? Math.Round(((currentRevenue - priorRevenue) / priorRevenue) * 100m, 1)
                : 0m;

            decimal ordersGrowth = priorTotalOrders > 0
                ? Math.Round(((decimal)(currentTotalOrders - priorTotalOrders) / priorTotalOrders) * 100m, 1)
                : 0m;

            // 2. Top Products
            var orderItems = currentOrders.SelectMany(o => o.Items).ToList();

            var productGroups = orderItems
                .GroupBy(i => i.ProductName)
                .Select(g => new
                {
                    Name = g.Key,
                    QuantitySold = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.TotalPrice)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(5)
                .ToList();

            int maxProductQty = productGroups.Count > 0 ? productGroups.Max(p => p.QuantitySold) : 0;
            var topProducts = productGroups.Select(p => new TopProductDto
            {
                Name = p.Name,
                QuantitySold = p.QuantitySold,
                Revenue = p.Revenue,
                Percentage = maxProductQty > 0 ? Math.Round(((decimal)p.QuantitySold / maxProductQty) * 100m, 0) : 0
            }).ToList();

            // 3. Popular Categories (based on existing categories & order items if available)
            var categories = await _dbContext.Categories.AsNoTracking().ToListAsync();
            var popularCategories = new List<TopProductDto>();

            if (categories.Count > 0 && orderItems.Count > 0)
            {
                var products = await _dbContext.Products.AsNoTracking().ToListAsync();
                var productCatMap = products
                    .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().CategoryName ?? "Other", StringComparer.OrdinalIgnoreCase);

                var catGroups = orderItems
                    .GroupBy(i => productCatMap.TryGetValue(i.ProductName, out var c) ? c : "Other")
                    .Select(g => new
                    {
                        Name = g.Key,
                        QuantitySold = g.Sum(i => i.Quantity),
                        Revenue = g.Sum(i => i.TotalPrice)
                    })
                    .OrderByDescending(c => c.QuantitySold)
                    .Take(4)
                    .ToList();

                int maxCatQty = catGroups.Count > 0 ? catGroups.Max(c => c.QuantitySold) : 0;
                popularCategories = catGroups.Select(c => new TopProductDto
                {
                    Name = c.Name,
                    QuantitySold = c.QuantitySold,
                    Revenue = c.Revenue,
                    Percentage = maxCatQty > 0 ? Math.Round(((decimal)c.QuantitySold / maxCatQty) * 100m, 0) : 0
                }).ToList();
            }

            // 4. Payment Breakdown
            var paymentGroups = currentOrders
                .GroupBy(o => string.IsNullOrWhiteSpace(o.PaymentMethod) ? "Card" : o.PaymentMethod)
                .Select(g => new
                {
                    Method = g.Key,
                    Amount = g.Sum(o => o.Total)
                })
                .ToList();

            decimal totalPaymentAmount = paymentGroups.Sum(p => p.Amount);
            var paymentBreakdown = paymentGroups.Select(p => new PaymentBreakdownDto
            {
                Method = p.Method,
                Amount = p.Amount,
                Percentage = totalPaymentAmount > 0 ? Math.Round((p.Amount / totalPaymentAmount) * 100m, 0) : 0
            }).OrderByDescending(p => p.Amount).ToList();

            // 5. Peak Hours (7 AM to 3 PM cafe operation)
            var hourLabels = new[] { "7a", "8a", "9a", "10a", "11a", "12p", "1p", "2p", "3p" };
            var hourDistribution = hourLabels.ToDictionary(lbl => lbl, _ => 0);

            // Incorporate real order hours if present
            foreach (var order in currentOrders)
            {
                int h = order.OrderTime.Hour;
                string? label = h switch
                {
                    7 => "7a",
                    8 => "8a",
                    9 => "9a",
                    10 => "10a",
                    11 => "11a",
                    12 => "12p",
                    13 => "1p",
                    14 => "2p",
                    15 => "3p",
                    _ => null
                };

                if (label != null && hourDistribution.ContainsKey(label))
                {
                    hourDistribution[label] += 1;
                }
            }

            int peakCount = hourDistribution.Values.Max();
            var peakHours = hourLabels.Select(lbl => new PeakHourDto
            {
                HourLabel = lbl,
                OrderCount = hourDistribution[lbl],
                Percentage = peakCount > 0 ? Math.Round(((decimal)hourDistribution[lbl] / peakCount) * 100m, 0) : 0
            }).ToList();

            var peakHourItem = peakHours.OrderByDescending(h => h.OrderCount).FirstOrDefault();
            string peakHourLabel = (peakHourItem != null && peakHourItem.OrderCount > 0)
                ? ConvertHourLabel(peakHourItem.HourLabel)
                : "None";

            // 6. Table Performance
            var tableOrders = currentOrders.Where(o => !string.IsNullOrWhiteSpace(o.TableNumber)).ToList();

            var tablePerformance = tableOrders
                .GroupBy(o => o.TableNumber!)
                .Select(g => new TablePerformanceDto
                {
                    TableNumber = $"Table {g.Key}",
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.Total)
                })
                .OrderByDescending(t => t.Revenue)
                .Take(5)
                .ToList();

            return new ReportDashboardDto
            {
                Summary = new SalesSummaryDto
                {
                    Revenue = currentRevenue,
                    TotalOrders = currentTotalOrders,
                    AvgOrderValue = avgOrderValue,
                    RevenueGrowth = revenueGrowth,
                    OrdersGrowth = ordersGrowth,
                    PeakHour = peakHourLabel
                },
                TopProducts = topProducts,
                PopularCategories = popularCategories,
                PaymentBreakdown = paymentBreakdown,
                PeakHours = peakHours,
                TablePerformance = tablePerformance
            };
        }

        #endregion

        #region Export CSV

        public async Task<byte[]> ExportReportCsvAsync(string? range)
        {
            var data = await GetReportDataAsync(range);
            var normalizedRange = NormalizeRange(range);

            var sb = new StringBuilder();
            sb.AppendLine($"Little Talk Cafe - Business Performance Report ({normalizedRange})");
            sb.AppendLine($"Generated At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();

            // Summary KPIs
            sb.AppendLine("=== SALES SUMMARY ===");
            sb.AppendLine("Metric,Value");
            sb.AppendLine($"Total Revenue,${data.Summary.Revenue:F2}");
            sb.AppendLine($"Total Orders,{data.Summary.TotalOrders}");
            sb.AppendLine($"Average Order Value,${data.Summary.AvgOrderValue:F2}");
            sb.AppendLine($"Revenue Growth vs Prior,{data.Summary.RevenueGrowth:+0.0;-0.0;0.0}%");
            sb.AppendLine($"Order Volume Growth vs Prior,{data.Summary.OrdersGrowth:+0.0;-0.0;0.0}%");
            sb.AppendLine($"Peak Business Hour,{data.Summary.PeakHour}");
            sb.AppendLine();

            // Best-Selling Products
            sb.AppendLine("=== BEST-SELLING PRODUCTS ===");
            sb.AppendLine("Product Name,Quantity Sold,Revenue,Relative Popularity");
            foreach (var prod in data.TopProducts)
            {
                sb.AppendLine($"\"{prod.Name.Replace("\"", "\"\"")}\",{prod.QuantitySold},${prod.Revenue:F2},{prod.Percentage}%");
            }
            sb.AppendLine();

            // Popular Categories
            sb.AppendLine("=== POPULAR CATEGORIES ===");
            sb.AppendLine("Category Name,Orders Count,Revenue,Popularity Share");
            foreach (var cat in data.PopularCategories)
            {
                sb.AppendLine($"\"{cat.Name.Replace("\"", "\"\"")}\",{cat.QuantitySold},${cat.Revenue:F2},{cat.Percentage}%");
            }
            sb.AppendLine();

            // Payment Breakdown
            sb.AppendLine("=== PAYMENT METHOD BREAKDOWN ===");
            sb.AppendLine("Payment Method,Total Amount,Share");
            foreach (var pm in data.PaymentBreakdown)
            {
                sb.AppendLine($"\"{pm.Method}\",${pm.Amount:F2},{pm.Percentage}%");
            }
            sb.AppendLine();

            // Table Performance
            sb.AppendLine("=== TABLE PERFORMANCE ===");
            sb.AppendLine("Table,Order Count,Total Revenue");
            foreach (var tbl in data.TablePerformance)
            {
                sb.AppendLine($"\"{tbl.TableNumber}\",{tbl.OrderCount},${tbl.Revenue:F2}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        #endregion

        #region Helpers

        private static string NormalizeRange(string? range)
        {
            if (string.IsNullOrWhiteSpace(range)) return "Today";

            var lower = range.Trim().ToLowerInvariant();
            if (lower.Contains("week")) return "This Week";
            if (lower.Contains("month")) return "This Month";
            return "Today";
        }

        private static (DateTime start, DateTime end, DateTime priorStart, DateTime priorEnd, int multiplier) GetRangeBounds(string range)
        {
            var now = DateTime.UtcNow;

            return range switch
            {
                "This Week" => (
                    now.AddDays(-7),
                    now,
                    now.AddDays(-14),
                    now.AddDays(-7),
                    7
                ),
                "This Month" => (
                    now.AddDays(-30),
                    now,
                    now.AddDays(-60),
                    now.AddDays(-30),
                    30
                ),
                _ => (
                    now.Date,
                    now.Date.AddDays(1).AddTicks(-1),
                    now.Date.AddDays(-1),
                    now.Date.AddTicks(-1),
                    1
                )
            };
        }

        private static string ConvertHourLabel(string label)
        {
            return label.ToLowerInvariant() switch
            {
                "7a" => "7:00 AM",
                "8a" => "8:00 AM",
                "9a" => "9:00 AM",
                "10a" => "10:00 AM",
                "11a" => "11:00 AM",
                "12p" => "12:00 PM",
                "1p" => "1:00 PM",
                "2p" => "2:00 PM",
                "3p" => "3:00 PM",
                _ => "9:00 AM"
            };
        }

        #endregion
    }
}
