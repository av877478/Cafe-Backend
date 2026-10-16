using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Cafe.Service
{
    public class ProductService
    {
        private readonly ApplicationDbContext _dbContext;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ProductService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Get Products

        public async Task<List<ProductDto>> GetAllProductsAsync(string? categoryId = null, string? statusFilter = null, string? search = null)
        {
            // Seed initial sample products if table is completely empty
            await EnsureDefaultProductsAsync();

            var query = _dbContext.Products.AsNoTracking().AsQueryable();

            // Category filter
            if (!string.IsNullOrWhiteSpace(categoryId) && !categoryId.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(categoryId, out int parsedCatId))
                {
                    query = query.Where(p => p.CategoryId == parsedCatId);
                }
            }

            // Status filter ('Active', 'Inactive', 'Out of Stock', 'CustomerMenu')
            if (!string.IsNullOrWhiteSpace(statusFilter) && !statusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (statusFilter.Equals("CustomerMenu", StringComparison.OrdinalIgnoreCase) ||
                    statusFilter.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.Status.ToLower() != "inactive");
                }
                else
                {
                    query = query.Where(p => p.Status.ToLower() == statusFilter.ToLower());
                }
            }

            // Search filter by name or description
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
            }

            var products = await query
                .OrderBy(p => p.Id)
                .ToListAsync();

            return products.Select(MapToDto).ToList();
        }

        public async Task<List<ProductDto>> GetCustomerMenuProductsAsync(string? categoryId = null, string? search = null)
        {
            await EnsureDefaultProductsAsync();

            var query = _dbContext.Products.AsNoTracking().Where(p => p.Status.ToLower() != "inactive");

            if (!string.IsNullOrWhiteSpace(categoryId) && !categoryId.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(categoryId, out int parsedCatId))
                {
                    query = query.Where(p => p.CategoryId == parsedCatId);
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
            }

            var products = await query
                .OrderBy(p => p.Id)
                .ToListAsync();

            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            return product == null ? null : MapToDto(product);
        }

        #endregion

        #region Create Product

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            if (!int.TryParse(dto.CategoryId, out int catId))
            {
                throw new ArgumentException("Invalid Category ID format.");
            }

            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == catId);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with ID {catId} does not exist.");
            }

            var trimmedName = dto.Name.Trim();
            var exists = await _dbContext.Products.AnyAsync(p => p.Name.ToLower() == trimmedName.ToLower());
            if (exists)
            {
                throw new InvalidOperationException($"Product '{trimmedName}' already exists. Please choose a unique name.");
            }

            string? customizationJson = dto.CustomizationGroups != null && dto.CustomizationGroups.Count > 0
                ? JsonSerializer.Serialize(dto.CustomizationGroups, JsonOptions)
                : null;

            var product = new Product
            {
                Name = trimmedName,
                CategoryId = catId,
                CategoryName = category.Name,
                Description = (dto.Description ?? string.Empty).Trim(),
                BasePrice = dto.BasePrice,
                ImageUrl = dto.ImageUrl.Trim(),
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
                IsFeatured = dto.IsFeatured,
                IsPopular = dto.IsPopular,
                PreparationTime = dto.PreparationTime > 0 ? dto.PreparationTime : 5,
                CustomizationGroupsJson = customizationJson,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            // Synchronize category item count
            await SyncCategoryItemCountAsync(catId);

            return MapToDto(product);
        }

        #endregion

        #region Update Product

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            if (!int.TryParse(dto.CategoryId, out int catId))
            {
                throw new ArgumentException("Invalid Category ID format.");
            }

            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == catId);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with ID {catId} does not exist.");
            }

            var trimmedName = dto.Name.Trim();
            var duplicate = await _dbContext.Products.AnyAsync(p => p.Id != id && p.Name.ToLower() == trimmedName.ToLower());
            if (duplicate)
            {
                throw new InvalidOperationException($"Another product with the name '{trimmedName}' already exists.");
            }

            int previousCategoryId = product.CategoryId;

            product.Name = trimmedName;
            product.CategoryId = catId;
            product.CategoryName = category.Name;
            product.Description = (dto.Description ?? string.Empty).Trim();
            product.BasePrice = dto.BasePrice;
            product.ImageUrl = dto.ImageUrl.Trim();
            product.Status = string.IsNullOrWhiteSpace(dto.Status) ? product.Status : dto.Status.Trim();
            product.IsFeatured = dto.IsFeatured;
            product.IsPopular = dto.IsPopular;
            product.PreparationTime = dto.PreparationTime > 0 ? dto.PreparationTime : product.PreparationTime;

            if (dto.CustomizationGroups != null)
            {
                product.CustomizationGroupsJson = dto.CustomizationGroups.Count > 0
                    ? JsonSerializer.Serialize(dto.CustomizationGroups, JsonOptions)
                    : null;
            }

            product.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Synchronize category item count if category changed
            if (previousCategoryId != catId)
            {
                await SyncCategoryItemCountAsync(previousCategoryId);
            }
            await SyncCategoryItemCountAsync(catId);

            return MapToDto(product);
        }

        public async Task<ProductDto> UpdateProductStatusAsync(int id, string status)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            var validStatuses = new[] { "Active", "Inactive", "Out of Stock" };
            var matchedStatus = validStatuses.FirstOrDefault(s => s.Equals(status, StringComparison.OrdinalIgnoreCase));
            if (matchedStatus == null)
            {
                throw new ArgumentException("Status must be 'Active', 'Inactive', or 'Out of Stock'.");
            }

            product.Status = matchedStatus;
            product.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return MapToDto(product);
        }

        #endregion

        #region Delete Product

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return false;
            }

            int categoryId = product.CategoryId;

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            // Synchronize category item count
            await SyncCategoryItemCountAsync(categoryId);

            return true;
        }

        #endregion

        #region Helpers & Default Seeding

        private static ProductDto MapToDto(Product entity)
        {
            List<CustomizationGroupDto>? groups = null;
            if (!string.IsNullOrWhiteSpace(entity.CustomizationGroupsJson))
            {
                try
                {
                    groups = JsonSerializer.Deserialize<List<CustomizationGroupDto>>(entity.CustomizationGroupsJson, JsonOptions);
                }
                catch
                {
                    groups = null;
                }
            }

            return new ProductDto
            {
                Id = entity.Id.ToString(),
                Name = entity.Name,
                CategoryId = entity.CategoryId.ToString(),
                CategoryName = entity.CategoryName,
                Description = entity.Description,
                BasePrice = entity.BasePrice,
                ImageUrl = entity.ImageUrl,
                Status = entity.Status,
                IsFeatured = entity.IsFeatured,
                IsPopular = entity.IsPopular,
                PreparationTime = entity.PreparationTime > 0 ? entity.PreparationTime : 5,
                CustomizationGroups = groups
            };
        }

        private async Task SyncCategoryItemCountAsync(int categoryId)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            if (category != null)
            {
                category.ItemCount = await _dbContext.Products.CountAsync(p => p.CategoryId == categoryId);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task EnsureDefaultProductsAsync()
        {
            if (await _dbContext.Products.AnyAsync()) return;

            // Make sure categories exist
            var categories = await _dbContext.Categories.ToListAsync();
            if (categories.Count == 0) return;

            var hotCoffeeCat = categories.FirstOrDefault(c => c.Name.Contains("Coffee", StringComparison.OrdinalIgnoreCase)) ?? categories[0];
            var coldBrewCat = categories.FirstOrDefault(c => c.Name.Contains("Cold", StringComparison.OrdinalIgnoreCase)) ?? categories.ElementAtOrDefault(1) ?? categories[0];
            var pastryCat = categories.FirstOrDefault(c => c.Name.Contains("Pastr", StringComparison.OrdinalIgnoreCase)) ?? categories.ElementAtOrDefault(2) ?? categories[0];
            var sandwichCat = categories.FirstOrDefault(c => c.Name.Contains("Sandwich", StringComparison.OrdinalIgnoreCase)) ?? categories.ElementAtOrDefault(3) ?? categories[0];

            var macchiatoCustomizations = new List<CustomizationGroupDto>
            {
                new CustomizationGroupDto
                {
                    Name = "Size",
                    IsRequired = true,
                    MaxSelections = 1,
                    Options = new List<CustomizationOptionDto>
                    {
                        new CustomizationOptionDto { Name = "Tall (Small)", PriceAdjustment = 0m },
                        new CustomizationOptionDto { Name = "Grande (Medium)", PriceAdjustment = 0.80m },
                        new CustomizationOptionDto { Name = "Venti (Large)", PriceAdjustment = 1.20m }
                    }
                },
                new CustomizationGroupDto
                {
                    Name = "Extra Shots",
                    IsRequired = false,
                    MaxSelections = null,
                    Options = new List<CustomizationOptionDto>
                    {
                        new CustomizationOptionDto { Name = "Extra Espresso Shot", PriceAdjustment = 1.00m }
                    }
                }
            };

            var seedProducts = new List<Product>
            {
                new Product
                {
                    Name = "Caramel Macchiato",
                    CategoryId = hotCoffeeCat.Id,
                    CategoryName = hotCoffeeCat.Name,
                    Description = "Freshly steamed milk with vanilla-flavored syrup marked with espresso and topped with a caramel drizzle.",
                    BasePrice = 4.95m,
                    ImageUrl = "https://images.unsplash.com/photo-1485808191679-5f86510681a2?q=80&w=600&auto=format&fit=crop",
                    Status = "Active",
                    IsFeatured = true,
                    IsPopular = true,
                    PreparationTime = 5,
                    CustomizationGroupsJson = JsonSerializer.Serialize(macchiatoCustomizations, JsonOptions),
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Iced Latte",
                    CategoryId = coldBrewCat.Id,
                    CategoryName = coldBrewCat.Name,
                    Description = "Our dark, rich espresso combined with milk and served over ice.",
                    BasePrice = 3.95m,
                    ImageUrl = "https://images.unsplash.com/photo-1461023058943-0708e528a304?q=80&w=600&auto=format&fit=crop",
                    Status = "Active",
                    IsFeatured = true,
                    IsPopular = true,
                    PreparationTime = 3,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Avocado Toast",
                    CategoryId = sandwichCat.Id,
                    CategoryName = sandwichCat.Name,
                    Description = "Sourdough toast topped with smashed avocado, chili flakes, and microgreens.",
                    BasePrice = 8.50m,
                    ImageUrl = "https://www.eatingbirdfood.com/wp-content/uploads/2023/12/avocado-toast-hero-cropped.jpg?auto=format&fit=crop&q=80&w=600",
                    Status = "Active",
                    IsFeatured = true,
                    IsPopular = true,
                    PreparationTime = 8,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Matcha Cheesecake",
                    CategoryId = pastryCat.Id,
                    CategoryName = pastryCat.Name,
                    Description = "Creamy cheesecake infused with premium ceremonial grade matcha.",
                    BasePrice = 6.50m,
                    ImageUrl = "https://emsfoodiefix.com/wp-content/uploads/2025/03/DSC08960.jpg?auto=format&fit=crop&q=80&w=600",
                    Status = "Active",
                    IsFeatured = true,
                    IsPopular = true,
                    PreparationTime = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Butter Croissant",
                    CategoryId = pastryCat.Id,
                    CategoryName = pastryCat.Name,
                    Description = "A classic butter croissant with a flaky, golden-brown crust.",
                    BasePrice = 3.25m,
                    ImageUrl = "https://images.unsplash.com/photo-1549903072-7e6e0fb26ffe?q=80&w=600&auto=format&fit=crop",
                    Status = "Active",
                    IsFeatured = false,
                    IsPopular = true,
                    PreparationTime = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Turkey Panini",
                    CategoryId = sandwichCat.Id,
                    CategoryName = sandwichCat.Name,
                    Description = "Roast turkey, provolone cheese, and pesto on toasted focaccia.",
                    BasePrice = 7.45m,
                    ImageUrl = "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?q=80&w=600&auto=format&fit=crop",
                    Status = "Active",
                    IsFeatured = false,
                    IsPopular = true,
                    PreparationTime = 7,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _dbContext.Products.AddRangeAsync(seedProducts);
            await _dbContext.SaveChangesAsync();

            // Update item counts for all categories
            foreach (var cat in categories)
            {
                cat.ItemCount = await _dbContext.Products.CountAsync(p => p.CategoryId == cat.Id);
            }
            await _dbContext.SaveChangesAsync();
        }

        #endregion
    }
}
