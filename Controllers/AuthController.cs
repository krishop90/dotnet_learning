using BooksApi.DTOs;
using BooksApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet("users")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUsers(
            [FromQuery] string? roleFilter = "user",
            [FromQuery] string search = "",
            [FromQuery] string sort = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Fetching users for admin");
                var result = await _authService.GetUsersAsync(roleFilter, search, sort, page, pageSize);

                if (result.Users == null || !result.Users.Any())
                {
                    _logger.LogWarning("No users found");
                    return Ok(new List<UserModel>());
                }

                Response.Headers.Add("X-Total-Count", result.TotalCount.ToString());
                Response.Headers.Add("X-Total-Pages", result.TotalPages.ToString());

                _logger.LogInformation($"Successfully retrieved {result.Users.Count()} users");
                return Ok(result.Users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users");
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<SignInResponse>> Register(RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);

                if (response == null)
                    return BadRequest(new { message = "User with this email already exists" });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration");
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<SignInResponse>> Login(SignInRequest request)
        {
            try
            {
                var response = await _authService.SignInAsync(request);

                if (response == null)
                    return Unauthorized(new { message = "Invalid email or password" });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }
    }
}