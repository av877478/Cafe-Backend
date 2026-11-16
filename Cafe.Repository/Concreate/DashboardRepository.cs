using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DashboardService _dashboardService;

        public DashboardRepository(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            return await _dashboardService.GetDashboardStatsAsync();
        }
    }
}
