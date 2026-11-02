using System;
using System.IO;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cafe.Service
{
    public class ProductVisualizationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ProductVisualizationService> _logger;

        private static readonly string[] ImageExtensions = new[]
        {
            ".jpg", ".jpeg", ".png", ".webp", ".avif", ".gif", ".svg", ".bmp", ".ico", ".tiff", ".tif", ".heic", ".heif", ".jfif"
        };

        public ProductVisualizationService(
            ApplicationDbContext dbContext,
            IFileStorageService fileStorageService,
            ILogger<ProductVisualizationService> logger)
        {
            _dbContext = dbContext;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        /// <summary>
        /// Automatically selects real 3D Cup & Plate or 3D Food Plate based on product name and category.
        /// </summary>
        public static string GetAutoModelUrlForProduct(string? productName, string? categoryName = null)
        {
            var combined = $"{productName} {categoryName}".ToLowerInvariant();

            // Food, bakery, dessert items get a real 3D serving plate
            if (combined.Contains("croissant") || combined.Contains("pizza") || combined.Contains("burger") ||
                combined.Contains("sandwich") || combined.Contains("bread") || combined.Contains("cake") ||
                combined.Contains("pastry") || combined.Contains("plate") || combined.Contains("platter") ||
                combined.Contains("toast") || combined.Contains("breakfast") || combined.Contains("salad") ||
                combined.Contains("cookie") || combined.Contains("dessert") || combined.Contains("pie"))
            {
                return "/uploads/models/real_food_plate.glb";
            }

            // By default for cafe: Authentic 3D Ceramic Coffee Cup & Saucer/Plate
            return "/uploads/models/real_cup_plate.glb";
        }

        public async Task<ProductVisualizationDto?> GetVisualizationByProductIdAsync(int productId)
        {
            var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                return null;
            }

            var vis = await _dbContext.ProductVisualizations
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.ProductId == productId);

            var autoModel = GetAutoModelUrlForProduct(product.Name);

            if (vis == null)
            {
                return new ProductVisualizationDto
                {
                    ProductId = productId,
                    ProductImageUrl = product.ImageUrl,
                    Model3DUrl = autoModel,
                    UsdzModelUrl = null,
                    Is3DEnabled = false,
                    IsAREnabled = false,
                    IsAutoRotateEnabled = true
                };
            }

            return new ProductVisualizationDto
            {
                ProductId = vis.ProductId,
                ProductImageUrl = !string.IsNullOrWhiteSpace(vis.ProductImageUrl) ? vis.ProductImageUrl : product.ImageUrl,
                Model3DUrl = !string.IsNullOrWhiteSpace(vis.Model3DUrl) ? vis.Model3DUrl : autoModel,
                UsdzModelUrl = vis.UsdzModelUrl,
                Is3DEnabled = vis.Is3DEnabled,
                IsAREnabled = vis.IsAREnabled,
                IsAutoRotateEnabled = vis.IsAutoRotateEnabled
            };
        }

        public async Task<ProductVisualizationDto> CreateOrUpdateVisualizationAsync(int productId, UpdateProductVisualizationDto dto)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} does not exist.");
            }

            var autoModel = GetAutoModelUrlForProduct(product.Name);
            var effectiveModelUrl = !string.IsNullOrWhiteSpace(dto.Model3DUrl) ? dto.Model3DUrl : autoModel;

            var vis = await _dbContext.ProductVisualizations.FirstOrDefaultAsync(v => v.ProductId == productId);
            if (vis == null)
            {
                vis = new ProductVisualization
                {
                    ProductId = productId,
                    ProductImageUrl = !string.IsNullOrWhiteSpace(dto.ProductImageUrl) ? dto.ProductImageUrl : product.ImageUrl,
                    Model3DUrl = effectiveModelUrl,
                    UsdzModelUrl = dto.UsdzModelUrl,
                    Is3DEnabled = dto.Is3DEnabled,
                    IsAREnabled = dto.IsAREnabled,
                    IsAutoRotateEnabled = dto.IsAutoRotateEnabled,
                    CreatedAt = DateTime.UtcNow
                };
                await _dbContext.ProductVisualizations.AddAsync(vis);
            }
            else
            {
                vis.ProductImageUrl = !string.IsNullOrWhiteSpace(dto.ProductImageUrl) ? dto.ProductImageUrl : product.ImageUrl;
                vis.Model3DUrl = effectiveModelUrl;
                vis.UsdzModelUrl = dto.UsdzModelUrl;
                vis.Is3DEnabled = dto.Is3DEnabled;
                vis.IsAREnabled = dto.IsAREnabled;
                vis.IsAutoRotateEnabled = dto.IsAutoRotateEnabled;
                vis.UpdatedAt = DateTime.UtcNow;
            }

            // Sync product image URL if a valid new image was provided
            if (!string.IsNullOrWhiteSpace(dto.ProductImageUrl))
            {
                product.ImageUrl = dto.ProductImageUrl;
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Saved 3D visualization settings for Product #{ProductId}", productId);

            return new ProductVisualizationDto
            {
                ProductId = vis.ProductId,
                ProductImageUrl = vis.ProductImageUrl,
                Model3DUrl = vis.Model3DUrl,
                UsdzModelUrl = vis.UsdzModelUrl,
                Is3DEnabled = vis.Is3DEnabled,
                IsAREnabled = vis.IsAREnabled,
                IsAutoRotateEnabled = vis.IsAutoRotateEnabled
            };
        }

        public async Task<bool> DeleteVisualizationAsync(int productId)
        {
            var vis = await _dbContext.ProductVisualizations.FirstOrDefaultAsync(v => v.ProductId == productId);
            if (vis == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(vis.Model3DUrl) && vis.Model3DUrl.StartsWith("/uploads/") && !vis.Model3DUrl.Contains("real_cup_plate") && !vis.Model3DUrl.Contains("real_food_plate"))
            {
                await _fileStorageService.DeleteFileAsync(vis.Model3DUrl);
            }

            _dbContext.ProductVisualizations.Remove(vis);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<UploadModelResultDto> UploadModelAsync(int productId, Stream fileStream, string fileName, string? contentType, long fileSize)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} does not exist.");
            }

            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(ext) && Array.Exists(ImageExtensions, e => e.Equals(ext, StringComparison.OrdinalIgnoreCase)))
            {
                // Route image files to UploadImageAsync
                return await UploadImageAsync(productId, fileStream, fileName, contentType, fileSize);
            }

            var savedUrl = await _fileStorageService.SaveModelFileAsync(fileStream, fileName, contentType, fileSize);

            // Automatically update or create visualization entry with this 3D model
            var vis = await _dbContext.ProductVisualizations.FirstOrDefaultAsync(v => v.ProductId == productId);
            if (vis == null)
            {
                vis = new ProductVisualization
                {
                    ProductId = productId,
                    ProductImageUrl = product.ImageUrl,
                    Model3DUrl = ext == ".usdz" ? null : savedUrl,
                    UsdzModelUrl = ext == ".usdz" ? savedUrl : null,
                    Is3DEnabled = true,
                    IsAREnabled = true,
                    IsAutoRotateEnabled = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _dbContext.ProductVisualizations.AddAsync(vis);
            }
            else
            {
                if (ext == ".usdz")
                {
                    vis.UsdzModelUrl = savedUrl;
                }
                else
                {
                    vis.Model3DUrl = savedUrl;
                }
                vis.Is3DEnabled = true;
                vis.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            return new UploadModelResultDto
            {
                Success = true,
                ModelUrl = savedUrl,
                FileName = fileName,
                FileSizeBytes = fileSize,
                Message = "3D model uploaded and linked to product successfully."
            };
        }

        public async Task<UploadModelResultDto> UploadImageAsync(int productId, Stream fileStream, string fileName, string? contentType, long fileSize)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} does not exist.");
            }

            var savedUrl = await _fileStorageService.SaveImageFileAsync(fileStream, fileName, contentType, fileSize);

            // Update product's primary image
            product.ImageUrl = savedUrl;

            // Automatically update or create visualization entry with this poster image
            var vis = await _dbContext.ProductVisualizations.FirstOrDefaultAsync(v => v.ProductId == productId);
            if (vis == null)
            {
                vis = new ProductVisualization
                {
                    ProductId = productId,
                    ProductImageUrl = savedUrl,
                    Model3DUrl = GetAutoModelUrlForProduct(product.Name),
                    Is3DEnabled = true,
                    IsAREnabled = true,
                    IsAutoRotateEnabled = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _dbContext.ProductVisualizations.AddAsync(vis);
            }
            else
            {
                vis.ProductImageUrl = savedUrl;
                if (string.IsNullOrWhiteSpace(vis.Model3DUrl))
                {
                    vis.Model3DUrl = GetAutoModelUrlForProduct(product.Name);
                }
                vis.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            return new UploadModelResultDto
            {
                Success = true,
                ModelUrl = savedUrl,
                FileName = fileName,
                FileSizeBytes = fileSize,
                Message = "Product poster image uploaded and updated successfully."
            };
        }
    }
}
