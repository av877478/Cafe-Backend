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
    public class OrderService
    {
        private readonly ApplicationDbContext _dbContext;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public OrderService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Get Orders

        public async Task<List<OrderDto>> GetAllOrdersAsync(
            string? status = null,
            string? serviceType = null,
            string? paymentStatus = null,
            string? customerName = null,
            string? tableNumber = null,
            string? search = null)
        {
            var query = _dbContext.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(o => o.Status.ToLower() == status.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(serviceType) && !serviceType.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(o => o.ServiceType.ToLower() == serviceType.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(paymentStatus) && !paymentStatus.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(o => o.PaymentStatus.ToLower() == paymentStatus.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(customerName))
            {
                var trimmedCust = customerName.Trim().ToLower();
                query = query.Where(o => o.CustomerName.ToLower() == trimmedCust);
            }

            if (!string.IsNullOrWhiteSpace(tableNumber))
            {
                var trimmedTable = tableNumber.Trim().ToLower();
                query = query.Where(o => o.TableNumber != null && o.TableNumber.ToLower() == trimmedTable);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(o =>
                    o.OrderNumber.ToLower().Contains(term) ||
                    o.CustomerName.ToLower().Contains(term) ||
                    (o.TransactionId != null && o.TransactionId.ToLower().Contains(term)) ||
                    (o.Notes != null && o.Notes.ToLower().Contains(term))
                );
            }

            var orders = await query
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(string idOrNumber)
        {
            var order = await FindOrderEntityAsync(idOrNumber);
            return order == null ? null : MapToDto(order);
        }

        #endregion

        #region Create Order

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            var customerName = dto.CustomerName.Trim();
            if (string.IsNullOrWhiteSpace(customerName))
            {
                throw new ArgumentException("Customer name is required.");
            }

            // Generate unique OrderNumber: e.g. ORD-1006
            var lastOrder = await _dbContext.Orders.OrderByDescending(o => o.Id).FirstOrDefaultAsync();
            int nextNumber = (lastOrder?.Id ?? 0) + 1001;
            string orderNumber = $"ORD-{nextNumber}";

            // Calculate totals if needed
            decimal subtotal = dto.Subtotal;
            if (subtotal <= 0 && dto.Items.Count > 0)
            {
                subtotal = dto.Items.Sum(i => i.TotalPrice > 0 ? i.TotalPrice : i.UnitPrice * i.Quantity);
            }

            decimal tax = dto.Tax;
            if (tax <= 0 && subtotal > 0)
            {
                tax = Math.Round(subtotal * 0.08m, 2); // 8% default tax if uncomputed
            }

            decimal total = dto.Total;
            if (total <= 0)
            {
                total = Math.Round(subtotal + tax, 2);
            }

            string? tableNumber = CustomerCartService.CleanTableNumber(dto.TableNumber);
            if (string.Equals(dto.ServiceType?.Trim(), "Takeaway", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dto.ServiceType?.Trim(), "Delivery", StringComparison.OrdinalIgnoreCase))
            {
                tableNumber = null;
            }

            var notes = dto.Notes?.Trim();
            if (!string.IsNullOrWhiteSpace(dto.GuestId) && !(notes != null && notes.Contains(dto.GuestId)))
            {
                notes = string.IsNullOrWhiteSpace(notes) ? $"[GuestId: {dto.GuestId.Trim()}]" : $"{notes} [GuestId: {dto.GuestId.Trim()}]";
            }

            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerName = customerName,
                TableNumber = tableNumber,
                ServiceType = string.IsNullOrWhiteSpace(dto.ServiceType) ? "Dine-In" : dto.ServiceType.Trim(),
                PaymentStatus = string.IsNullOrWhiteSpace(dto.PaymentStatus) ? "Pending" : dto.PaymentStatus.Trim(),
                PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? "Online" : dto.PaymentMethod.Trim(),
                TransactionId = string.IsNullOrWhiteSpace(dto.TransactionId) ? $"TXN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}" : dto.TransactionId.Trim(),
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "New" : dto.Status.Trim(),
                OrderTime = DateTime.UtcNow,
                Subtotal = subtotal,
                Tax = tax,
                Total = total,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var itemDto in dto.Items)
            {
                string? customJson = itemDto.Customizations != null && itemDto.Customizations.Count > 0
                    ? JsonSerializer.Serialize(itemDto.Customizations, JsonOptions)
                    : null;

                decimal itemTotal = itemDto.TotalPrice > 0 ? itemDto.TotalPrice : itemDto.UnitPrice * itemDto.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = itemDto.ProductId.Trim(),
                    ProductName = itemDto.ProductName.Trim(),
                    Quantity = itemDto.Quantity <= 0 ? 1 : itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemTotal,
                    CustomizationsJson = customJson
                });
            }

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            return MapToDto(order);
        }

        #endregion

        #region Update Order Status & Workflow

        public async Task<OrderDto> UpdateOrderStatusAsync(string idOrNumber, string newStatus)
        {
            var order = await FindOrderEntityAsync(idOrNumber);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order '{idOrNumber}' not found.");
            }

            order.Status = newStatus.Trim();
            order.UpdatedAt = DateTime.UtcNow;

            if (newStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                newStatus.Equals("Served", StringComparison.OrdinalIgnoreCase) ||
                newStatus.Equals("Picked Up", StringComparison.OrdinalIgnoreCase) ||
                newStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
                newStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                order.CompletedTime ??= DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            return MapToDto(order);
        }

        public async Task<OrderDto> ConfirmOrderAsync(string idOrNumber, int prepTime)
        {
            var order = await FindOrderEntityAsync(idOrNumber);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order '{idOrNumber}' not found.");
            }

            order.Status = "Confirmed";
            order.EstimatedPrepTime = prepTime;
            order.ConfirmedTime = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return MapToDto(order);
        }

        public async Task<OrderDto> RejectOrderAsync(string idOrNumber, string? reason)
        {
            var order = await FindOrderEntityAsync(idOrNumber);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order '{idOrNumber}' not found.");
            }

            order.Status = "Rejected";
            order.RejectionReason = reason?.Trim();
            order.CompletedTime = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return MapToDto(order);
        }

        public async Task<OrderDto> AssignWaiterAsync(string idOrNumber, string waiterName)
        {
            var order = await FindOrderEntityAsync(idOrNumber);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order '{idOrNumber}' not found.");
            }

            order.WaiterName = waiterName.Trim();
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return MapToDto(order);
        }

        public async Task<OrderDto> UpdatePaymentStatusAsync(string idOrNumber, string paymentStatus, string? paymentMethod = null, string? transactionId = null)
        {
            var order = await FindOrderEntityAsync(idOrNumber);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order '{idOrNumber}' not found.");
            }

            order.PaymentStatus = paymentStatus.Trim();
            if (!string.IsNullOrWhiteSpace(paymentMethod))
            {
                order.PaymentMethod = paymentMethod.Trim();
            }
            if (!string.IsNullOrWhiteSpace(transactionId))
            {
                order.TransactionId = transactionId.Trim();
            }
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return MapToDto(order);
        }

        public async Task<bool> DeleteOrderAsync(string idOrNumber)
        {
            var order = await FindOrderEntityAsync(idOrNumber);
            if (order == null)
            {
                return false;
            }

            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<OrderDto>> GetCustomerOrdersAsync(string customerName)
        {
            var trimmed = customerName.Trim().ToLower();
            var orders = await _dbContext.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .Where(o => o.CustomerName.ToLower() == trimmed)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto> CancelCustomerOrderAsync(string idOrNumber, string? reason)
        {
            var order = await FindOrderEntityAsync(idOrNumber);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order '{idOrNumber}' not found.");
            }

            var cancellableStatuses = new[] { "new", "waiting for confirmation" };
            if (!cancellableStatuses.Contains(order.Status.ToLower()))
            {
                throw new InvalidOperationException($"Order cannot be cancelled because it is already in '{order.Status}' status.");
            }

            order.Status = "Cancelled";
            order.RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Cancelled by customer" : reason.Trim();
            order.CompletedTime = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return MapToDto(order);
        }

        public async Task<ReorderResponseDto> GetReorderDetailsAsync(string idOrNumber)
        {
            var order = await FindOrderEntityAsync(idOrNumber);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order '{idOrNumber}' not found.");
            }

            var activeProducts = await _dbContext.Products
                .AsNoTracking()
                .Where(p => p.Status.ToLower() == "active")
                .ToListAsync();

            var activeProductIds = activeProducts.Select(p => p.Id.ToString()).ToHashSet();

            var orderDto = MapToDto(order);
            var result = new ReorderResponseDto
            {
                OrderNumber = orderDto.OrderNumber,
                Items = orderDto.Items
            };

            foreach (var item in orderDto.Items)
            {
                if (!activeProductIds.Contains(item.ProductId))
                {
                    result.AllItemsAvailable = false;
                    result.UnavailableItems.Add(item.ProductName);
                }
            }

            return result;
        }

        #endregion

        #region Helpers & Seeding

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
                TableNumber = CustomerCartService.CleanTableNumber(entity.TableNumber),
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
