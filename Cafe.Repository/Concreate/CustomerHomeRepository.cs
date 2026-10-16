using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class CustomerHomeRepository : ICustomerHomeRepository
    {
        private readonly CustomerHomeService _customerHomeService;

        public CustomerHomeRepository(CustomerHomeService customerHomeService)
        {
            _customerHomeService = customerHomeService;
        }

        public async Task<CustomerHomeDto> GetCustomerHomeDataAsync()
        {
            return await _customerHomeService.GetCustomerHomeDataAsync();
        }

        public async Task<List<ProductDto>> GetFeaturedProductsAsync()
        {
            return await _customerHomeService.GetFeaturedProductsAsync();
        }

        public async Task<List<ProductDto>> GetPopularProductsAsync()
        {
            return await _customerHomeService.GetPopularProductsAsync();
        }
    }
}
