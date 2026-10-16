using System.Collections.Generic;

namespace Cafe.Model.Model
{
    public class CustomerMenuDto
    {
        public List<CategoryDto> Categories { get; set; } = new();
        public List<ProductDto> Products { get; set; } = new();
        public List<ProductDto> FeaturedProducts { get; set; } = new();
        public int TotalItems { get; set; }
        public string CurrencySymbol { get; set; } = "$";
        public GeneralSettingsDto? GeneralSettings { get; set; }
        public string OperatingStatusText { get; set; } = "Open";
        public bool IsOpen { get; set; } = true;
    }
}
