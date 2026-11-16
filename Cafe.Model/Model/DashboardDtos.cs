using System.Collections.Generic;

namespace Cafe.Model.Model
{
    public class RevenueChartDto
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Data { get; set; } = new();
    }

    public class DashboardPopularProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Sales { get; set; }
        public decimal Price { get; set; }
    }

    public class DashboardRecentOrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string Table { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Time { get; set; } = string.Empty;
    }

    public class DashboardOrderStatusSummaryDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class DashboardStatsDto
    {
        public decimal TodaysSales { get; set; }
        public int TodaysOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int ActiveTables { get; set; }
        public int TotalTables { get; set; }
        public RevenueChartDto RevenueChartData { get; set; } = new();
        public List<DashboardPopularProductDto> PopularProducts { get; set; } = new();
        public List<DashboardRecentOrderDto> RecentOrders { get; set; } = new();
        public List<DashboardOrderStatusSummaryDto> OrderStatusSummary { get; set; } = new();
    }
}
