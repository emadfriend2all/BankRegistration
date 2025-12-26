using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationPortal.Server.DTOs.Auth;
using RegistrationPortal.Server.Services;

namespace RegistrationPortal.Server.Controllers;

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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid login data", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(new
            {
                message = "Login successful",
                token = result.Token,
                expiration = result.Expiration,
                user = result.User,
                roles = result.Roles,
                permissions = result.Permissions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", loginDto.Username);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid registration data", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (result == null)
            {
                return Conflict(new { message = "Username or email already exists" });
            }

            return CreatedAtAction(nameof(Login), new
            {
                message = "Registration successful",
                token = result.Token,
                expiration = result.Expiration,
                user = result.User,
                roles = result.Roles,
                permissions = result.Permissions
            }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Username}", registerDto.Username);
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            // This would be handled by JWT middleware in practice, but we can provide an endpoint for testing
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = await _authService.GetUserByIdAsync(int.Parse(userId));
            if (user == null || !await _authService.IsUserActiveAsync(user.Id))
            {
                return Unauthorized(new { message = "User not found or inactive" });
            }

            var roles = await _authService.GetUserRolesAsync(user.Id);
            var permissions = await _authService.GetUserPermissionsAsync(user.Id);

            return Ok(new
            {
                message = "Token is valid",
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    isActive = user.IsActive
                },
                roles,
                permissions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token validation");
            return StatusCode(500, new { message = "An error occurred during token validation" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User not found in token" });
            }

            var user = await _authService.GetUserByIdAsync(int.Parse(userIdClaim.Value));
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _authService.GetUserRolesAsync(user.Id);
            var permissions = await _authService.GetUserPermissionsAsync(user.Id);

            return Ok(new
            {
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    branch = user.Branch,
                    isActive = user.IsActive,
                    createdAt = user.CreatedAt,
                    lastLoginAt = user.LastLoginAt
                },
                roles,
                permissions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred while getting user information" });
        }
    }
}
