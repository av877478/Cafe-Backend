using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// Registers a new admin account in a single direct method.
        /// Routes: POST /api/account/register, POST /api/admin/register, POST /api/admin/registration/start
        /// </summary>
        [HttpPost("register")]
        [HttpPost("/api/admin/register")]
        [HttpPost("/api/admin/registration/start")]
        [ProducesResponseType(typeof(RegistrationCompletionResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegistrationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _accountRepository.RegisterAdminAsync(request);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the account.", error = ex.Message });
            }
        }

        /// <summary>
        /// Authenticate user credentials and return JWT bearer token.
        /// Routes: POST /api/account/login, POST /api/admin/login
        /// </summary>
        [HttpPost("login")]
        [HttpPost("/api/admin/login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _accountRepository.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while logging in.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get current authenticated user profile using JWT claims.
        /// Route: GET /api/account/me
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid authentication claims." });
            }

            var user = await _accountRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user);
        }

        /// <summary>
        /// Test endpoint requiring Admin role authorization.
        /// Route: GET /api/account/admin-check
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-check")]
        public IActionResult AdminCheck()
        {
            return Ok(new
            {
                message = "Access granted: You are authorized as Admin.",
                adminName = User.Identity?.Name,
                email = User.FindFirst(ClaimTypes.Email)?.Value
            });
        }
    }
}
