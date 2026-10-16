using System;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportRepository _reportRepository;

        public ReportController(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        /// <summary>
        /// Retrieves the comprehensive business reports and analytics dashboard.
        /// Query parameters: range = 'Today' | 'This Week' | 'This Month' (default 'Today').
        /// Routes: GET /api/reports, GET /api/reports/dashboard
        /// </summary>
        [HttpGet]
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ReportDashboardDto), 200)]
        public async Task<IActionResult> GetReportDashboard([FromQuery] string? range = "Today")
        {
            try
            {
                var data = await _reportRepository.GetReportDataAsync(range);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve report data.", error = ex.Message });
            }
        }

        /// <summary>
        /// Exports the sales and performance report as a CSV spreadsheet.
        /// Route: GET /api/reports/export?range=Today
        /// </summary>
        [HttpGet("export")]
        [Produces("text/csv")]
        [ProducesResponseType(typeof(FileResult), 200)]
        public async Task<IActionResult> ExportReportCsv([FromQuery] string? range = "Today")
        {
            try
            {
                var csvBytes = await _reportRepository.ExportReportCsvAsync(range);
                var safeRange = (range ?? "Today").Replace(" ", "-").ToLowerInvariant();
                var fileName = $"little-talk-cafe-report-{safeRange}-{DateTime.UtcNow:yyyyMMdd}.csv";

                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to export report CSV.", error = ex.Message });
            }
        }
    }
}
