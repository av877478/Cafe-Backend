using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface IStaffRepository
    {
        Task<List<StaffMemberDto>> GetAllStaffMembersAsync(bool activeOnly = true);
        Task<StaffMemberDto?> GetStaffMemberByIdAsync(int id);
        Task<StaffMemberDto> CreateStaffMemberAsync(CreateStaffMemberDto dto);
        Task<StaffMemberDto> UpdateStaffMemberAsync(int id, UpdateStaffMemberDto dto);
        Task<bool> DeleteStaffMemberAsync(int id);
        Task<List<OrderDto>> GetActiveDineInOrdersAsync();
        Task<StaffPanelOverviewDto> GetStaffPanelOverviewAsync();
        Task<OrderDto> AssignWaiterAsync(string orderId, string waiterName);
        Task<OrderDto> MarkOrderServedAsync(string orderId);
    }
}
