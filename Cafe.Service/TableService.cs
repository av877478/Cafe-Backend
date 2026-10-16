using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Cafe.Service
{
    public class TableService
    {
        private readonly ApplicationDbContext _dbContext;

        public TableService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Get Tables

        public async Task<List<TableDto>> GetAllTablesAsync(string? statusFilter = null)
        {
            // Seed initial tables if database is completely empty
            await EnsureDefaultTablesAsync();

            var query = _dbContext.Tables.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(statusFilter) && !statusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => t.Status.ToLower() == statusFilter.Trim().ToLower());
            }

            var tables = await query
                .OrderBy(t => t.Number)
                .Select(t => new TableDto
                {
                    Id = t.Id.ToString(),
                    Number = t.Number,
                    Capacity = t.Capacity,
                    Status = t.Status
                })
                .ToListAsync();

            return tables;
        }

        public async Task<TableDto?> GetTableByIdAsync(int id)
        {
            var table = await _dbContext.Tables.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (table == null) return null;

            return new TableDto
            {
                Id = table.Id.ToString(),
                Number = table.Number,
                Capacity = table.Capacity,
                Status = table.Status
            };
        }

        #endregion

        #region Create Table

        public async Task<TableDto> CreateTableAsync(CreateTableDto dto)
        {
            // Validate unique table number
            var exists = await _dbContext.Tables.AnyAsync(t => t.Number == dto.Number);
            if (exists)
            {
                throw new InvalidOperationException($"Table #{dto.Number} already exists. Please choose a different table number.");
            }

            var newTable = new CafeTable
            {
                Number = dto.Number,
                Capacity = dto.Capacity,
                Status = NormalizeStatus(dto.Status),
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Tables.AddAsync(newTable);
            await _dbContext.SaveChangesAsync();

            // Automatically generate active QR code for newly created table
            var qrToken = QrCodeService.GenerateSecureToken();
            var targetUrl = $"http://localhost:4200/t/{qrToken}";
            var imageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(targetUrl)}";

            var qrCode = new TableQrCode
            {
                TableId = newTable.Id,
                TableNumber = newTable.Number,
                Token = qrToken,
                Status = "Active",
                ImageUrl = imageUrl,
                GeneratedAt = DateTime.UtcNow
            };

            await _dbContext.TableQrCodes.AddAsync(qrCode);
            await _dbContext.SaveChangesAsync();

            return new TableDto
            {
                Id = newTable.Id.ToString(),
                Number = newTable.Number,
                Capacity = newTable.Capacity,
                Status = newTable.Status
            };
        }

        #endregion

        #region Update Table

        public async Task<TableDto> UpdateTableAsync(int id, UpdateTableDto dto)
        {
            var table = await _dbContext.Tables.FirstOrDefaultAsync(t => t.Id == id);
            if (table == null)
            {
                throw new KeyNotFoundException($"Table with ID {id} not found.");
            }

            // If number changed, verify it does not conflict with another table
            if (table.Number != dto.Number)
            {
                var conflict = await _dbContext.Tables.AnyAsync(t => t.Number == dto.Number && t.Id != id);
                if (conflict)
                {
                    throw new InvalidOperationException($"Table #{dto.Number} already exists. Cannot update to an existing table number.");
                }
            }

            table.Number = dto.Number;
            table.Capacity = dto.Capacity;
            table.Status = NormalizeStatus(dto.Status);
            table.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return new TableDto
            {
                Id = table.Id.ToString(),
                Number = table.Number,
                Capacity = table.Capacity,
                Status = table.Status
            };
        }

        public async Task<TableDto> UpdateTableStatusAsync(int id, string status)
        {
            var table = await _dbContext.Tables.FirstOrDefaultAsync(t => t.Id == id);
            if (table == null)
            {
                throw new KeyNotFoundException($"Table with ID {id} not found.");
            }

            table.Status = NormalizeStatus(status);
            table.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return new TableDto
            {
                Id = table.Id.ToString(),
                Number = table.Number,
                Capacity = table.Capacity,
                Status = table.Status
            };
        }

        #endregion

        #region Delete Table

        public async Task<bool> DeleteTableAsync(int id)
        {
            var table = await _dbContext.Tables.FirstOrDefaultAsync(t => t.Id == id);
            if (table == null)
            {
                return false;
            }

            var qrs = await _dbContext.TableQrCodes.Where(q => q.TableId == id).ToListAsync();
            if (qrs.Count != 0)
            {
                _dbContext.TableQrCodes.RemoveRange(qrs);
            }

            _dbContext.Tables.Remove(table);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Seed Helpers

        private async Task EnsureDefaultTablesAsync()
        {
            if (!await _dbContext.Tables.AnyAsync())
            {
                var defaultTables = new List<CafeTable>
                {
                    new CafeTable { Number = 1, Capacity = 2, Status = "Available", CreatedAt = DateTime.UtcNow },
                    new CafeTable { Number = 2, Capacity = 2, Status = "Occupied", CreatedAt = DateTime.UtcNow },
                    new CafeTable { Number = 3, Capacity = 4, Status = "Available", CreatedAt = DateTime.UtcNow },
                    new CafeTable { Number = 4, Capacity = 4, Status = "Reserved", CreatedAt = DateTime.UtcNow },
                    new CafeTable { Number = 5, Capacity = 6, Status = "Available", CreatedAt = DateTime.UtcNow },
                    new CafeTable { Number = 6, Capacity = 8, Status = "Occupied", CreatedAt = DateTime.UtcNow },
                    new CafeTable { Number = 7, Capacity = 2, Status = "Out of Service", CreatedAt = DateTime.UtcNow },
                    new CafeTable { Number = 8, Capacity = 4, Status = "Available", CreatedAt = DateTime.UtcNow }
                };

                await _dbContext.Tables.AddRangeAsync(defaultTables);
                await _dbContext.SaveChangesAsync();
            }
        }

        private static string NormalizeStatus(string status)
        {
            var clean = (status ?? string.Empty).Trim();
            if (clean.Equals("Available", StringComparison.OrdinalIgnoreCase)) return "Available";
            if (clean.Equals("Occupied", StringComparison.OrdinalIgnoreCase)) return "Occupied";
            if (clean.Equals("Reserved", StringComparison.OrdinalIgnoreCase)) return "Reserved";
            if (clean.Equals("Out of Service", StringComparison.OrdinalIgnoreCase)) return "Out of Service";
            return "Available";
        }

        #endregion
    }
}
