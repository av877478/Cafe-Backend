using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/customer/menu")]
    [Route("api/catalog/menu")]
    [ApiController]
    public class CustomerMenuController : ControllerBase
    {
        private readonly ICustomerMenuRepository _customerMenuRepository;

        public CustomerMenuController(ICustomerMenuRepository customerMenuRepository)
        {
            _customerMenuRepository = customerMenuRepository;
        }

        /// <summary>
        /// Retrieves the aggregated customer menu (Categories, Products, Featured Products, Currency & Settings).
        /// Routes: GET /api/customer/menu, GET /api/catalog/menu
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CustomerMenuDto), 200)]
        public async Task<IActionResult> GetCustomerMenu(
            [FromQuery] string? categoryId = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var menu = await _customerMenuRepository.GetCustomerMenuAsync(categoryId, search);
                return Ok(menu);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load customer menu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all active categories for customer menu filtering.
        /// Route: GET /api/customer/menu/categories
        /// </summary>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<CategoryDto>), 200)]
        public async Task<IActionResult> GetCustomerCategories()
        {
            try
            {
                var categories = await _customerMenuRepository.GetCustomerCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load menu categories.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves customer-visible products with optional category and search filters.
        /// Route: GET /api/customer/menu/products
        /// </summary>
        [HttpGet("products")]
        [ProducesResponseType(typeof(List<ProductDto>), 200)]
        public async Task<IActionResult> GetCustomerProducts(
            [FromQuery] string? categoryId = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var products = await _customerMenuRepository.GetCustomerProductsAsync(categoryId, search);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load products for menu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves detailed information for a single product by ID, including its customization groups.
        /// Route: GET /api/customer/menu/products/{id}
        /// </summary>
        [HttpGet("products/{id:int}")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCustomerProductById(int id)
        {
            try
            {
                var product = await _customerMenuRepository.GetCustomerProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {id} not found." });
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load product details.", error = ex.Message });
            }
        }
    }
}
