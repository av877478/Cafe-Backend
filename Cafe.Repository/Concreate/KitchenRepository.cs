using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class KitchenRepository : IKitchenRepository
    {
        private readonly KitchenService _kitchenService;

        public KitchenRepository(KitchenService kitchenService)
        {
            _kitchenService = kitchenService;
        }

        public async Task<KitchenBoardDto> GetKitchenBoardAsync()
        {
            return await _kitchenService.GetKitchenBoardAsync();
        }

        public async Task<List<OrderDto>> GetActiveOrdersAsync()
        {
            return await _kitchenService.GetActiveOrdersAsync();
        }

        public async Task<OrderDto> AdvanceOrderStatusAsync(string orderId)
        {
            return await _kitchenService.AdvanceOrderStatusAsync(orderId);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(string orderId, string status)
        {
            return await _kitchenService.UpdateOrderStatusAsync(orderId, status);
        }
    }
}
