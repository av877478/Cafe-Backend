using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cafe.Model.Model
{
    [Table("ProductVisualizations")]
    public class ProductVisualization
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [MaxLength(1000)]
        public string? ProductImageUrl { get; set; }

        [MaxLength(1000)]
        public string? Model3DUrl { get; set; }

        [MaxLength(1000)]
        public string? UsdzModelUrl { get; set; }

        public bool Is3DEnabled { get; set; } = true;

        public bool IsAREnabled { get; set; } = true;

        public bool IsAutoRotateEnabled { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
