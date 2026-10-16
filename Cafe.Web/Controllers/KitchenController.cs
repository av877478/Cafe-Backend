using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/kitchen")]
    [ApiController]
    public class KitchenController : ControllerBase
    {
        private readonly IKitchenRepository _kitchenRepository;

        public KitchenController(IKitchenRepository kitchenRepository)
        {
            _kitchenRepository = kitchenRepository;
        }

        /// <summary>
        /// Retrieves the 3-column Kitchen Kanban Board: New (Confirmed), Preparing, and Ready orders.
        /// Routes: GET /api/kitchen, GET /api/kitchen/board
        /// </summary>
        [HttpGet]
        [HttpGet("board")]
        [ProducesResponseType(typeof(KitchenBoardDto), 200)]
        public async Task<IActionResult> GetKitchenBoard()
        {
            try
            {
                var board = await _kitchenRepository.GetKitchenBoardAsync();
                return Ok(board);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve kitchen board.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all active orders currently in the kitchen (Confirmed, Preparing, Ready).
        /// Routes: GET /api/kitchen/orders
        /// </summary>
        [HttpGet("orders")]
        [ProducesResponseType(typeof(List<OrderDto>), 200)]
        public async Task<IActionResult> GetActiveKitchenOrders()
        {
            try
            {
                var orders = await _kitchenRepository.GetActiveOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve active kitchen orders.", error = ex.Message });
            }
        }

        /// <summary>
        /// Advances an order to the next stage in the kitchen pipeline:
        /// Confirmed -> Preparing -> Ready -> Served / Picked Up.
        /// Routes: POST /api/kitchen/orders/{id}/advance, PATCH /api/kitchen/orders/{id}/advance
        /// </summary>
        [HttpPost("orders/{id}/advance")]
        [HttpPatch("orders/{id}/advance")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AdvanceOrderStatus(string id)
        {
            try
            {
                var updated = await _kitchenRepository.AdvanceOrderStatusAsync(id);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to advance order status.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates the kitchen status of an order directly (Preparing, Ready, Served).
        /// Routes: PATCH /api/kitchen/orders/{id}/status
        /// </summary>
        [HttpPatch("orders/{id}/status")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateKitchenStatus(string id, [FromBody] UpdateKitchenStatusDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _kitchenRepository.UpdateOrderStatusAsync(id, request.Status);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update kitchen status.", error = ex.Message });
            }
        }
    }
}
