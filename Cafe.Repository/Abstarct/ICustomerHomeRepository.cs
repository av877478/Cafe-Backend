using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface ICustomerHomeRepository
    {
        Task<CustomerHomeDto> GetCustomerHomeDataAsync();
        Task<List<ProductDto>> GetFeaturedProductsAsync();
        Task<List<ProductDto>> GetPopularProductsAsync();
    }
}
