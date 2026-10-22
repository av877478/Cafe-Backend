using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/[controller]")]
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Retrieves all categories with optional status filter and search term.
        /// Routes: GET /api/category, GET /api/categories
        /// </summary>
        /// <param name="status">Optional status filter: 'All', 'Active', or 'Inactive'.</param>
        /// <param name="search">Optional search term matching category name or description.</param>
        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryDto>), 200)]
        public async Task<IActionResult> GetAllCategories([FromQuery] string? status = null, [FromQuery] string? search = null)
        {
            try
            {
                var categories = await _categoryRepository.GetAllCategoriesAsync(status, search);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve categories.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a single category by ID.
        /// Routes: GET /api/category/{id}, GET /api/categories/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = $"Category with ID {id} not found." });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve category.", error = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new category.
        /// Routes: POST /api/category, POST /api/categories
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _categoryRepository.CreateCategoryAsync(request);
                return CreatedAtAction(nameof(GetCategoryById), new { id = int.Parse(created.Id) }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create category.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing category.
        /// Routes: PUT /api/category/{id}, PUT /api/categories/{id}
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _categoryRepository.UpdateCategoryAsync(id, request);
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
                return StatusCode(500, new { message = "Failed to update category.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates the active status of a category.
        /// Routes: PATCH /api/category/{id}/status, PATCH /api/categories/{id}/status
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCategoryStatus(int id, [FromBody] UpdateCategoryStatusDto request)
        {
            try
            {
                var updated = await _categoryRepository.UpdateCategoryStatusAsync(id, request.IsActive);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update category status.", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes all categories from the database.
        /// Routes: DELETE /api/category/all, DELETE /api/categories/all
        /// </summary>
        [HttpDelete("all")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteAllCategories()
        {
            try
            {
                var count = await _categoryRepository.DeleteAllCategoriesAsync();
                return Ok(new { success = true, count, message = $"All {count} categories deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete all categories.", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a category by ID.
        /// Routes: DELETE /api/category/{id}, DELETE /api/categories/{id}
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var deleted = await _categoryRepository.DeleteCategoryAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"Category with ID {id} not found." });
                }
                return Ok(new { success = true, message = $"Category #{id} deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete category.", error = ex.Message });
            }
        }
    }
}
