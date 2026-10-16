using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class CustomerMenuRepository : ICustomerMenuRepository
    {
        private readonly CustomerMenuService _customerMenuService;

        public CustomerMenuRepository(CustomerMenuService customerMenuService)
        {
            _customerMenuService = customerMenuService;
        }

        public async Task<CustomerMenuDto> GetCustomerMenuAsync(string? categoryId = null, string? search = null)
        {
            return await _customerMenuService.GetCustomerMenuAsync(categoryId, search);
        }

        public async Task<List<ProductDto>> GetCustomerProductsAsync(string? categoryId = null, string? search = null)
        {
            return await _customerMenuService.GetCustomerProductsAsync(categoryId, search);
        }

        public async Task<ProductDto?> GetCustomerProductByIdAsync(int id)
        {
            return await _customerMenuService.GetCustomerProductByIdAsync(id);
        }

        public async Task<List<CategoryDto>> GetCustomerCategoriesAsync()
        {
            return await _customerMenuService.GetCustomerCategoriesAsync();
        }
    }
}
