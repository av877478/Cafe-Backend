using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface ICustomerMenuRepository
    {
        Task<CustomerMenuDto> GetCustomerMenuAsync(string? categoryId = null, string? search = null);
        Task<List<ProductDto>> GetCustomerProductsAsync(string? categoryId = null, string? search = null);
        Task<ProductDto?> GetCustomerProductByIdAsync(int id);
        Task<List<CategoryDto>> GetCustomerCategoriesAsync();
    }
}
