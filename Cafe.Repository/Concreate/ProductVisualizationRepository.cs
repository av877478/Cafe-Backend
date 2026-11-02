using System.IO;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class ProductVisualizationRepository : IProductVisualizationRepository
    {
        private readonly ProductVisualizationService _visualizationService;

        public ProductVisualizationRepository(ProductVisualizationService visualizationService)
        {
            _visualizationService = visualizationService;
        }

        public async Task<ProductVisualizationDto?> GetVisualizationByProductIdAsync(int productId)
        {
            return await _visualizationService.GetVisualizationByProductIdAsync(productId);
        }

        public async Task<ProductVisualizationDto> CreateOrUpdateVisualizationAsync(int productId, UpdateProductVisualizationDto dto)
        {
            return await _visualizationService.CreateOrUpdateVisualizationAsync(productId, dto);
        }

        public async Task<bool> DeleteVisualizationAsync(int productId)
        {
            return await _visualizationService.DeleteVisualizationAsync(productId);
        }

        public async Task<UploadModelResultDto> UploadModelAsync(int productId, Stream fileStream, string fileName, string? contentType, long fileSize)
        {
            return await _visualizationService.UploadModelAsync(productId, fileStream, fileName, contentType, fileSize);
        }

        public async Task<UploadModelResultDto> UploadImageAsync(int productId, Stream fileStream, string fileName, string? contentType, long fileSize)
        {
            return await _visualizationService.UploadImageAsync(productId, fileStream, fileName, contentType, fileSize);
        }
    }
}
