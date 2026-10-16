using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface IOrderRepository
    {
        Task<List<OrderDto>> GetAllOrdersAsync(
            string? status = null,
            string? serviceType = null,
            string? paymentStatus = null,
            string? customerName = null,
            string? tableNumber = null,
            string? search = null);

        Task<OrderDto?> GetOrderByIdAsync(string idOrNumber);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderDto> UpdateOrderStatusAsync(string idOrNumber, string newStatus);
        Task<OrderDto> ConfirmOrderAsync(string idOrNumber, int prepTime);
        Task<OrderDto> RejectOrderAsync(string idOrNumber, string? reason);
        Task<OrderDto> AssignWaiterAsync(string idOrNumber, string waiterName);
        Task<OrderDto> UpdatePaymentStatusAsync(string idOrNumber, string paymentStatus, string? paymentMethod = null, string? transactionId = null);
        Task<bool> DeleteOrderAsync(string idOrNumber);
        Task<List<OrderDto>> GetCustomerOrdersAsync(string customerName);
        Task<OrderDto> CancelCustomerOrderAsync(string idOrNumber, string? reason);
        Task<ReorderResponseDto> GetReorderDetailsAsync(string idOrNumber);
    }
}
