using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/customer/home")]
    [Route("api/catalog/home")]
    [ApiController]
    public class CustomerHomeController : ControllerBase
    {
        private readonly ICustomerHomeRepository _customerHomeRepository;

        public CustomerHomeController(ICustomerHomeRepository customerHomeRepository)
        {
            _customerHomeRepository = customerHomeRepository;
        }

        /// <summary>
        /// Retrieves aggregated home data for the customer panel (Categories, Featured Products, Popular Products, Settings).
        /// Routes: GET /api/customer/home, GET /api/catalog/home
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CustomerHomeDto), 200)]
        public async Task<IActionResult> GetHomeData()
        {
            try
            {
                var data = await _customerHomeRepository.GetCustomerHomeDataAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load customer home data.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves featured products for the customer panel.
        /// Routes: GET /api/customer/home/featured, GET /api/catalog/home/featured
        /// </summary>
        [HttpGet("featured")]
        [ProducesResponseType(typeof(List<ProductDto>), 200)]
        public async Task<IActionResult> GetFeaturedProducts()
        {
            try
            {
                var featured = await _customerHomeRepository.GetFeaturedProductsAsync();
                return Ok(featured);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load featured products.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves popular products for the customer panel.
        /// Routes: GET /api/customer/home/popular, GET /api/catalog/home/popular
        /// </summary>
        [HttpGet("popular")]
        [ProducesResponseType(typeof(List<ProductDto>), 200)]
        public async Task<IActionResult> GetPopularProducts()
        {
            try
            {
                var popular = await _customerHomeRepository.GetPopularProductsAsync();
                return Ok(popular);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load popular products.", error = ex.Message });
            }
        }
    }
}
