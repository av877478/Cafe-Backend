using System;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/[controller]")]
    [Route("api/dashboard")]
    [Route("api/admin/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        /// <summary>
        /// Retrieves the real-time overview dashboard statistics directly from the database.
        /// Routes: GET /api/dashboard, GET /api/admin/dashboard
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(DashboardStatsDto), 200)]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await _dashboardRepository.GetDashboardStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve dashboard stats from database.", error = ex.Message });
            }
        }
    }
}
