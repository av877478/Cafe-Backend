using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface ISettingRepository
    {
        Task<CafeSettingsDto> GetSettingsAsync();
        Task<CafeSettingsDto> UpdateSettingsAsync(CafeSettingsDto dto);
    }
}
