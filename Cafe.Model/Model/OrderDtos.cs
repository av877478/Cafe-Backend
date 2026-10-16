using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cafe.Model.Model
{
    public class OrderItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public List<string> Customizations { get; set; } = new();
    }

    public class OrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? TableNumber { get; set; }
        public string ServiceType { get; set; } = "Dine-In";
        public string PaymentStatus { get; set; } = "Pending";
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string Status { get; set; } = "New";
        public DateTime OrderTime { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public DateTime? CompletedTime { get; set; }
        public int? EstimatedPrepTime { get; set; }
        public DateTime? ConfirmedTime { get; set; }
        public string? RejectionReason { get; set; }
        public string? Notes { get; set; }
        public string? WaiterName { get; set; }
    }

    public class CreateOrderItemDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "ProductId is required.")]
        public string ProductId { get; set; } = string.Empty;

        [Required(ErrorMessage = "ProductName is required.")]
        public string ProductName { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;

        [Range(0, 100000, ErrorMessage = "UnitPrice must be non-negative.")]
        public decimal UnitPrice { get; set; }

        [Range(0, 100000, ErrorMessage = "TotalPrice must be non-negative.")]
        public decimal TotalPrice { get; set; }

        public List<string>? Customizations { get; set; }
    }

    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Customer name is required.")]
        public string CustomerName { get; set; } = string.Empty;

        public string? TableNumber { get; set; }

        public string ServiceType { get; set; } = "Dine-In";

        public string PaymentStatus { get; set; } = "Pending";

        public string? PaymentMethod { get; set; }

        public string? TransactionId { get; set; }

        public string? Status { get; set; } = "New";

        public decimal Subtotal { get; set; }

        public decimal Tax { get; set; }

        public decimal Total { get; set; }

        public string? Notes { get; set; }

        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;
    }

    public class ConfirmOrderDto
    {
        [Required]
        [Range(1, 180, ErrorMessage = "Prep time must be between 1 and 180 minutes.")]
        public int PrepTime { get; set; } = 15;
    }

    public class RejectOrderDto
    {
        public string? Reason { get; set; }
    }

    public class AssignWaiterDto
    {
        [Required(ErrorMessage = "Waiter name is required.")]
        public string WaiterName { get; set; } = string.Empty;
    }

    public class UpdatePaymentStatusDto
    {
        [Required(ErrorMessage = "PaymentStatus is required.")]
        public string PaymentStatus { get; set; } = string.Empty;

        public string? PaymentMethod { get; set; }

        public string? TransactionId { get; set; }
    }

    public class CancelOrderDto
    {
        public string? Reason { get; set; }
    }

    public class ReorderResponseDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
        public bool AllItemsAvailable { get; set; } = true;
        public List<string> UnavailableItems { get; set; } = new();
    }
}
