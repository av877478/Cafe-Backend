using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderService _orderService;

        public OrderRepository(OrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync(
            string? status = null,
            string? serviceType = null,
            string? paymentStatus = null,
            string? customerName = null,
            string? tableNumber = null,
            string? search = null)
        {
            return await _orderService.GetAllOrdersAsync(status, serviceType, paymentStatus, customerName, tableNumber, search);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(string idOrNumber)
        {
            return await _orderService.GetOrderByIdAsync(idOrNumber);
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            return await _orderService.CreateOrderAsync(dto);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(string idOrNumber, string newStatus)
        {
            return await _orderService.UpdateOrderStatusAsync(idOrNumber, newStatus);
        }

        public async Task<OrderDto> ConfirmOrderAsync(string idOrNumber, int prepTime)
        {
            return await _orderService.ConfirmOrderAsync(idOrNumber, prepTime);
        }

        public async Task<OrderDto> RejectOrderAsync(string idOrNumber, string? reason)
        {
            return await _orderService.RejectOrderAsync(idOrNumber, reason);
        }

        public async Task<OrderDto> AssignWaiterAsync(string idOrNumber, string waiterName)
        {
            return await _orderService.AssignWaiterAsync(idOrNumber, waiterName);
        }

        public async Task<OrderDto> UpdatePaymentStatusAsync(string idOrNumber, string paymentStatus, string? paymentMethod = null, string? transactionId = null)
        {
            return await _orderService.UpdatePaymentStatusAsync(idOrNumber, paymentStatus, paymentMethod, transactionId);
        }

        public async Task<bool> DeleteOrderAsync(string idOrNumber)
        {
            return await _orderService.DeleteOrderAsync(idOrNumber);
        }

        public async Task<List<OrderDto>> GetCustomerOrdersAsync(string customerName)
        {
            return await _orderService.GetCustomerOrdersAsync(customerName);
        }

        public async Task<OrderDto> CancelCustomerOrderAsync(string idOrNumber, string? reason)
        {
            return await _orderService.CancelCustomerOrderAsync(idOrNumber, reason);
        }

        public async Task<ReorderResponseDto> GetReorderDetailsAsync(string idOrNumber)
        {
            return await _orderService.GetReorderDetailsAsync(idOrNumber);
        }
    }
}
