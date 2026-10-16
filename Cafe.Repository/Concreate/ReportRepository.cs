using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class ReportRepository : IReportRepository
    {
        private readonly ReportService _reportService;

        public ReportRepository(ReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<ReportDashboardDto> GetReportDataAsync(string? range)
        {
            return await _reportService.GetReportDataAsync(range);
        }

        public async Task<byte[]> ExportReportCsvAsync(string? range)
        {
            return await _reportService.ExportReportCsvAsync(range);
        }
    }
}
