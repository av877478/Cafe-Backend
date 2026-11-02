using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cafe.Service
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalFileStorageService> _logger;
        private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB limit

        // Supported 3D formats
        private static readonly HashSet<string> AllowedModelExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".glb",
            ".gltf",
            ".usdz",
            ".bin",
            ".obj"
        };

        // Supported Image formats (Any image upload extension)
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".avif",
            ".gif",
            ".svg",
            ".bmp",
            ".ico",
            ".tiff",
            ".tif",
            ".heic",
            ".heif",
            ".jfif",
            ".pjpeg",
            ".pjp"
        };

        public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> SaveModelFileAsync(Stream fileStream, string originalFileName, string? contentType, long fileSize)
        {
            var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant();

            // If an image format was supplied to model endpoint, route to image saving seamlessly
            if (!string.IsNullOrWhiteSpace(extension) && AllowedImageExtensions.Contains(extension))
            {
                return await SaveImageFileAsync(fileStream, originalFileName, contentType, fileSize);
            }

            return await SaveFileInternalAsync(fileStream, originalFileName, fileSize, AllowedModelExtensions, "models");
        }

        public async Task<string> SaveImageFileAsync(Stream fileStream, string originalFileName, string? contentType, long fileSize)
        {
            return await SaveFileInternalAsync(fileStream, originalFileName, fileSize, AllowedImageExtensions, "images");
        }

        private async Task<string> SaveFileInternalAsync(
            Stream fileStream,
            string originalFileName,
            long fileSize,
            HashSet<string> allowedExtensions,
            string subFolder)
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                throw new ArgumentException("The uploaded file is empty.", nameof(fileStream));
            }

            if (fileSize > MaxFileSizeBytes)
            {
                throw new InvalidOperationException($"File size ({fileSize / (1024 * 1024)} MB) exceeds the maximum allowed limit of 50 MB.");
            }

            var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"Unsupported file format '{extension}'. Allowed formats: {string.Join(", ", allowedExtensions)}");
            }

            var baseDir = AppContext.BaseDirectory;
            var uploadsFolder = Path.Combine(baseDir, "wwwroot", "uploads", subFolder);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var sanitizedBaseName = Path.GetFileNameWithoutExtension(originalFileName).Replace(" ", "_");
            if (sanitizedBaseName.Length > 30) sanitizedBaseName = sanitizedBaseName.Substring(0, 30);

            var uniqueFileName = $"{sanitizedBaseName}_{Guid.NewGuid():N}{extension}";
            var destinationFilePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var outputStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await fileStream.CopyToAsync(outputStream);
            }

            _logger.LogInformation("File saved to disk ({Folder}): {Path}", subFolder, destinationFilePath);

            // Return relative URL for static file serving
            return $"/uploads/{subFolder}/{uniqueFileName}";
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return Task.FromResult(false);

            try
            {
                var cleanPath = fileUrl.TrimStart('/');
                var filePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", cleanPath.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted file: {Path}", filePath);
                    return Task.FromResult(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file from URL: {Url}", fileUrl);
            }

            return Task.FromResult(false);
        }
    }
}
