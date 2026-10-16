using System;

namespace Cafe.Model.Model
{
    public class CafeSetting
    {
        public int Id { get; set; } = 1;

        // General
        public string CafeName { get; set; } = "Little Talk Cafe";
        public string Description { get; set; } = "Where Every Sip Starts a Conversation.";
        public string LogoUrl { get; set; } = "assets/LogoImage/1.png";
        public string Address { get; set; } = "123 Coffee Lane, Brewville, BE 90210";
        public string Phone { get; set; } = "+1 (555) 123-4567";
        public string Email { get; set; } = "hello@artisancafe.com";
        public string FestivalTheme { get; set; } = "normal";

        // Operating Hours stored as JSON
        public string HoursJson { get; set; } = string.Empty;

        // Financial
        public decimal TaxRate { get; set; } = 8.0m;
        public string Currency { get; set; } = "USD";
        public bool AcceptCash { get; set; } = true;
        public bool AcceptCard { get; set; } = true;
        public bool AcceptUpi { get; set; } = true;

        // Notifications
        public bool NewOrderEmail { get; set; } = true;
        public bool NewOrderSms { get; set; } = false;
        public bool DailyReport { get; set; } = true;

        // Security
        public bool Require2fa { get; set; } = false;
        public int SessionTimeout { get; set; } = 60;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
