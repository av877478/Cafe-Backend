using System.IO;
using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface IProductVisualizationRepository
    {
        Task<ProductVisualizationDto?> GetVisualizationByProductIdAsync(int productId);
        Task<ProductVisualizationDto> CreateOrUpdateVisualizationAsync(int productId, UpdateProductVisualizationDto dto);
        Task<bool> DeleteVisualizationAsync(int productId);
        Task<UploadModelResultDto> UploadModelAsync(int productId, Stream fileStream, string fileName, string? contentType, long fileSize);
        Task<UploadModelResultDto> UploadImageAsync(int productId, Stream fileStream, string fileName, string? contentType, long fileSize);
    }
}
