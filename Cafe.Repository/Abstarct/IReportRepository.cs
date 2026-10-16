using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface IReportRepository
    {
        Task<ReportDashboardDto> GetReportDataAsync(string? range);
        Task<byte[]> ExportReportCsvAsync(string? range);
    }
}
