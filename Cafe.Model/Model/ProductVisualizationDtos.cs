using System.ComponentModel.DataAnnotations;

namespace Cafe.Model.Model
{
    public class ProductVisualizationDto
    {
        public int ProductId { get; set; }
        public string? ProductImageUrl { get; set; }
        public string? Model3DUrl { get; set; }
        public string? UsdzModelUrl { get; set; }
        public bool Is3DEnabled { get; set; } = true;
        public bool IsAREnabled { get; set; } = true;
        public bool IsAutoRotateEnabled { get; set; } = true;
    }

    public class UpdateProductVisualizationDto
    {
        public string? ProductImageUrl { get; set; }

        public string? Model3DUrl { get; set; }

        public string? UsdzModelUrl { get; set; }

        public bool Is3DEnabled { get; set; } = true;

        public bool IsAREnabled { get; set; } = true;

        public bool IsAutoRotateEnabled { get; set; } = true;
    }

    public class UploadModelResultDto
    {
        public bool Success { get; set; } = true;
        public string ModelUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
