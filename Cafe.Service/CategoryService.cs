using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Cafe.Service
{
    public class CategoryService
    {
        private readonly ApplicationDbContext _dbContext;

        public CategoryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Get Categories

        public async Task<List<CategoryDto>> GetAllCategoriesAsync(string? statusFilter = null, string? search = null)
        {
            var query = _dbContext.Categories.AsNoTracking();

            // Status filtering: All, Active, Inactive
            if (!string.IsNullOrWhiteSpace(statusFilter) && !statusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (statusFilter.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(c => c.IsActive);
                }
                else if (statusFilter.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(c => !c.IsActive);
                }
            }

            // Search filter by name or description
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(term) || c.Description.ToLower().Contains(term));
            }

            var categories = await query
                .OrderBy(c => c.Id)
                .ToListAsync();

            return categories.Select(MapToDto).ToList();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _dbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            return category == null ? null : MapToDto(category);
        }

        #endregion

        #region Create Category

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var trimmedName = dto.Name.Trim();
            var exists = await _dbContext.Categories.AnyAsync(c => c.Name.ToLower() == trimmedName.ToLower());
            if (exists)
            {
                throw new InvalidOperationException($"Category '{trimmedName}' already exists. Please choose a unique name.");
            }

            var newCategory = new Category
            {
                Name = trimmedName,
                Description = (dto.Description ?? string.Empty).Trim(),
                ImageUrl = dto.ImageUrl.Trim(),
                IsActive = dto.IsActive,
                ItemCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Categories.AddAsync(newCategory);
            await _dbContext.SaveChangesAsync();

            return MapToDto(newCategory);
        }

        #endregion

        #region Update Category

        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            var trimmedName = dto.Name.Trim();
            if (!category.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase))
            {
                var conflict = await _dbContext.Categories.AnyAsync(c => c.Name.ToLower() == trimmedName.ToLower() && c.Id != id);
                if (conflict)
                {
                    throw new InvalidOperationException($"Category '{trimmedName}' already exists.");
                }
            }

            category.Name = trimmedName;
            category.Description = (dto.Description ?? string.Empty).Trim();
            category.ImageUrl = dto.ImageUrl.Trim();
            category.IsActive = dto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return MapToDto(category);
        }

        public async Task<CategoryDto> UpdateCategoryStatusAsync(int id, bool isActive)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            category.IsActive = isActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return MapToDto(category);
        }

        #endregion

        #region Delete Category

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return false;

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Helpers

        public static CategoryDto MapToDto(Category c)
        {
            return new CategoryDto
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                Icon = ResolveCategoryIcon(c.Name),
                IsActive = c.IsActive,
                ItemCount = c.ItemCount
            };
        }

        public static string ResolveCategoryIcon(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "☕";
            var lower = name.ToLower();
            if (lower.Contains("hot") || lower.Contains("coffee") || lower.Contains("espresso") || lower.Contains("brew")) return "☕";
            if (lower.Contains("cold") || lower.Contains("iced") || lower.Contains("frappe")) return "🧊";
            if (lower.Contains("tea") || lower.Contains("chai") || lower.Contains("matcha")) return "🍵";
            if (lower.Contains("pastr") || lower.Contains("bakery") || lower.Contains("croissant") || lower.Contains("bread")) return "🥐";
            if (lower.Contains("sandwich") || lower.Contains("toast") || lower.Contains("panini") || lower.Contains("savory")) return "🥪";
            if (lower.Contains("cake") || lower.Contains("dessert") || lower.Contains("sweet") || lower.Contains("cookie")) return "🍰";
            if (lower.Contains("juice") || lower.Contains("smoothie") || lower.Contains("shake") || lower.Contains("bever")) return "🧃";
            if (lower.Contains("snack") || lower.Contains("bite")) return "🥨";
            if (lower.Contains("meal") || lower.Contains("dish") || lower.Contains("breakfast") || lower.Contains("lunch")) return "🍽️";
            return "☕";
        }

        #endregion
    }
}
