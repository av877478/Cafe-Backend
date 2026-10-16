using System.Collections.Generic;

namespace Cafe.Model.Model
{
    public class CustomerHomeDto
    {
        public List<CategoryDto> Categories { get; set; } = new();
        public List<ProductDto> FeaturedProducts { get; set; } = new();
        public List<ProductDto> PopularProducts { get; set; } = new();
        public GeneralSettingsDto GeneralSettings { get; set; } = new();
        public string CurrencySymbol { get; set; } = "$";
        public string OperatingStatusText { get; set; } = string.Empty;
        public bool IsOpen { get; set; } = true;
    }
}
