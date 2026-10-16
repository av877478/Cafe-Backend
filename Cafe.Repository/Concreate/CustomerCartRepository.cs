using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class CustomerCartRepository : ICustomerCartRepository
    {
        private readonly CustomerCartService _customerCartService;

        public CustomerCartRepository(CustomerCartService customerCartService)
        {
            _customerCartService = customerCartService;
        }

        public async Task<ValidateCartResponseDto> ValidateAndPriceCartAsync(ValidateCartRequestDto request)
        {
            return await _customerCartService.ValidateAndPriceCartAsync(request);
        }

        public async Task<OrderDto> CheckoutCartAsync(CartCheckoutRequestDto request)
        {
            return await _customerCartService.CheckoutCartAsync(request);
        }

        public async Task<CheckoutSettingsDto> GetCheckoutSettingsAsync()
        {
            return await _customerCartService.GetCheckoutSettingsAsync();
        }
    }
}
