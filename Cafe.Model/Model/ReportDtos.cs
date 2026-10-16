using System.Collections.Generic;

namespace Cafe.Model.Model
{
    public class SalesSummaryDto
    {
        public decimal Revenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AvgOrderValue { get; set; }
        public decimal RevenueGrowth { get; set; } // percentage vs prior period (+/-)
        public decimal OrdersGrowth { get; set; }  // percentage vs prior period (+/-)
        public string PeakHour { get; set; } = "9:00 AM";
    }

    public class TopProductDto
    {
        public string Name { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; } // Relative to highest product (0-100)
    }

    public class PaymentBreakdownDto
    {
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; } // Percentage of total revenue (0-100)
    }

    public class PeakHourDto
    {
        public string HourLabel { get; set; } = string.Empty; // e.g. "7a", "8a", "9a", ...
        public int OrderCount { get; set; }
        public decimal Percentage { get; set; } // Relative to peak hour volume (0-100)
    }

    public class TablePerformanceDto
    {
        public string TableNumber { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ReportDashboardDto
    {
        public SalesSummaryDto Summary { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
        public List<TopProductDto> PopularCategories { get; set; } = new();
        public List<PaymentBreakdownDto> PaymentBreakdown { get; set; } = new();
        public List<PeakHourDto> PeakHours { get; set; } = new();
        public List<TablePerformanceDto> TablePerformance { get; set; } = new();
    }
}
