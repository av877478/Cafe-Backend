using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Cafe.Service
{
    public class KitchenService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OrderService _orderService;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public KitchenService(ApplicationDbContext dbContext, OrderService orderService)
        {
            _dbContext = dbContext;
            _orderService = orderService;
        }

        #region Kitchen Kanban Board

        public async Task<KitchenBoardDto> GetKitchenBoardAsync()
        {
            var activeOrders = await _dbContext.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .Where(o => o.Status == "Confirmed" || o.Status == "Preparing" || o.Status == "Ready")
                .OrderBy(o => o.OrderTime)
                .ToListAsync();

            var mappedOrders = activeOrders.Select(MapToDto).ToList();

            return new KitchenBoardDto
            {
                NewOrders = mappedOrders
                    .Where(o => o.Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(o => o.OrderTime)
                    .ToList(),

                PreparingOrders = mappedOrders
                    .Where(o => o.Status.Equals("Preparing", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(o => o.OrderTime)
                    .ToList(),

                ReadyOrders = mappedOrders
                    .Where(o => o.Status.Equals("Ready", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(o => o.OrderTime)
                    .ToList()
            };
        }

        public async Task<List<OrderDto>> GetActiveOrdersAsync()
        {
            var orders = await _dbContext.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .Where(o => o.Status == "Confirmed" || o.Status == "Preparing" || o.Status == "Ready")
                .OrderBy(o => o.OrderTime)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        #endregion

        #region Status Transition Actions

        public async Task<OrderDto> AdvanceOrderStatusAsync(string orderId)
        {
            var order = await FindOrderEntityAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order '{orderId}' not found.");
            }

            string currentStatus = order.Status.Trim();
            string nextStatus = currentStatus switch
            {
                "New" => "Confirmed",
                "Waiting for Confirmation" => "Confirmed",
                "Confirmed" => "Preparing",
                "Preparing" => "Ready",
                "Ready" => order.ServiceType.Equals("Takeaway", StringComparison.OrdinalIgnoreCase) ? "Picked Up" : "Served",
                _ => currentStatus
            };

            order.Status = nextStatus;
            order.UpdatedAt = DateTime.UtcNow;

            if (nextStatus is "Served" or "Picked Up" or "Completed")
            {
                order.CompletedTime ??= DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            return MapToDto(order);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(string orderId, string status)
        {
            return await _orderService.UpdateOrderStatusAsync(orderId, status);
        }

        #endregion

        #region Helpers

        private async Task<Order?> FindOrderEntityAsync(string idOrNumber)
        {
            if (int.TryParse(idOrNumber, out int intId))
            {
                var byId = await _dbContext.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == intId);
                if (byId != null) return byId;
            }

            var trimmed = idOrNumber.Trim();
            return await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderNumber.ToLower() == trimmed.ToLower());
        }

        private static OrderDto MapToDto(Order entity)
        {
            var itemsDto = (entity.Items ?? new List<OrderItem>()).Select(i =>
            {
                List<string>? customs = null;
                if (!string.IsNullOrWhiteSpace(i.CustomizationsJson))
                {
                    try
                    {
                        customs = JsonSerializer.Deserialize<List<string>>(i.CustomizationsJson, JsonOptions);
                    }
                    catch
                    {
                        customs = null;
                    }
                }

                return new OrderItemDto
                {
                    Id = i.Id.ToString(),
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    Customizations = customs ?? new List<string>()
                };
            }).ToList();

            return new OrderDto
            {
                Id = entity.Id.ToString(),
                OrderNumber = entity.OrderNumber,
                CustomerName = entity.CustomerName,
                TableNumber = entity.TableNumber,
                ServiceType = entity.ServiceType,
                PaymentStatus = entity.PaymentStatus,
                PaymentMethod = entity.PaymentMethod,
                TransactionId = entity.TransactionId,
                Status = entity.Status,
                OrderTime = entity.OrderTime,
                Items = itemsDto,
                Subtotal = entity.Subtotal,
                Tax = entity.Tax,
                Total = entity.Total,
                CompletedTime = entity.CompletedTime,
                EstimatedPrepTime = entity.EstimatedPrepTime,
                ConfirmedTime = entity.ConfirmedTime,
                RejectionReason = entity.RejectionReason,
                Notes = entity.Notes,
                WaiterName = entity.WaiterName
            };
        }

        #endregion
    }
}
