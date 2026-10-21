using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/[controller]")]
    [Route("api/qr-codes")]
    [ApiController]
    public class QrCodeController : ControllerBase
    {
        private readonly IQrCodeRepository _qrCodeRepository;

        public QrCodeController(IQrCodeRepository qrCodeRepository)
        {
            _qrCodeRepository = qrCodeRepository;
        }

        /// <summary>
        /// Retrieves all table QR codes.
        /// Routes: GET /api/qrcode, GET /api/qr-codes
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<QRCodeDataDto>), 200)]
        public async Task<IActionResult> GetAllQrCodes()
        {
            try
            {
                var qrs = await _qrCodeRepository.GetAllQrCodesAsync();
                return Ok(qrs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve QR codes.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific QR code by ID.
        /// Routes: GET /api/qrcode/{id}, GET /api/qr-codes/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(QRCodeDataDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetQrCodeById(int id)
        {
            try
            {
                var qr = await _qrCodeRepository.GetQrCodeByIdAsync(id);
                if (qr == null)
                {
                    return NotFound(new { message = $"QR code with ID {id} not found." });
                }
                return Ok(qr);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve QR code.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves active QR code for a specific table ID.
        /// Routes: GET /api/qrcode/table/{tableId}, GET /api/qr-codes/table/{tableId}
        /// </summary>
        [HttpGet("table/{tableId:int}")]
        [ProducesResponseType(typeof(QRCodeDataDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetQrCodeByTableId(int tableId)
        {
            try
            {
                var qr = await _qrCodeRepository.GetQrCodeByTableIdAsync(tableId);
                if (qr == null)
                {
                    return NotFound(new { message = $"No active QR code found for table ID {tableId}." });
                }
                return Ok(qr);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve table QR code.", error = ex.Message });
            }
        }

        /// <summary>
        /// Generates or regenerates a QR code for a table.
        /// Routes: POST /api/qrcode/generate, POST /api/qr-codes/generate
        /// </summary>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(QRCodeDataDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GenerateQrCode([FromBody] GenerateQrRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (!int.TryParse(request.TableId, out int tableId))
                {
                    return BadRequest(new { message = "Invalid table ID." });
                }

                var qr = await _qrCodeRepository.GenerateOrRegenerateQrAsync(tableId, request.TableNumber);
                return Ok(qr);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to generate QR code.", error = ex.Message });
            }
        }

        /// <summary>
        /// Revokes an existing QR code.
        /// Routes: POST /api/qrcode/{id}/revoke, POST /api/qr-codes/{id}/revoke
        /// </summary>
        [HttpPost("{id:int}/revoke")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RevokeQrCode(int id)
        {
            try
            {
                var success = await _qrCodeRepository.RevokeQrCodeAsync(id);
                if (!success)
                {
                    return NotFound(new { message = $"QR code with ID {id} not found." });
                }
                return Ok(new { success = true, message = $"QR code #{id} revoked successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to revoke QR code.", error = ex.Message });
            }
        }

        /// <summary>
        /// Streams high-resolution PNG image of the QR code.
        /// Routes: GET /api/qrcode/{id}/image, GET /api/qr-codes/{id}/image
        /// </summary>
        [HttpGet("{id:int}/image")]
        [Produces("image/png")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetQrImage(int id)
        {
            try
            {
                var qr = await _qrCodeRepository.GetQrCodeByIdAsync(id);
                if (qr == null)
                {
                    return NotFound();
                }

                var pngBytes = _qrCodeRepository.GenerateQrPngBytes(qr.Token);
                return File(pngBytes, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to generate QR image.", error = ex.Message });
            }
        }

        /// <summary>
        /// Downloads the table QR code PNG image file as an attachment.
        /// Routes: GET /api/qrcode/{id}/download, GET /api/qr-codes/{id}/download
        /// </summary>
        [HttpGet("{id:int}/download")]
        [Produces("image/png")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DownloadQrCode(int id)
        {
            try
            {
                var qr = await _qrCodeRepository.GetQrCodeByIdAsync(id);
                if (qr == null)
                {
                    return NotFound();
                }

                var pngBytes = _qrCodeRepository.GenerateQrPngBytes(qr.Token);
                var fileName = $"Table-{qr.TableNumber}-QR.png";
                return File(pngBytes, "image/png", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to download QR image.", error = ex.Message });
            }
        }

        /// <summary>
        /// Validates a scanned QR token and returns the assigned table number.
        /// Routes: GET /api/qrcode/resolve/{token}, GET /api/qr-codes/resolve/{token}
        /// </summary>
        [HttpGet("resolve/{token}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResolveToken(string token)
        {
            try
            {
                var (success, tableNumber, tableId, message) = await _qrCodeRepository.ResolveTokenAsync(token);
                if (!success)
                {
                    return BadRequest(new { success = false, message = message ?? "Invalid or expired QR code." });
                }

                return Ok(new { success = true, tableNumber, tableId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to resolve QR token.", error = ex.Message });
            }
        }
    }
}
