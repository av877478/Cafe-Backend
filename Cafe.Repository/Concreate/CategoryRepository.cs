using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CategoryService _categoryService;

        public CategoryRepository(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync(string? statusFilter = null, string? search = null)
        {
            return await _categoryService.GetAllCategoriesAsync(statusFilter, search);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            return await _categoryService.GetCategoryByIdAsync(id);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            return await _categoryService.CreateCategoryAsync(dto);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            return await _categoryService.UpdateCategoryAsync(id, dto);
        }

        public async Task<CategoryDto> UpdateCategoryStatusAsync(int id, bool isActive)
        {
            return await _categoryService.UpdateCategoryStatusAsync(id, isActive);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await _categoryService.DeleteCategoryAsync(id);
        }

        public async Task<int> DeleteAllCategoriesAsync()
        {
            return await _categoryService.DeleteAllCategoriesAsync();
        }
    }
}
