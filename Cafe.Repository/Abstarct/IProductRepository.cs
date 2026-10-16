using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface IProductRepository
    {
        Task<List<ProductDto>> GetAllProductsAsync(string? categoryId = null, string? statusFilter = null, string? search = null);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<ProductDto> UpdateProductStatusAsync(int id, string status);
        Task<bool> DeleteProductAsync(int id);
    }
}
