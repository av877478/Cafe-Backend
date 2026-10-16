using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class StaffRepository : IStaffRepository
    {
        private readonly StaffService _staffService;

        public StaffRepository(StaffService staffService)
        {
            _staffService = staffService;
        }

        public async Task<List<StaffMemberDto>> GetAllStaffMembersAsync(bool activeOnly = true)
        {
            return await _staffService.GetAllStaffMembersAsync(activeOnly);
        }

        public async Task<StaffMemberDto?> GetStaffMemberByIdAsync(int id)
        {
            return await _staffService.GetStaffMemberByIdAsync(id);
        }

        public async Task<StaffMemberDto> CreateStaffMemberAsync(CreateStaffMemberDto dto)
        {
            return await _staffService.CreateStaffMemberAsync(dto);
        }

        public async Task<StaffMemberDto> UpdateStaffMemberAsync(int id, UpdateStaffMemberDto dto)
        {
            return await _staffService.UpdateStaffMemberAsync(id, dto);
        }

        public async Task<bool> DeleteStaffMemberAsync(int id)
        {
            return await _staffService.DeleteStaffMemberAsync(id);
        }

        public async Task<List<OrderDto>> GetActiveDineInOrdersAsync()
        {
            return await _staffService.GetActiveDineInOrdersAsync();
        }

        public async Task<StaffPanelOverviewDto> GetStaffPanelOverviewAsync()
        {
            return await _staffService.GetStaffPanelOverviewAsync();
        }

        public async Task<OrderDto> AssignWaiterAsync(string orderId, string waiterName)
        {
            return await _staffService.AssignWaiterAsync(orderId, waiterName);
        }

        public async Task<OrderDto> MarkOrderServedAsync(string orderId)
        {
            return await _staffService.MarkOrderServedAsync(orderId);
        }
    }
}
