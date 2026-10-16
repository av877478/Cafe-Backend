using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/[controller]")]
    [Route("api/orders")]
    [Route("api/customer/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Retrieves all orders with optional status, service type, payment status, customer, table, and search filters.
        /// Routes: GET /api/order, GET /api/orders
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<OrderDto>), 200)]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] string? status = null,
            [FromQuery] string? serviceType = null,
            [FromQuery] string? paymentStatus = null,
            [FromQuery] string? customerName = null,
            [FromQuery] string? tableNumber = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var orders = await _orderRepository.GetAllOrdersAsync(status, serviceType, paymentStatus, customerName, tableNumber, search);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve orders.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a single order by ID or order number (e.g. 'ORD-1001').
        /// Routes: GET /api/order/{id}, GET /api/orders/{id}
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetOrderById(string id)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = $"Order '{id}' not found." });
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve order.", error = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new order.
        /// Routes: POST /api/order, POST /api/orders
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _orderRepository.CreateOrderAsync(request);
                return CreatedAtAction(nameof(GetOrderById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create order.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates the status of an order (e.g. 'Preparing', 'Ready', 'Served', 'Completed', 'Cancelled').
        /// Routes: PATCH /api/order/{id}/status, PATCH /api/orders/{id}/status
        /// </summary>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _orderRepository.UpdateOrderStatusAsync(id, request.Status);
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
                return StatusCode(500, new { message = "Failed to update order status.", error = ex.Message });
            }
        }

        /// <summary>
        /// Confirms an order with an estimated preparation time in minutes.
        /// Routes: PATCH /api/order/{id}/confirm, PATCH /api/orders/{id}/confirm
        /// </summary>
        [HttpPatch("{id}/confirm")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ConfirmOrder(string id, [FromBody] ConfirmOrderDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _orderRepository.ConfirmOrderAsync(id, request.PrepTime);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to confirm order.", error = ex.Message });
            }
        }

        /// <summary>
        /// Rejects an order with an optional reason.
        /// Routes: PATCH /api/order/{id}/reject, PATCH /api/orders/{id}/reject
        /// </summary>
        [HttpPatch("{id}/reject")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RejectOrder(string id, [FromBody] RejectOrderDto request)
        {
            try
            {
                var updated = await _orderRepository.RejectOrderAsync(id, request.Reason);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to reject order.", error = ex.Message });
            }
        }

        /// <summary>
        /// Assigns a waiter to a dine-in order.
        /// Routes: PATCH /api/order/{id}/waiter, PATCH /api/orders/{id}/waiter
        /// </summary>
        [HttpPatch("{id}/waiter")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignWaiter(string id, [FromBody] AssignWaiterDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _orderRepository.AssignWaiterAsync(id, request.WaiterName);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to assign waiter.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates payment status and details (e.g. Paid, Pending, Refunded).
        /// Routes: PATCH /api/order/{id}/payment, PATCH /api/orders/{id}/payment
        /// </summary>
        [HttpPatch("{id}/payment")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePaymentStatus(string id, [FromBody] UpdatePaymentStatusDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _orderRepository.UpdatePaymentStatusAsync(id, request.PaymentStatus, request.PaymentMethod, request.TransactionId);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update payment status.", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes an order by ID or order number.
        /// Routes: DELETE /api/order/{id}, DELETE /api/orders/{id}
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            try
            {
                var success = await _orderRepository.DeleteOrderAsync(id);
                if (!success)
                {
                    return NotFound(new { message = $"Order '{id}' not found." });
                }
                return Ok(new { success = true, message = $"Order '{id}' deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete order.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves past orders for a specific customer by name (for customer profile orders tab).
        /// Routes: GET /api/orders/customer/{customerName}, GET /api/customer/orders/customer/{customerName}
        /// </summary>
        [HttpGet("customer/{customerName}")]
        [ProducesResponseType(typeof(List<OrderDto>), 200)]
        public async Task<IActionResult> GetCustomerOrders(string customerName)
        {
            try
            {
                var orders = await _orderRepository.GetCustomerOrdersAsync(customerName);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load customer orders.", error = ex.Message });
            }
        }

        /// <summary>
        /// Allows a customer or staff to cancel an order that is still pending or waiting for confirmation.
        /// Routes: POST /api/orders/{id}/cancel, POST /api/customer/orders/{id}/cancel
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CancelOrder(string id, [FromBody] CancelOrderDto? request = null)
        {
            try
            {
                var cancelled = await _orderRepository.CancelCustomerOrderAsync(id, request?.Reason);
                return Ok(cancelled);
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
                return StatusCode(500, new { message = "Failed to cancel order.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves items and live availability for reordering a past order.
        /// Routes: GET /api/orders/{id}/reorder, GET /api/customer/orders/{id}/reorder
        /// </summary>
        [HttpGet("{id}/reorder")]
        [ProducesResponseType(typeof(ReorderResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetReorderDetails(string id)
        {
            try
            {
                var reorder = await _orderRepository.GetReorderDetailsAsync(id);
                return Ok(reorder);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load reorder details.", error = ex.Message });
            }
        }
    }
}
