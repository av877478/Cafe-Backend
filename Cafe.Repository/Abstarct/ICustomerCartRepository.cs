using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface ICustomerCartRepository
    {
        Task<ValidateCartResponseDto> ValidateAndPriceCartAsync(ValidateCartRequestDto request);
        Task<OrderDto> CheckoutCartAsync(CartCheckoutRequestDto request);
        Task<CheckoutSettingsDto> GetCheckoutSettingsAsync();
    }
}
