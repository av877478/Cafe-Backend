using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class TableRepository : ITableRepository
    {
        private readonly TableService _tableService;

        public TableRepository(TableService tableService)
        {
            _tableService = tableService;
        }

        public async Task<List<TableDto>> GetAllTablesAsync(string? statusFilter = null)
        {
            return await _tableService.GetAllTablesAsync(statusFilter);
        }

        public async Task<TableDto?> GetTableByIdAsync(int id)
        {
            return await _tableService.GetTableByIdAsync(id);
        }

        public async Task<TableDto> CreateTableAsync(CreateTableDto dto)
        {
            return await _tableService.CreateTableAsync(dto);
        }

        public async Task<TableDto> UpdateTableAsync(int id, UpdateTableDto dto)
        {
            return await _tableService.UpdateTableAsync(id, dto);
        }

        public async Task<TableDto> UpdateTableStatusAsync(int id, string status)
        {
            return await _tableService.UpdateTableStatusAsync(id, status);
        }

        public async Task<bool> DeleteTableAsync(int id)
        {
            return await _tableService.DeleteTableAsync(id);
        }
    }
}
