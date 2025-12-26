using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.Entities.Identity;
using RegistrationPortal.Server.Services;

namespace RegistrationPortal.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly RegistrationPortalDbContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(IAuthService authService, RegistrationPortalDbContext context, ILogger<UserController> logger)
    {
        _authService = authService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.IsActive,
                    u.CreatedAt,
                    u.LastLoginAt
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.IsActive,
                    u.CreatedAt,
                    u.LastLoginAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _authService.GetUserRolesAsync(id);
            var permissions = await _authService.GetUserPermissionsAsync(id);

            return Ok(new { user, roles, permissions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user with ID: {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving user" });
        }
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "User activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user with ID: {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while activating user" });
        }
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user with ID: {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while deactivating user" });
        }
    }

    [HttpGet("{id}/roles")]
    public async Task<IActionResult> GetUserRoles(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _authService.GetUserRolesAsync(id);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user with ID: {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving user roles" });
        }
    }

    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetUserPermissions(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var permissions = await _authService.GetUserPermissionsAsync(id);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user with ID: {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving user permissions" });
        }
    }
}
