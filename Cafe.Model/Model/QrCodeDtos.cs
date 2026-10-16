using System;
using System.ComponentModel.DataAnnotations;

namespace Cafe.Model.Model
{
    public class QRCodeDataDto
    {
        public string Id { get; set; } = string.Empty;
        public string TableId { get; set; } = string.Empty;
        public int TableNumber { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Status { get; set; } = "Active"; // Active | Inactive
        public DateTime GeneratedAt { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class GenerateQrRequestDto
    {
        [Required]
        public string TableId { get; set; } = string.Empty;

        [Required]
        [Range(1, 9999)]
        public int TableNumber { get; set; }
    }

    public class RevokeQrRequestDto
    {
        [Required]
        public string Id { get; set; } = string.Empty;
    }
}
