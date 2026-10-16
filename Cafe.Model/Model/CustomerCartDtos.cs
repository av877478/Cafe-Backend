using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cafe.Model.Model
{
    public class CustomerCartItemDto
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
        public int Quantity { get; set; } = 1;

        public decimal? UnitPrice { get; set; }

        public decimal? TotalPrice { get; set; }

        public Dictionary<string, string>? Options { get; set; }

        public string? Notes { get; set; }
    }

    public class ValidateCartRequestDto
    {
        [Required]
        public List<CustomerCartItemDto> Items { get; set; } = new();

        public string? TableNumber { get; set; }

        public string? ServiceType { get; set; }
    }

    public class ValidatedCartItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public Dictionary<string, string>? Options { get; set; }
        public string? Notes { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? WarningMessage { get; set; }
    }

    public class ValidateCartResponseDto
    {
        public List<ValidatedCartItemDto> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string CurrencySymbol { get; set; } = "$";
        public bool CanProceed { get; set; } = true;
        public List<string> Warnings { get; set; } = new();
    }

    public class CartCheckoutRequestDto
    {
        [Required(ErrorMessage = "Customer name is required.")]
        public string CustomerName { get; set; } = string.Empty;

        public string? TableNumber { get; set; }

        public string ServiceType { get; set; } = "Dine-In"; // Dine-In | Takeaway

        public string PaymentMethod { get; set; } = "Online"; // Online | Cash | Card | UPI

        public string? PaymentStatus { get; set; } = "Pending";

        public string? TransactionId { get; set; }

        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Cart must contain at least one item.")]
        public List<CustomerCartItemDto> Items { get; set; } = new();
    }

    public class CheckoutSettingsDto
    {
        public string Currency { get; set; } = "USD";
        public string CurrencySymbol { get; set; } = "$";
        public decimal TaxRate { get; set; } = 8.0m;
        public List<string> PaymentMethods { get; set; } = new();
        public bool IsOpen { get; set; } = true;
        public string OperatingStatusText { get; set; } = "Open";
    }
}
