using System;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/[controller]")]
    [Route("api/settings")]
    [ApiController]
    public class SettingController : ControllerBase
    {
        private readonly ISettingRepository _settingRepository;

        public SettingController(ISettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        /// <summary>
        /// Retrieves the global cafe settings (General, Hours, Financials, Notifications, Security).
        /// Routes: GET /api/setting, GET /api/settings
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CafeSettingsDto), 200)]
        public async Task<IActionResult> GetSettings()
        {
            try
            {
                var settings = await _settingRepository.GetSettingsAsync();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve cafe settings.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates the global cafe settings.
        /// Routes: PUT /api/setting, PUT /api/settings, POST /api/settings
        /// </summary>
        [HttpPut]
        [HttpPost]
        [ProducesResponseType(typeof(CafeSettingsDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateSettings([FromBody] CafeSettingsDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Settings data cannot be empty." });
            }

            try
            {
                var updated = await _settingRepository.UpdateSettingsAsync(dto);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update cafe settings.", error = ex.Message });
            }
        }
    }
}
