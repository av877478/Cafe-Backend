using System.IO;
using System.Threading.Tasks;

namespace Cafe.Service
{
    public interface IFileStorageService
    {
        Task<string> SaveModelFileAsync(Stream fileStream, string originalFileName, string? contentType, long fileSize);
        Task<string> SaveImageFileAsync(Stream fileStream, string originalFileName, string? contentType, long fileSize);
        Task<bool> DeleteFileAsync(string fileUrl);
    }
}
