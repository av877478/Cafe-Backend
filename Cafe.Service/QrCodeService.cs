using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QRCoder;

namespace Cafe.Service
{
    public class QrCodeService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public QrCodeService(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        #region Get QR Codes

        public async Task<List<QRCodeDataDto>> GetAllQrCodesAsync()
        {
            // Auto-generate QR codes for any tables that don't have one and ensure valid URLs
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

        #region Resolve QR Token

        /// <summary>
        /// Validates a scanned QR token and resolves it to an active table number.
        /// </summary>
        public async Task<(bool Success, string? TableNumber, string? Message)> ResolveTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, null, "QR token is required.");
            }

            var cleanToken = token.Trim();
            var qr = await _dbContext.TableQrCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Token == cleanToken);

            if (qr == null)
            {
                // Fallback for mock preview tokens like 'tbl_4'
                if (cleanToken.StartsWith("tbl_", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = cleanToken.Split('_');
                    var tblNum = parts.Length > 1 ? parts[1] : "1";
                    return (true, tblNum, null);
                }

                return (false, null, "Invalid or expired QR code.");
            }

            if (!qr.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                return (false, null, "This table QR code has been deactivated.");
            }

            return (true, qr.TableNumber.ToString(), null);
        }

        #endregion

        #region Generate & Regenerate QR Code

        public async Task<QRCodeDataDto> GenerateOrRegenerateQrAsync(int tableId, int tableNumber)
        {
            var existingQrs = await _dbContext.TableQrCodes
                .Where(q => q.TableId == tableId && q.Status == "Active")
                .ToListAsync();

            foreach (var existing in existingQrs)
            {
                existing.Status = "Inactive";
                existing.RevokedAt = DateTime.UtcNow;
            }

            // Generate unique secure token and build proper customer welcome URL
            var token = GenerateSecureToken();
            var targetUrl = BuildTargetUrl(token);
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
            var targetUrl = BuildTargetUrl(token);
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(targetUrl, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        #endregion

        #region Seed & URL Helpers

        /// <summary>
        /// Builds the target customer URL for a QR code token, respecting configured base URL or LAN IP.
        /// </summary>
        public string BuildTargetUrl(string token)
        {
            var baseUrl = _configuration["Frontend:BaseUrl"] ?? _configuration["QrCode:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                // Fall back to LAN IP if detectable so phone scanning works over Wi-Fi
                baseUrl = GetLocalIpBaseUrl() ?? "http://localhost:4200";
            }

            baseUrl = baseUrl.TrimEnd('/');
            return $"{baseUrl}/customer/welcome/{token}";
        }

        private static string? GetLocalIpBaseUrl()
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530);
                if (socket.LocalEndPoint is IPEndPoint endPoint)
                {
                    return $"http://{endPoint.Address}:4200";
                }
            }
            catch
            {
                // Fallback
            }
            return null;
        }

        public async Task EnsureQrCodesForExistingTablesAsync()
        {
            var tables = await _dbContext.Tables.AsNoTracking().ToListAsync();
            var activeTableIds = await _dbContext.TableQrCodes
                .Where(q => q.Status == "Active")
                .Select(q => q.TableId)
                .Distinct()
                .ToListAsync();

            var missingTables = tables.Where(t => !activeTableIds.Contains(t.Id)).ToList();
            foreach (var table in missingTables)
            {
                var token = GenerateSecureToken();
                var targetUrl = BuildTargetUrl(token);
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

            if (missingTables.Count > 0)
            {
                await _dbContext.SaveChangesAsync();
            }

            // Also repair existing active QR codes if their ImageUrl still has the old URL format
            var activeQrs = await _dbContext.TableQrCodes.Where(q => q.Status == "Active").ToListAsync();
            bool updated = false;
            foreach (var q in activeQrs)
            {
                var correctTargetUrl = BuildTargetUrl(q.Token);
                var correctImageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(correctTargetUrl)}";
                if (q.ImageUrl != correctImageUrl)
                {
                    q.ImageUrl = correctImageUrl;
                    updated = true;
                }
            }

            if (updated)
            {
                await _dbContext.SaveChangesAsync();
            }
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
