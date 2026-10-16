using System.Collections.Generic;

namespace Cafe.Model.Model
{
    public class OperatingHourDto
    {
        public string Day { get; set; } = string.Empty;
        public bool IsOpen { get; set; } = true;
        public string OpenTime { get; set; } = "07:00";
        public string CloseTime { get; set; } = "18:00";
    }

    public class GeneralSettingsDto
    {
        public string CafeName { get; set; } = "Little Talk Cafe";
        public string Description { get; set; } = "Where Every Sip Starts a Conversation.";
        public string LogoUrl { get; set; } = "assets/LogoImage/1.png";
        public string Address { get; set; } = "123 Coffee Lane, Brewville, BE 90210";
        public string Phone { get; set; } = "+1 (555) 123-4567";
        public string Email { get; set; } = "hello@artisancafe.com";
        public string FestivalTheme { get; set; } = "normal";
    }

    public class FinancialSettingsDto
    {
        public decimal TaxRate { get; set; } = 8.0m;
        public string Currency { get; set; } = "USD";
        public bool AcceptCash { get; set; } = true;
        public bool AcceptCard { get; set; } = true;
        public bool AcceptUpi { get; set; } = true;
    }

    public class NotificationSettingsDto
    {
        public bool NewOrderEmail { get; set; } = true;
        public bool NewOrderSms { get; set; } = false;
        public bool DailyReport { get; set; } = true;
    }

    public class SecuritySettingsDto
    {
        public bool Require2fa { get; set; } = false;
        public int SessionTimeout { get; set; } = 60;
    }

    public class CafeSettingsDto
    {
        public GeneralSettingsDto General { get; set; } = new();
        public List<OperatingHourDto> Hours { get; set; } = new();
        public FinancialSettingsDto Financial { get; set; } = new();
        public NotificationSettingsDto Notifications { get; set; } = new();
        public SecuritySettingsDto Security { get; set; } = new();
    }
}
