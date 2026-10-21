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
    public class StaffService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OrderService _orderService;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public StaffService(ApplicationDbContext dbContext, OrderService orderService)
        {
            _dbContext = dbContext;
            _orderService = orderService;
        }

        #region Staff Members Management

        public async Task<List<StaffMemberDto>> GetAllStaffMembersAsync(bool activeOnly = true)
        {
            var query = _dbContext.StaffMembers.AsNoTracking().AsQueryable();

            if (activeOnly)
            {
                query = query.Where(s => s.IsActive);
            }

            var staff = await query
                .OrderBy(s => s.Id)
                .ToListAsync();

            return staff.Select(MapStaffToDto).ToList();
        }

        public async Task<StaffMemberDto?> GetStaffMemberByIdAsync(int id)
        {
            var staff = await _dbContext.StaffMembers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            return staff == null ? null : MapStaffToDto(staff);
        }

        public async Task<StaffMemberDto> CreateStaffMemberAsync(CreateStaffMemberDto dto)
        {
            var trimmedName = dto.Name.Trim();
            var exists = await _dbContext.StaffMembers.AnyAsync(s => s.Name.ToLower() == trimmedName.ToLower());
            if (exists)
            {
                throw new InvalidOperationException($"Staff member '{trimmedName}' already exists.");
            }

            var staff = new StaffMember
            {
                Name = trimmedName,
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "Waiter" : dto.Role.Trim(),
                Phone = dto.Phone?.Trim(),
                Email = dto.Email?.Trim(),
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.StaffMembers.AddAsync(staff);
            await _dbContext.SaveChangesAsync();

            return MapStaffToDto(staff);
        }

        public async Task<StaffMemberDto> UpdateStaffMemberAsync(int id, UpdateStaffMemberDto dto)
        {
            var staff = await _dbContext.StaffMembers.FirstOrDefaultAsync(s => s.Id == id);
            if (staff == null)
            {
                throw new KeyNotFoundException($"Staff member with ID {id} not found.");
            }

            var trimmedName = dto.Name.Trim();
            var duplicate = await _dbContext.StaffMembers.AnyAsync(s => s.Id != id && s.Name.ToLower() == trimmedName.ToLower());
            if (duplicate)
            {
                throw new InvalidOperationException($"Another staff member with the name '{trimmedName}' already exists.");
            }

            staff.Name = trimmedName;
            staff.Role = string.IsNullOrWhiteSpace(dto.Role) ? "Waiter" : dto.Role.Trim();
            staff.Phone = dto.Phone?.Trim();
            staff.Email = dto.Email?.Trim();
            staff.IsActive = dto.IsActive;
            staff.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return MapStaffToDto(staff);
        }

        public async Task<bool> DeleteStaffMemberAsync(int id)
        {
            var staff = await _dbContext.StaffMembers.FirstOrDefaultAsync(s => s.Id == id);
            if (staff == null)
            {
                return false;
            }

            _dbContext.StaffMembers.Remove(staff);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Waitstaff Panel Table Orders

        public async Task<List<OrderDto>> GetActiveDineInOrdersAsync()
        {
            var inactiveStatuses = new[] { "served", "completed", "cancelled", "rejected" };

            var orders = await _dbContext.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .Where(o => o.ServiceType.ToLower() == "dine-in" &&
                            !inactiveStatuses.Contains(o.Status.ToLower()))
                .ToListAsync();

            // Sort matching frontend priority: Ready (1), Preparing (2), Confirmed (3), New (4), other (5);
            // Secondary sort: oldest first (OrderTime ascending)
            var sortedOrders = orders
                .OrderBy(o => GetStatusPriority(o.Status))
                .ThenBy(o => o.OrderTime)
                .Select(MapOrderToDto)
                .ToList();

            return sortedOrders;
        }

        public async Task<StaffPanelOverviewDto> GetStaffPanelOverviewAsync()
        {
            var activeOrders = await GetActiveDineInOrdersAsync();
            var staffMembers = await GetAllStaffMembersAsync(activeOnly: true);

            return new StaffPanelOverviewDto
            {
                ActiveTableCount = activeOrders.Count,
                Orders = activeOrders,
                Waiters = staffMembers.Select(s => s.Name).ToList()
            };
        }

        public async Task<OrderDto> AssignWaiterAsync(string orderId, string waiterName)
        {
            var order = await FindOrderEntityAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order '{orderId}' not found.");
            }

            order.WaiterName = waiterName.Trim();
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return MapOrderToDto(order);
        }

        public async Task<OrderDto> MarkOrderServedAsync(string orderId)
        {
            var order = await FindOrderEntityAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order '{orderId}' not found.");
            }

            order.Status = "Served";
            order.CompletedTime = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return MapOrderToDto(order);
        }

        #endregion

        #region Helpers & Seeding

        private static int GetStatusPriority(string status)
        {
            return status.ToLower() switch
            {
                "ready" => 1,
                "preparing" => 2,
                "confirmed" => 3,
                "new" => 4,
                _ => 5
            };
        }

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

        private static StaffMemberDto MapStaffToDto(StaffMember entity)
        {
            return new StaffMemberDto
            {
                Id = entity.Id.ToString(),
                Name = entity.Name,
                Role = entity.Role,
                Phone = entity.Phone,
                Email = entity.Email,
                IsActive = entity.IsActive
            };
        }

        private static OrderDto MapOrderToDto(Order entity)
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
