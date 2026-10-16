using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cafe.Model.Model
{
    public class StaffMemberDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "Waiter";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateStaffMemberDto
    {
        [Required(ErrorMessage = "Staff name is required.")]
        [MaxLength(100, ErrorMessage = "Staff name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [MaxLength(50, ErrorMessage = "Role cannot exceed 50 characters.")]
        public string Role { get; set; } = "Waiter";

        [MaxLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateStaffMemberDto
    {
        [Required(ErrorMessage = "Staff name is required.")]
        [MaxLength(100, ErrorMessage = "Staff name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [MaxLength(50, ErrorMessage = "Role cannot exceed 50 characters.")]
        public string Role { get; set; } = "Waiter";

        [MaxLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class AssignStaffOrderDto
    {
        [Required(ErrorMessage = "Waiter name is required.")]
        [MaxLength(100, ErrorMessage = "Waiter name cannot exceed 100 characters.")]
        public string WaiterName { get; set; } = string.Empty;
    }

    public class StaffPanelOverviewDto
    {
        public int ActiveTableCount { get; set; }
        public List<OrderDto> Orders { get; set; } = new();
        public List<string> Waiters { get; set; } = new();
    }
}
