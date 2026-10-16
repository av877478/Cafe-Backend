using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly ITableRepository _tableRepository;

        public TableController(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        /// <summary>
        /// Retrieves all cafe tables with optional status filtering.
        /// Routes: GET /api/table, GET /api/tables
        /// </summary>
        /// <param name="status">Optional status filter: Available, Occupied, Reserved, Out of Service, or All.</param>
        [HttpGet]
        [HttpGet("/api/tables")]
        [ProducesResponseType(typeof(List<TableDto>), 200)]
        public async Task<IActionResult> GetAllTables([FromQuery] string? status = null)
        {
            try
            {
                var tables = await _tableRepository.GetAllTablesAsync(status);
                return Ok(tables);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve tables.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a single cafe table by its ID.
        /// Routes: GET /api/table/{id}, GET /api/tables/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        [HttpGet("/api/tables/{id:int}")]
        [ProducesResponseType(typeof(TableDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTableById(int id)
        {
            try
            {
                var table = await _tableRepository.GetTableByIdAsync(id);
                if (table == null)
                {
                    return NotFound(new { message = $"Table with ID {id} not found." });
                }
                return Ok(table);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve table.", error = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new cafe table.
        /// Routes: POST /api/table, POST /api/tables
        /// </summary>
        [HttpPost]
        [HttpPost("/api/tables")]
        [ProducesResponseType(typeof(TableDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateTable([FromBody] CreateTableDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdTable = await _tableRepository.CreateTableAsync(request);
                return CreatedAtAction(nameof(GetTableById), new { id = int.Parse(createdTable.Id) }, createdTable);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create table.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing table's details (number, capacity, status).
        /// Routes: PUT /api/table/{id}, PUT /api/tables/{id}
        /// </summary>
        [HttpPut("{id:int}")]
        [HttpPut("/api/tables/{id:int}")]
        [ProducesResponseType(typeof(TableDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] UpdateTableDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _tableRepository.UpdateTableAsync(id, request);
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
                return StatusCode(500, new { message = "Failed to update table.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates only the status of an existing table.
        /// Routes: PATCH /api/table/{id}/status, PATCH /api/tables/{id}/status
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [HttpPatch("/api/tables/{id:int}/status")]
        [ProducesResponseType(typeof(TableDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateTableStatus(int id, [FromBody] UpdateTableStatusDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _tableRepository.UpdateTableStatusAsync(id, request.Status);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update table status.", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a cafe table by its ID.
        /// Routes: DELETE /api/table/{id}, DELETE /api/tables/{id}
        /// </summary>
        [HttpDelete("{id:int}")]
        [HttpDelete("/api/tables/{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteTable(int id)
        {
            try
            {
                var deleted = await _tableRepository.DeleteTableAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"Table with ID {id} not found." });
                }
                return Ok(new { success = true, message = $"Table #{id} deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete table.", error = ex.Message });
            }
        }
    }
}
