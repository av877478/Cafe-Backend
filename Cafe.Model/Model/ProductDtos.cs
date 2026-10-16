using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cafe.Model.Model
{
    public class CustomizationOptionDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public decimal PriceAdjustment { get; set; } = 0;
        public decimal PriceModifier => PriceAdjustment;
    }

    public class CustomizationGroupDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool Required => IsRequired;
        public int? MaxSelections { get; set; }
        public List<CustomizationOptionDto> Options { get; set; } = new();
    }

    public class ProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal Price => BasePrice;
        public string ImageUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "Active"; // Active | Inactive | Out of Stock
        public bool IsAvailable => Status != null && Status.Equals("Active", System.StringComparison.OrdinalIgnoreCase);
        public bool IsFeatured { get; set; } = false;
        public bool IsPopular { get; set; } = false;
        public int PreparationTime { get; set; } = 5;
        public List<CustomizationGroupDto>? CustomizationGroups { get; set; }
        public List<CustomizationGroupDto>? CustomizationOptions => CustomizationGroups;
    }

    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required.")]
        [MaxLength(50, ErrorMessage = "Product name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public string CategoryId { get; set; } = string.Empty;

        public string? CategoryName { get; set; }

        [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base price is required.")]
        [Range(0, 100000, ErrorMessage = "Price must be positive.")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Product image URL is required.")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Active|Inactive|Out of Stock)$", ErrorMessage = "Status must be 'Active', 'Inactive', or 'Out of Stock'.")]
        public string Status { get; set; } = "Active";

        public bool IsFeatured { get; set; } = false;
        public bool IsPopular { get; set; } = false;
        public int PreparationTime { get; set; } = 5;

        public List<CustomizationGroupDto>? CustomizationGroups { get; set; }
    }

    public class UpdateProductDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [MaxLength(50, ErrorMessage = "Product name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public string CategoryId { get; set; } = string.Empty;

        public string? CategoryName { get; set; }

        [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base price is required.")]
        [Range(0, 100000, ErrorMessage = "Price must be positive.")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Product image URL is required.")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Active|Inactive|Out of Stock)$", ErrorMessage = "Status must be 'Active', 'Inactive', or 'Out of Stock'.")]
        public string Status { get; set; } = "Active";

        public bool IsFeatured { get; set; } = false;
        public bool IsPopular { get; set; } = false;
        public int PreparationTime { get; set; } = 5;

        public List<CustomizationGroupDto>? CustomizationGroups { get; set; }
    }

    public class UpdateProductStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Active|Inactive|Out of Stock)$", ErrorMessage = "Status must be 'Active', 'Inactive', or 'Out of Stock'.")]
        public string Status { get; set; } = "Active";
    }
}
