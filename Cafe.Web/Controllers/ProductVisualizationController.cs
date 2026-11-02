using System;
using System.IO;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    public class Upload3DModelRequest
    {
        public IFormFile? File { get; set; }
    }

    [ApiController]
    [Route("api/products/{productId:int}/visualization")]
    public class ProductVisualizationController : ControllerBase
    {
        private readonly IProductVisualizationRepository _visualizationRepository;

        public ProductVisualizationController(IProductVisualizationRepository visualizationRepository)
        {
            _visualizationRepository = visualizationRepository;
        }

        /// <summary>
        /// Retrieves the 3D and AR visualization configuration for a product.
        /// Route: GET /api/products/{productId}/visualization
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProductVisualizationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVisualization([FromRoute] int productId)
        {
            try
            {
                var vis = await _visualizationRepository.GetVisualizationByProductIdAsync(productId);
                if (vis == null)
                {
                    return NotFound(new { message = $"Product with ID #{productId} was not found." });
                }
                return Ok(vis);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while retrieving product visualization.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Creates or updates the 3D/AR visualization configuration for a product.
        /// Route: PUT /api/products/{productId}/visualization
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(ProductVisualizationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVisualization([FromRoute] int productId, [FromBody] UpdateProductVisualizationDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _visualizationRepository.CreateOrUpdateVisualizationAsync(productId, request);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while saving visualization settings.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes the 3D visualization configuration and associated files for a product.
        /// Route: DELETE /api/products/{productId}/visualization
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVisualization([FromRoute] int productId)
        {
            try
            {
                var deleted = await _visualizationRepository.DeleteVisualizationAsync(productId);
                if (!deleted)
                {
                    return NotFound(new { message = $"No visualization configuration found for product #{productId}." });
                }

                return Ok(new { success = true, message = $"Visualization for product #{productId} removed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while deleting product visualization.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Uploads a 3D model asset (GLB, GLTF, USDZ) or image for a product.
        /// Route: POST /api/products/{productId}/visualization/upload-model
        /// </summary>
        [HttpPost("upload-model")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadModelResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadModel([FromRoute] int productId, [FromForm] Upload3DModelRequest request)
        {
            var file = request?.File;
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was provided in the request." });
            }

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _visualizationRepository.UploadModelAsync(
                    productId,
                    stream,
                    file.FileName,
                    file.ContentType,
                    file.Length);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while uploading file.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Uploads any image format (JPG, PNG, WEBP, AVIF, SVG, GIF, BMP, etc.) for a product poster.
        /// Route: POST /api/products/{productId}/visualization/upload-image
        /// </summary>
        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadModelResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadImage([FromRoute] int productId, [FromForm] Upload3DModelRequest request)
        {
            var file = request?.File;
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No image file was provided in the request." });
            }

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _visualizationRepository.UploadImageAsync(
                    productId,
                    stream,
                    file.FileName,
                    file.ContentType,
                    file.Length);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while uploading image.",
                    error = ex.Message
                });
            }
        }
    }
}
