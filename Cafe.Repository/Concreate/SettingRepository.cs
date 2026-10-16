using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class SettingRepository : ISettingRepository
    {
        private readonly SettingService _settingService;

        public SettingRepository(SettingService settingService)
        {
            _settingService = settingService;
        }

        public async Task<CafeSettingsDto> GetSettingsAsync()
        {
            return await _settingService.GetSettingsAsync();
        }

        public async Task<CafeSettingsDto> UpdateSettingsAsync(CafeSettingsDto dto)
        {
            return await _settingService.UpdateSettingsAsync(dto);
        }
    }
}
