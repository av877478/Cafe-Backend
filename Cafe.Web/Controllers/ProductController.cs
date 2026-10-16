using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/[controller]")]
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>
        /// Retrieves all menu products with optional category, status, and search filters.
        /// Routes: GET /api/product, GET /api/products
        /// </summary>
        /// <param name="categoryId">Optional category filter (category ID or 'All').</param>
        /// <param name="status">Optional status filter: 'All', 'Active', 'Inactive', 'Out of Stock'.</param>
        /// <param name="search">Optional search query matching name or description.</param>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductDto>), 200)]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] string? categoryId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var products = await _productRepository.GetAllProductsAsync(categoryId, status, search);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve products.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a single product by ID.
        /// Routes: GET /api/product/{id}, GET /api/products/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {id} not found." });
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve product.", error = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new product.
        /// Routes: POST /api/product, POST /api/products
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ProductDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _productRepository.CreateProductAsync(request);
                return CreatedAtAction(nameof(GetProductById), new { id = int.Parse(created.Id) }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create product.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing product.
        /// Routes: PUT /api/product/{id}, PUT /api/products/{id}
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _productRepository.UpdateProductAsync(id, request);
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update product.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates only the status of an existing product (Active | Inactive | Out of Stock).
        /// Routes: PATCH /api/product/{id}/status, PATCH /api/products/{id}/status
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProductStatus(int id, [FromBody] UpdateProductStatusDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _productRepository.UpdateProductStatusAsync(id, request.Status);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update product status.", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a product by ID.
        /// Routes: DELETE /api/product/{id}, DELETE /api/products/{id}
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var success = await _productRepository.DeleteProductAsync(id);
                if (!success)
                {
                    return NotFound(new { message = $"Product with ID {id} not found." });
                }
                return Ok(new { success = true, message = $"Product {id} deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete product.", error = ex.Message });
            }
        }
    }
}
