using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductService _productService;

        public ProductRepository(ProductService productService)
        {
            _productService = productService;
        }

        public async Task<List<ProductDto>> GetAllProductsAsync(string? categoryId = null, string? statusFilter = null, string? search = null)
        {
            return await _productService.GetAllProductsAsync(categoryId, statusFilter, search);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            return await _productService.GetProductByIdAsync(id);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            return await _productService.CreateProductAsync(dto);
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            return await _productService.UpdateProductAsync(id, dto);
        }

        public async Task<ProductDto> UpdateProductStatusAsync(int id, string status)
        {
            return await _productService.UpdateProductStatusAsync(id, status);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            return await _productService.DeleteProductAsync(id);
        }
    }
}
