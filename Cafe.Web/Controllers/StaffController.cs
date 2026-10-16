using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/staff")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly IStaffRepository _staffRepository;

        public StaffController(IStaffRepository staffRepository)
        {
            _staffRepository = staffRepository;
        }

        /// <summary>
        /// Retrieves all staff members (waitstaff).
        /// Route: GET /api/staff
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<StaffMemberDto>), 200)]
        public async Task<IActionResult> GetAllStaff([FromQuery] bool activeOnly = true)
        {
            try
            {
                var staff = await _staffRepository.GetAllStaffMembersAsync(activeOnly);
                return Ok(staff);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve staff members.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a single staff member by ID.
        /// Route: GET /api/staff/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(StaffMemberDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetStaffById(int id)
        {
            try
            {
                var staff = await _staffRepository.GetStaffMemberByIdAsync(id);
                if (staff == null)
                {
                    return NotFound(new { message = $"Staff member with ID {id} not found." });
                }
                return Ok(staff);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve staff member.", error = ex.Message });
            }
        }

        /// <summary>
        /// Adds a new staff member.
        /// Route: POST /api/staff
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(StaffMemberDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffMemberDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _staffRepository.CreateStaffMemberAsync(request);
                return CreatedAtAction(nameof(GetStaffById), new { id = int.Parse(created.Id) }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create staff member.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing staff member.
        /// Route: PUT /api/staff/{id}
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(StaffMemberDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffMemberDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _staffRepository.UpdateStaffMemberAsync(id, request);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update staff member.", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a staff member.
        /// Route: DELETE /api/staff/{id}
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            try
            {
                var deleted = await _staffRepository.DeleteStaffMemberAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"Staff member with ID {id} not found." });
                }
                return Ok(new { success = true, message = $"Staff member {id} deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete staff member.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all active Dine-In table orders sorted for the Waitstaff Panel.
        /// Priority: Ready (1), Preparing (2), Confirmed (3), New (4), oldest first.
        /// Route: GET /api/staff/orders
        /// </summary>
        [HttpGet("orders")]
        [ProducesResponseType(typeof(List<OrderDto>), 200)]
        public async Task<IActionResult> GetActiveDineInOrders()
        {
            try
            {
                var orders = await _staffRepository.GetActiveDineInOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve active dine-in orders.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves complete Waitstaff Panel overview: active table count, orders, and waiters list.
        /// Route: GET /api/staff/overview
        /// </summary>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(StaffPanelOverviewDto), 200)]
        public async Task<IActionResult> GetStaffPanelOverview()
        {
            try
            {
                var overview = await _staffRepository.GetStaffPanelOverviewAsync();
                return Ok(overview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve staff panel overview.", error = ex.Message });
            }
        }

        /// <summary>
        /// Assigns or reassigns a waiter to a table order.
        /// Routes: PATCH /api/staff/orders/{id}/assign, POST /api/staff/orders/{id}/assign
        /// </summary>
        [HttpPatch("orders/{id}/assign")]
        [HttpPost("orders/{id}/assign")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignWaiter(string id, [FromBody] AssignStaffOrderDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _staffRepository.AssignWaiterAsync(id, request.WaiterName);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to assign waiter to order.", error = ex.Message });
            }
        }

        /// <summary>
        /// Marks a table order as Served.
        /// Routes: PATCH /api/staff/orders/{id}/served, POST /api/staff/orders/{id}/served
        /// </summary>
        [HttpPatch("orders/{id}/served")]
        [HttpPost("orders/{id}/served")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkOrderServed(string id)
        {
            try
            {
                var updated = await _staffRepository.MarkOrderServedAsync(id);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to mark order as served.", error = ex.Message });
            }
        }
    }
}
