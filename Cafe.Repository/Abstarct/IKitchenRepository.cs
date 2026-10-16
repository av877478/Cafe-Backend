using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface IKitchenRepository
    {
        Task<KitchenBoardDto> GetKitchenBoardAsync();
        Task<List<OrderDto>> GetActiveOrdersAsync();
        Task<OrderDto> AdvanceOrderStatusAsync(string orderId);
        Task<OrderDto> UpdateOrderStatusAsync(string orderId, string status);
    }
}
