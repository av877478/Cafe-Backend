using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface ITableRepository
    {
        Task<List<TableDto>> GetAllTablesAsync(string? statusFilter = null);
        Task<TableDto?> GetTableByIdAsync(int id);
        Task<TableDto> CreateTableAsync(CreateTableDto dto);
        Task<TableDto> UpdateTableAsync(int id, UpdateTableDto dto);
        Task<TableDto> UpdateTableStatusAsync(int id, string status);
        Task<bool> DeleteTableAsync(int id);
    }
}
