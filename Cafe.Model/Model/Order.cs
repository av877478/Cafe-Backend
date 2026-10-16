using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cafe.Model.Model
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? TableNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string ServiceType { get; set; } = "Dine-In"; // Dine-In, Takeaway, Delivery

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending"; // Paid, Pending, Failed, Refunded

        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // Online, Cash, Card, UPI

        [MaxLength(100)]
        public string? TransactionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "New"; // New, Waiting for Confirmation, Confirmed, Preparing, Ready, Served, Picked Up, Completed, Cancelled, Rejected

        public DateTime OrderTime { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public DateTime? CompletedTime { get; set; }

        public int? EstimatedPrepTime { get; set; }

        public DateTime? ConfirmedTime { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? WaiterName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation collection
        public List<OrderItem> Items { get; set; } = new();
    }
}
