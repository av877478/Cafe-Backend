using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Cafe.Service
{
    public class SettingService
    {
        private readonly ApplicationDbContext _context;

        public SettingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CafeSettingsDto> GetSettingsAsync()
        {
            var entity = await _context.Settings.FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = GetDefaultEntity();
                _context.Settings.Add(entity);
                await _context.SaveChangesAsync();
            }

            return MapToDto(entity);
        }

        public async Task<CafeSettingsDto> UpdateSettingsAsync(CafeSettingsDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var entity = await _context.Settings.FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = new CafeSetting();
                _context.Settings.Add(entity);
            }

            // General
            if (dto.General != null)
            {
                entity.CafeName = !string.IsNullOrWhiteSpace(dto.General.CafeName) ? dto.General.CafeName.Trim() : "Little Talk Cafe";
                entity.Description = dto.General.Description ?? string.Empty;
                entity.LogoUrl = dto.General.LogoUrl ?? string.Empty;
                entity.Address = dto.General.Address ?? string.Empty;
                entity.Phone = dto.General.Phone ?? string.Empty;
                entity.Email = dto.General.Email ?? string.Empty;
                entity.FestivalTheme = !string.IsNullOrWhiteSpace(dto.General.FestivalTheme) ? dto.General.FestivalTheme.ToLowerInvariant() : "normal";
            }

            // Hours
            if (dto.Hours != null)
            {
                entity.HoursJson = JsonSerializer.Serialize(dto.Hours);
            }

            // Financial
            if (dto.Financial != null)
            {
                entity.TaxRate = dto.Financial.TaxRate >= 0 ? dto.Financial.TaxRate : 8.0m;
                entity.Currency = !string.IsNullOrWhiteSpace(dto.Financial.Currency) ? dto.Financial.Currency.Trim() : "USD";
                entity.AcceptCash = dto.Financial.AcceptCash;
                entity.AcceptCard = dto.Financial.AcceptCard;
                entity.AcceptUpi = dto.Financial.AcceptUpi;
            }

            // Notifications
            if (dto.Notifications != null)
            {
                entity.NewOrderEmail = dto.Notifications.NewOrderEmail;
                entity.NewOrderSms = dto.Notifications.NewOrderSms;
                entity.DailyReport = dto.Notifications.DailyReport;
            }

            // Security
            if (dto.Security != null)
            {
                entity.Require2fa = dto.Security.Require2fa;
                entity.SessionTimeout = dto.Security.SessionTimeout >= 5 ? dto.Security.SessionTimeout : 60;
            }

            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(entity);
        }

        private static CafeSettingsDto MapToDto(CafeSetting entity)
        {
            List<OperatingHourDto> hours;
            try
            {
                hours = !string.IsNullOrWhiteSpace(entity.HoursJson)
                    ? JsonSerializer.Deserialize<List<OperatingHourDto>>(entity.HoursJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? GetDefaultHours()
                    : GetDefaultHours();
            }
            catch
            {
                hours = GetDefaultHours();
            }

            return new CafeSettingsDto
            {
                General = new GeneralSettingsDto
                {
                    CafeName = entity.CafeName,
                    Description = entity.Description,
                    LogoUrl = entity.LogoUrl,
                    Address = entity.Address,
                    Phone = entity.Phone,
                    Email = entity.Email,
                    FestivalTheme = entity.FestivalTheme
                },
                Hours = hours,
                Financial = new FinancialSettingsDto
                {
                    TaxRate = entity.TaxRate,
                    Currency = entity.Currency,
                    AcceptCash = entity.AcceptCash,
                    AcceptCard = entity.AcceptCard,
                    AcceptUpi = entity.AcceptUpi
                },
                Notifications = new NotificationSettingsDto
                {
                    NewOrderEmail = entity.NewOrderEmail,
                    NewOrderSms = entity.NewOrderSms,
                    DailyReport = entity.DailyReport
                },
                Security = new SecuritySettingsDto
                {
                    Require2fa = entity.Require2fa,
                    SessionTimeout = entity.SessionTimeout
                }
            };
        }

        private static CafeSetting GetDefaultEntity()
        {
            var defaultHours = GetDefaultHours();
            return new CafeSetting
            {
                CafeName = "Little Talk Cafe",
                Description = "Where Every Sip Starts a Conversation.",
                LogoUrl = "https://images.unsplash.com/photo-1559925393-8be0ec4767c8?q=80&w=200&auto=format&fit=crop",
                Address = "123 Coffee Lane, Brewville, BE 90210",
                Phone = "+1 (555) 123-4567",
                Email = "hello@artisancafe.com",
                FestivalTheme = "normal",
                HoursJson = JsonSerializer.Serialize(defaultHours),
                TaxRate = 8.5m,
                Currency = "USD",
                AcceptCash = true,
                AcceptCard = true,
                AcceptUpi = true,
                NewOrderEmail = true,
                NewOrderSms = false,
                DailyReport = true,
                Require2fa = false,
                SessionTimeout = 60,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private static List<OperatingHourDto> GetDefaultHours()
        {
            return new List<OperatingHourDto>
            {
                new() { Day = "Monday", IsOpen = true, OpenTime = "07:00", CloseTime = "18:00" },
                new() { Day = "Tuesday", IsOpen = true, OpenTime = "07:00", CloseTime = "18:00" },
                new() { Day = "Wednesday", IsOpen = true, OpenTime = "07:00", CloseTime = "18:00" },
                new() { Day = "Thursday", IsOpen = true, OpenTime = "07:00", CloseTime = "18:00" },
                new() { Day = "Friday", IsOpen = true, OpenTime = "07:00", CloseTime = "20:00" },
                new() { Day = "Saturday", IsOpen = true, OpenTime = "08:00", CloseTime = "20:00" },
                new() { Day = "Sunday", IsOpen = false, OpenTime = "08:00", CloseTime = "15:00" }
            };
        }
    }
}
