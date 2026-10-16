using System;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/customer/cart")]
    [Route("api/cart")]
    [ApiController]
    public class CustomerCartController : ControllerBase
    {
        private readonly ICustomerCartRepository _cartRepository;

        public CustomerCartController(ICustomerCartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        /// <summary>
        /// Validates cart items against live database records, calculates server-verified prices and tax rates.
        /// Routes: POST /api/customer/cart/validate, POST /api/cart/validate
        /// </summary>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ValidateCartResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ValidateCart([FromBody] ValidateCartRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _cartRepository.ValidateAndPriceCartAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to validate cart items.", error = ex.Message });
            }
        }

        /// <summary>
        /// Direct server-side cart checkout with price verification, inventory availability check and order placement.
        /// Routes: POST /api/customer/cart/checkout, POST /api/cart/checkout
        /// </summary>
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CheckoutCart([FromBody] CartCheckoutRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var order = await _cartRepository.CheckoutCartAsync(request);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to complete checkout.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves cart/checkout financial settings (currency, tax rate, payment methods, operating hours).
        /// Routes: GET /api/customer/cart/settings, GET /api/cart/settings
        /// </summary>
        [HttpGet("settings")]
        [ProducesResponseType(typeof(CheckoutSettingsDto), 200)]
        public async Task<IActionResult> GetCheckoutSettings()
        {
            try
            {
                var settings = await _cartRepository.GetCheckoutSettingsAsync();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve checkout settings.", error = ex.Message });
            }
        }
    }
}
