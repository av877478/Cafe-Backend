using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class QrCodeRepository : IQrCodeRepository
    {
        private readonly QrCodeService _qrCodeService;

        public QrCodeRepository(QrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }

        public async Task<List<QRCodeDataDto>> GetAllQrCodesAsync()
        {
            return await _qrCodeService.GetAllQrCodesAsync();
        }

        public async Task<QRCodeDataDto?> GetQrCodeByIdAsync(int id)
        {
            return await _qrCodeService.GetQrCodeByIdAsync(id);
        }

        public async Task<QRCodeDataDto?> GetQrCodeByTableIdAsync(int tableId)
        {
            return await _qrCodeService.GetQrCodeByTableIdAsync(tableId);
        }

        public async Task<QRCodeDataDto> GenerateOrRegenerateQrAsync(int tableId, int tableNumber)
        {
            return await _qrCodeService.GenerateOrRegenerateQrAsync(tableId, tableNumber);
        }

        public async Task<bool> RevokeQrCodeAsync(int id)
        {
            return await _qrCodeService.RevokeQrCodeAsync(id);
        }

        public byte[] GenerateQrPngBytes(string token)
        {
            return _qrCodeService.GenerateQrPngBytes(token);
        }

        public async Task<(bool Success, string? TableNumber, string? TableId, string? Message)> ResolveTokenAsync(string token)
        {
            return await _qrCodeService.ResolveTokenAsync(token);
        }
    }
}
