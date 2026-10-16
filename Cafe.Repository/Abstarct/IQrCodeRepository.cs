using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface IQrCodeRepository
    {
        Task<List<QRCodeDataDto>> GetAllQrCodesAsync();
        Task<QRCodeDataDto?> GetQrCodeByIdAsync(int id);
        Task<QRCodeDataDto?> GetQrCodeByTableIdAsync(int tableId);
        Task<QRCodeDataDto> GenerateOrRegenerateQrAsync(int tableId, int tableNumber);
        Task<bool> RevokeQrCodeAsync(int id);
        byte[] GenerateQrPngBytes(string token);
    }
}
