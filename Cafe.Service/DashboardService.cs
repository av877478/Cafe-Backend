using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Cafe.Service
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _dbContext;

        public DashboardService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;
            var todayUtcStart = now.Date;
            var todayUtcEnd = todayUtcStart.AddDays(1).AddTicks(-1);

            // 1. Fetch all orders with their items
            var allOrders = await _dbContext.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            // 2. Fetch all tables
            var allTables = await _dbContext.Tables
                .AsNoTracking()
                .ToListAsync();

            // 3. Fetch all products
            var allProducts = await _dbContext.Products
                .AsNoTracking()
                .ToListAsync();

            // Filter non-cancelled/non-rejected orders
            var validOrders = allOrders
                .Where(o => !string.Equals(o.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(o.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var todaysOrders = validOrders
                .Where(o => o.OrderTime >= todayUtcStart && o.OrderTime <= todayUtcEnd)
                .ToList();

            // If orders exist today, show today's; otherwise show total valid orders
            var primaryOrders = todaysOrders.Count > 0 ? todaysOrders : validOrders;

            decimal todaysSales = primaryOrders.Sum(o => o.Total);
            int todaysOrdersCount = primaryOrders.Count;

            // Pending Orders (needs kitchen or payment attention)
            int pendingOrders = allOrders.Count(o =>
                string.Equals(o.Status, "New", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Status, "Waiting for Confirmation", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Status, "Pending", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.PaymentStatus, "Pending", StringComparison.OrdinalIgnoreCase));

            // Completed Orders
            int completedOrders = allOrders.Count(o =>
                string.Equals(o.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Status, "Served", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Status, "Picked Up", StringComparison.OrdinalIgnoreCase));

            // Tables
            int totalTables = allTables.Count > 0 ? allTables.Count : 15;
            
            // Check active tables from Tables table and active orders
            var occupiedTableNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in allTables.Where(t => string.Equals(t.Status, "Occupied", StringComparison.OrdinalIgnoreCase) ||
                                                  string.Equals(t.Status, "Reserved", StringComparison.OrdinalIgnoreCase)))
            {
                occupiedTableNumbers.Add(t.Number.ToString());
            }

            foreach (var o in allOrders.Where(o => string.Equals(o.Status, "Preparing", StringComparison.OrdinalIgnoreCase) ||
                                                  string.Equals(o.Status, "Confirmed", StringComparison.OrdinalIgnoreCase) ||
                                                  string.Equals(o.Status, "New", StringComparison.OrdinalIgnoreCase)))
            {
                if (!string.IsNullOrWhiteSpace(o.TableNumber) && !o.TableNumber.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                {
                    var cleanNum = o.TableNumber.Replace("Table", "", StringComparison.OrdinalIgnoreCase).Trim();
                    occupiedTableNumbers.Add(cleanNum);
                }
            }

            int activeTables = occupiedTableNumbers.Count;
            if (activeTables > totalTables) activeTables = totalTables;

            // Revenue Chart Data (Hourly slots: 8 AM, 10 AM, 12 PM, 2 PM, 4 PM, 6 PM, 8 PM)
            var timeSlots = new (string Label, int StartHour, int EndHour)[]
            {
                ("8 AM", 7, 9),
                ("10 AM", 9, 11),
                ("12 PM", 11, 13),
                ("2 PM", 13, 15),
                ("4 PM", 15, 17),
                ("6 PM", 17, 19),
                ("8 PM", 19, 21)
            };

            var chartLabels = new List<string>();
            var chartData = new List<decimal>();

            foreach (var slot in timeSlots)
            {
                chartLabels.Add(slot.Label);
                var revenueInSlot = primaryOrders
                    .Where(o => o.OrderTime.Hour >= slot.StartHour && o.OrderTime.Hour < slot.EndHour)
                    .Sum(o => o.Total);
                chartData.Add(revenueInSlot);
            }

            // In case orders happened outside the 8 AM - 8 PM windows, distribute evenly or ensure at least the total matches
            if (chartData.All(d => d == 0m) && todaysSales > 0m)
            {
                // Put total on the peak slot or middle slot
                int middleIndex = chartData.Count / 2;
                chartData[middleIndex] = todaysSales;
            }

            // Order Status Summary (Doughnut)
            int inProgressCount = allOrders.Count(o =>
                string.Equals(o.Status, "Confirmed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Status, "Preparing", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Status, "Ready", StringComparison.OrdinalIgnoreCase));

            int pendingCount = allOrders.Count(o =>
                string.Equals(o.Status, "New", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Status, "Waiting for Confirmation", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Status, "Pending", StringComparison.OrdinalIgnoreCase));

            var orderStatusSummary = new List<DashboardOrderStatusSummaryDto>
            {
                new DashboardOrderStatusSummaryDto { Status = "Completed", Count = completedOrders, Color = "#4E220F" },
                new DashboardOrderStatusSummaryDto { Status = "In Progress", Count = inProgressCount, Color = "#9D6638" },
                new DashboardOrderStatusSummaryDto { Status = "Pending", Count = pendingCount, Color = "#B0BA99" }
            };

            // Recent Orders (Top 5 latest orders from database)
            var recentOrders = allOrders.Take(5).Select(o =>
            {
                string statusMapped;
                if (string.Equals(o.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(o.Status, "Served", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(o.Status, "Picked Up", StringComparison.OrdinalIgnoreCase))
                {
                    statusMapped = "Completed";
                }
                else if (string.Equals(o.Status, "Confirmed", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(o.Status, "Preparing", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(o.Status, "Ready", StringComparison.OrdinalIgnoreCase))
                {
                    statusMapped = "In Progress";
                }
                else
                {
                    statusMapped = "Pending";
                }

                string tableDisplay = "Takeaway";
                if (!string.IsNullOrWhiteSpace(o.TableNumber) && !o.TableNumber.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                {
                    tableDisplay = o.TableNumber.StartsWith("Table", StringComparison.OrdinalIgnoreCase)
                        ? o.TableNumber
                        : $"Table {o.TableNumber}";
                }
                else if (string.Equals(o.ServiceType, "Dine-In", StringComparison.OrdinalIgnoreCase))
                {
                    tableDisplay = "Dine-In";
                }

                return new DashboardRecentOrderDto
                {
                    Id = string.IsNullOrWhiteSpace(o.OrderNumber) ? $"#ORD-{o.Id}" : (o.OrderNumber.StartsWith("#") ? o.OrderNumber : $"#{o.OrderNumber}"),
                    Table = tableDisplay,
                    Status = statusMapped,
                    Total = o.Total,
                    Time = FormatTimeAgo(o.OrderTime)
                };
            }).ToList();

            // Popular Products (Aggregated from OrderItems in DB)
            var orderItems = allOrders.SelectMany(o => o.Items).ToList();
            var popularGrouped = orderItems
                .GroupBy(i => i.ProductName, StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    Name = g.Key,
                    Sales = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.TotalPrice)
                })
                .OrderByDescending(p => p.Sales)
                .Take(5)
                .ToList();

            var popularProducts = new List<DashboardPopularProductDto>();
            foreach (var p in popularGrouped)
            {
                var productMeta = allProducts.FirstOrDefault(prod => prod.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                popularProducts.Add(new DashboardPopularProductDto
                {
                    Name = p.Name,
                    Category = productMeta?.CategoryName ?? "Coffee",
                    Sales = p.Sales,
                    Price = productMeta?.BasePrice ?? (p.Sales > 0 ? Math.Round(p.Revenue / p.Sales, 2) : 0m)
                });
            }

            // If fewer than 4 products in order history, fill with other available products from Products table
            if (popularProducts.Count < 4)
            {
                foreach (var prod in allProducts)
                {
                    if (!popularProducts.Any(pp => pp.Name.Equals(prod.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        popularProducts.Add(new DashboardPopularProductDto
                        {
                            Name = prod.Name,
                            Category = prod.CategoryName ?? "Coffee",
                            Sales = 0,
                            Price = prod.BasePrice
                        });
                        if (popularProducts.Count >= 4) break;
                    }
                }
            }

            return new DashboardStatsDto
            {
                TodaysSales = todaysSales,
                TodaysOrders = todaysOrdersCount,
                PendingOrders = pendingOrders,
                CompletedOrders = completedOrders,
                ActiveTables = activeTables,
                TotalTables = totalTables,
                RevenueChartData = new RevenueChartDto
                {
                    Labels = chartLabels,
                    Data = chartData
                },
                PopularProducts = popularProducts,
                RecentOrders = recentOrders,
                OrderStatusSummary = orderStatusSummary
            };
        }

        private static string FormatTimeAgo(DateTime dt)
        {
            var diff = DateTime.UtcNow - dt;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} mins ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return dt.ToString("MMM d");
        }
    }
}
