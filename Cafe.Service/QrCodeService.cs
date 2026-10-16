using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace Cafe.Service
{
    public class QrCodeService
    {
        private readonly ApplicationDbContext _dbContext;

        public QrCodeService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Get QR Codes

        public async Task<List<QRCodeDataDto>> GetAllQrCodesAsync()
        {
            // Auto-generate QR codes for any tables that don't have one
            await EnsureQrCodesForExistingTablesAsync();

            var qrs = await _dbContext.TableQrCodes
                .AsNoTracking()
                .OrderBy(q => q.TableNumber)
                .ThenByDescending(q => q.GeneratedAt)
                .Select(q => new QRCodeDataDto
                {
                    Id = q.Id.ToString(),
                    TableId = q.TableId.ToString(),
                    TableNumber = q.TableNumber,
                    Token = q.Token,
                    Status = q.Status,
                    GeneratedAt = q.GeneratedAt,
                    ImageUrl = q.ImageUrl
                })
                .ToListAsync();

            return qrs;
        }

        public async Task<QRCodeDataDto?> GetQrCodeByIdAsync(int id)
        {
            var qr = await _dbContext.TableQrCodes.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id);
            if (qr == null) return null;

            return new QRCodeDataDto
            {
                Id = qr.Id.ToString(),
                TableId = qr.TableId.ToString(),
                TableNumber = qr.TableNumber,
                Token = qr.Token,
                Status = qr.Status,
                GeneratedAt = qr.GeneratedAt,
                ImageUrl = qr.ImageUrl
            };
        }

        public async Task<QRCodeDataDto?> GetQrCodeByTableIdAsync(int tableId)
        {
            var qr = await _dbContext.TableQrCodes
                .AsNoTracking()
                .Where(q => q.TableId == tableId && q.Status == "Active")
                .OrderByDescending(q => q.GeneratedAt)
                .FirstOrDefaultAsync();

            if (qr == null) return null;

            return new QRCodeDataDto
            {
                Id = qr.Id.ToString(),
                TableId = qr.TableId.ToString(),
                TableNumber = qr.TableNumber,
                Token = qr.Token,
                Status = qr.Status,
                GeneratedAt = qr.GeneratedAt,
                ImageUrl = qr.ImageUrl
            };
        }

        #endregion

        #region Generate & Regenerate QR Code

        public async Task<QRCodeDataDto> GenerateOrRegenerateQrAsync(int tableId, int tableNumber)
        {
            // Deactivate any existing active QR code for this table
            var existingQrs = await _dbContext.TableQrCodes
                .Where(q => q.TableId == tableId && q.Status == "Active")
                .ToListAsync();

            foreach (var existing in existingQrs)
            {
                existing.Status = "Inactive";
                existing.RevokedAt = DateTime.UtcNow;
            }

            // Generate unique secure token
            var token = GenerateSecureToken();
            var targetUrl = $"http://localhost:4200/t/{token}";
            var imageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(targetUrl)}";

            var newQr = new TableQrCode
            {
                TableId = tableId,
                TableNumber = tableNumber,
                Token = token,
                Status = "Active",
                ImageUrl = imageUrl,
                GeneratedAt = DateTime.UtcNow
            };

            await _dbContext.TableQrCodes.AddAsync(newQr);
            await _dbContext.SaveChangesAsync();

            return new QRCodeDataDto
            {
                Id = newQr.Id.ToString(),
                TableId = newQr.TableId.ToString(),
                TableNumber = newQr.TableNumber,
                Token = newQr.Token,
                Status = newQr.Status,
                GeneratedAt = newQr.GeneratedAt,
                ImageUrl = newQr.ImageUrl
            };
        }

        #endregion

        #region Revoke QR Code

        public async Task<bool> RevokeQrCodeAsync(int id)
        {
            var qr = await _dbContext.TableQrCodes.FirstOrDefaultAsync(q => q.Id == id);
            if (qr == null) return false;

            qr.Status = "Inactive";
            qr.RevokedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Image & PNG Byte Generation

        public byte[] GenerateQrPngBytes(string token)
        {
            var targetUrl = $"http://localhost:4200/t/{token}";
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(targetUrl, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        #endregion

        #region Seed Helpers

        public async Task EnsureQrCodesForExistingTablesAsync()
        {
            var tables = await _dbContext.Tables.AsNoTracking().ToListAsync();
            var activeTableIds = await _dbContext.TableQrCodes
                .Where(q => q.Status == "Active")
                .Select(q => q.TableId)
                .Distinct()
                .ToListAsync();

            var missingTables = tables.Where(t => !activeTableIds.Contains(t.Id)).ToList();
            if (missingTables.Count == 0) return;

            foreach (var table in missingTables)
            {
                var token = GenerateSecureToken();
                var targetUrl = $"http://localhost:4200/t/{token}";
                var imageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(targetUrl)}";

                var qr = new TableQrCode
                {
                    TableId = table.Id,
                    TableNumber = table.Number,
                    Token = token,
                    Status = "Active",
                    ImageUrl = imageUrl,
                    GeneratedAt = DateTime.UtcNow
                };

                await _dbContext.TableQrCodes.AddAsync(qr);
            }

            await _dbContext.SaveChangesAsync();
        }

        public static string GenerateSecureToken()
        {
            var bytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        #endregion
    }
}
