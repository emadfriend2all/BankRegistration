using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RegistrationPortal.Server.Constants;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.DTOs.Auth;
using RegistrationPortal.Server.DTOs.Pagination;
using RegistrationPortal.Server.Entities.Identity;
using RegistrationPortal.Server.Services;
using RegistrationPortal.Server.Repositories;

namespace RegistrationPortal.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly RegistrationPortalDbContext _context;
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserController(IAuthService authService, RegistrationPortalDbContext context, ILogger<UserController> logger, IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _authService = authService;
        _context = context;
        _logger = logger;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Users.List)]
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
    [Authorize(Policy = Permissions.Users.ViewDetails)]
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
    [Authorize(Policy = Permissions.Users.Update)]
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
    [Authorize(Policy = Permissions.Users.Update)]
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
    [Authorize(Policy = Permissions.Users.ViewDetails)]
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
    [Authorize(Policy = Permissions.Users.ViewDetails)]
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

    [HttpGet("paginated")]
    [Authorize(Policy = Permissions.Users.List)]
    public async Task<ActionResult<PaginatedResultDto<UserListDto>>> GetUsersPaginated([FromQuery] PaginationParameters parameters)
    {
        try
        {
            var result = await _userRepository.GetUsersPaginatedAsync(
                pageNumber: parameters.PageNumber,
                pageSize: parameters.PageSize,
                searchTerm: parameters.SearchTerm,
                sortBy: parameters.SortBy,
                sortDescending: parameters.SortDescending,
                status: parameters.Status);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated users");
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Users.Create)]
    public async Task<IActionResult> CreateUser([FromBody] RegisterDto registerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid user data", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            // Check existing by username or email (case-insensitive)
            using (var existsCmd = connection.CreateCommand())
            {
                existsCmd.CommandText = @"SELECT COUNT(*) FROM SSDBONLINE.USERS
                                          WHERE UPPER(USERNAME) = UPPER(:uname)
                                             OR UPPER(EMAIL) = UPPER(:email)";
                var pUser = existsCmd.CreateParameter(); pUser.ParameterName = "uname"; pUser.Value = registerDto.Username; existsCmd.Parameters.Add(pUser);
                var pEmail = existsCmd.CreateParameter(); pEmail.ParameterName = "email"; pEmail.Value = registerDto.Email; existsCmd.Parameters.Add(pEmail);
                var existsObj = await existsCmd.ExecuteScalarAsync();
                if (Convert.ToInt32(existsObj ?? 0) > 0)
                {
                    return BadRequest(new { message = "User with this username or email already exists" });
                }
            }

            // Hash password
            var passwordHash = _passwordHasher.HashPassword(null!, registerDto.Password);

            // Insert user (Oracle 11g compatible)
            using (var insertCmd = connection.CreateCommand())
            {
                insertCmd.CommandText = @"INSERT INTO SSDBONLINE.USERS
                    (USERNAME, EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, BRANCH, IS_ACTIVE, CREATED_AT)
                    VALUES (:uname, :email, :phash, :fname, :lname, :branch, :isactive, SYSDATE)";

                var uname = insertCmd.CreateParameter(); uname.ParameterName = "uname"; uname.Value = registerDto.Username; insertCmd.Parameters.Add(uname);
                var email = insertCmd.CreateParameter(); email.ParameterName = "email"; email.Value = registerDto.Email; insertCmd.Parameters.Add(email);
                var phash = insertCmd.CreateParameter(); phash.ParameterName = "phash"; phash.Value = passwordHash; insertCmd.Parameters.Add(phash);
                var fname = insertCmd.CreateParameter(); fname.ParameterName = "fname"; fname.Value = string.IsNullOrWhiteSpace(registerDto.FirstName) ? (object)DBNull.Value : registerDto.FirstName; insertCmd.Parameters.Add(fname);
                var lname = insertCmd.CreateParameter(); lname.ParameterName = "lname"; lname.Value = string.IsNullOrWhiteSpace(registerDto.LastName) ? (object)DBNull.Value : registerDto.LastName; insertCmd.Parameters.Add(lname);
                var branch = insertCmd.CreateParameter(); branch.ParameterName = "branch"; branch.Value = string.IsNullOrWhiteSpace(registerDto.Branch) ? (object)DBNull.Value : registerDto.Branch; insertCmd.Parameters.Add(branch);
                var isactive = insertCmd.CreateParameter(); isactive.ParameterName = "isactive"; isactive.Value = 1; insertCmd.Parameters.Add(isactive);

                await insertCmd.ExecuteNonQueryAsync();
            }

            return Ok(new { message = "User created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Username}", registerDto.Username);
            return StatusCode(500, new { message = "An error occurred while creating user" });
        }
    }

    [HttpPost("assign-role")]
    [Authorize(Policy = Permissions.Users.AssignRoles)]
    public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleDto assignRoleDto)
    {
        try
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            // 1) Validate User exists
            using (var checkUserCmd = connection.CreateCommand())
            {
                checkUserCmd.CommandText = @"SELECT COUNT(1) FROM ""USERS"" u WHERE u.""USER_ID"" = :userId";
                var userIdParam = checkUserCmd.CreateParameter();
                userIdParam.ParameterName = "userId";
                userIdParam.Value = assignRoleDto.UserId;
                checkUserCmd.Parameters.Add(userIdParam);

                var userCountObj = await checkUserCmd.ExecuteScalarAsync();
                var userCount = Convert.ToInt32(userCountObj ?? 0);
                if (userCount == 0)
                {
                    return NotFound(new { message = "User not found" });
                }
            }

            // 2) Validate Role exists
            using (var checkRoleCmd = connection.CreateCommand())
            {
                checkRoleCmd.CommandText = @"SELECT COUNT(1) FROM ""ROLES"" r WHERE r.""ROLE_ID"" = :roleId";
                var roleIdParam = checkRoleCmd.CreateParameter();
                roleIdParam.ParameterName = "roleId";
                roleIdParam.Value = assignRoleDto.RoleId;
                checkRoleCmd.Parameters.Add(roleIdParam);

                var roleCountObj = await checkRoleCmd.ExecuteScalarAsync();
                var roleCount = Convert.ToInt32(roleCountObj ?? 0);
                if (roleCount == 0)
                {
                    return NotFound(new { message = "Role not found" });
                }
            }

            // 3) Remove all existing roles for this user
            using (var deleteCmd = connection.CreateCommand())
            {
                deleteCmd.CommandText = @"DELETE FROM ""USER_ROLES"" WHERE ""USER_ID"" = :userId";
                var userIdParam = deleteCmd.CreateParameter();
                userIdParam.ParameterName = "userId";
                userIdParam.Value = assignRoleDto.UserId;
                deleteCmd.Parameters.Add(userIdParam);
                await deleteCmd.ExecuteNonQueryAsync();
            }

            // 4) Insert role assignment (Oracle 11g compatible)
            using (var insertCmd = connection.CreateCommand())
            {
                insertCmd.CommandText = @"
INSERT INTO ""USER_ROLES""
    (""USER_ID"", ""ROLE_ID"", ""ASSIGNED_AT"", ""ASSIGNED_BY"")
VALUES
    (:userId, :roleId, SYSDATE, :assignedBy)";

                var userIdParam = insertCmd.CreateParameter();
                userIdParam.ParameterName = "userId";
                userIdParam.Value = assignRoleDto.UserId;
                insertCmd.Parameters.Add(userIdParam);

                var roleIdParam = insertCmd.CreateParameter();
                roleIdParam.ParameterName = "roleId";
                roleIdParam.Value = assignRoleDto.RoleId;
                insertCmd.Parameters.Add(roleIdParam);

                var assignedByParam = insertCmd.CreateParameter();
                assignedByParam.ParameterName = "assignedBy";
                assignedByParam.Value = GetCurrentUserId(); // implement this to return current user ID
                insertCmd.Parameters.Add(assignedByParam);

                await insertCmd.ExecuteNonQueryAsync();
            }

            return Ok(new { message = "Role assigned to user successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user. UserId: {UserId}, RoleId: {RoleId}",
                assignRoleDto.UserId, assignRoleDto.RoleId);
            return StatusCode(500, new { message = "An error occurred while assigning role to user" });
        }
    }


    [HttpPost("remove-role")]
    [Authorize(Policy = Permissions.Users.AssignRoles)]
    public async Task<IActionResult> RemoveRoleFromUser([FromBody] AssignRoleDto assignRoleDto)
    {
        try
        {
            // Check if user exists
            var user = await _context.Users.FindAsync(assignRoleDto.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if role exists
            var role = await _context.Roles.FindAsync(assignRoleDto.RoleId);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            // Find the user role assignment
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == assignRoleDto.UserId && ur.RoleId == assignRoleDto.RoleId);

            if (userRole == null)
            {
                return BadRequest(new { message = "User does not have this role" });
            }

            // Remove role from user
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Role removed from user successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user. UserId: {UserId}, RoleId: {RoleId}",
                assignRoleDto.UserId, assignRoleDto.RoleId);
            return StatusCode(500, new { message = "An error occurred while removing role from user" });
        }
    }

    private int GetCurrentUserId()
    {
        // This is a placeholder - you'll need to implement this based on your authentication system
        // For example, if using JWT claims:
        var userIdClaim = User.FindFirst("UserId")?.Value;
        return userIdClaim != null ? int.Parse(userIdClaim) : 0;
    }
}
