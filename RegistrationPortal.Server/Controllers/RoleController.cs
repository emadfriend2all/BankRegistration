using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Constants;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.DTOs.Auth;
using RegistrationPortal.Server.DTOs.Pagination;
using RegistrationPortal.Server.Entities.Identity;
using RegistrationPortal.Server.Repositories;

namespace RegistrationPortal.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly RegistrationPortalDbContext _context;
    private readonly ILogger<RoleController> _logger;
    private readonly IRoleRepository _roleRepository;

    public RoleController(RegistrationPortalDbContext context, ILogger<RoleController> logger, IRoleRepository roleRepository)
    {
        _context = context;
        _logger = logger;
        _roleRepository = roleRepository;
    }

    [HttpGet("paginated")]
    [Authorize(Policy = Permissions.Roles.List)]
    public async Task<ActionResult<PaginatedResultDto<RoleListDto>>> GetRolesPaginated([FromQuery] PaginationParameters parameters)
    {
        try
        {
            var result = await _roleRepository.GetRolesPaginatedAsync(
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
            _logger.LogError(ex, "Error getting paginated roles");
            return StatusCode(500, new { message = "An error occurred while retrieving roles" });
        }
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Roles.List)]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var roles = await _context.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    r.IsActive,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            return StatusCode(500, new { message = "An error occurred while retrieving roles" });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Permissions.Roles.ViewDetails)]
    public async Task<IActionResult> GetRole(int id)
    {
        try
        {
            var role = await _context.Roles
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    r.IsActive,
                    r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .Join(_context.Permissions, rp => rp.PermissionId, p => p.Id, (rp, p) => new
                {
                    p.Id,
                    p.Name,
                    p.Description
                })
                .ToListAsync();

            return Ok(new { role, permissions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role with ID: {RoleId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving role" });
        }
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Roles.Create)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
    {
        try
        {
            if (await _context.Roles.AnyAsync(r => r.Name == createRoleDto.Name))
            {
                return BadRequest(new { message = "Role with this name already exists" });
            }

            var role = new Role
            {
                Name = createRoleDto.Name,
                Description = createRoleDto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Role created successfully", role });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, new { message = "An error occurred while creating role" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.Roles.Update)]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto updateRoleDto)
    {
        try
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            // Check if name is being changed and if it already exists
            if (role.Name != updateRoleDto.Name &&
                await _context.Roles.AnyAsync(r => r.Name == updateRoleDto.Name && r.Id != id))
            {
                return BadRequest(new { message = "Role with this name already exists" });
            }

            role.Name = updateRoleDto.Name;
            role.Description = updateRoleDto.Description;
            role.IsActive = updateRoleDto.IsActive;
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Role updated successfully", role });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role with ID: {RoleId}", id);
            return StatusCode(500, new { message = "An error occurred while updating role" });
        }
    }

    [HttpPost("assign-permission")]
    [Authorize(Policy = Permissions.Roles.AssignPermissions)]
    public async Task<IActionResult> AssignPermissionToRole([FromBody] AssignPermissionDto assignPermissionDto)
    {
        try
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            // 1) Validate Role exists
            using (var checkRoleCmd = connection.CreateCommand())
            {
                checkRoleCmd.CommandText = "SELECT COUNT(1) FROM \"ROLES\" r WHERE r.\"ROLE_ID\" = :roleId";
                var roleIdParam = checkRoleCmd.CreateParameter();
                roleIdParam.ParameterName = "roleId";
                roleIdParam.Value = assignPermissionDto.RoleId;
                checkRoleCmd.Parameters.Add(roleIdParam);

                var roleCountObj = await checkRoleCmd.ExecuteScalarAsync();
                var roleCount = Convert.ToInt32(roleCountObj ?? 0);
                if (roleCount == 0)
                {
                    return NotFound(new { message = "Role not found" });
                }
            }

            // 2) Validate Permission exists
            using (var checkPermCmd = connection.CreateCommand())
            {
                checkPermCmd.CommandText = "SELECT COUNT(1) FROM \"PERMISSIONS\" p WHERE p.\"PERMISSION_ID\" = :permId";
                var permIdParam = checkPermCmd.CreateParameter();
                permIdParam.ParameterName = "permId";
                permIdParam.Value = assignPermissionDto.PermissionId;
                checkPermCmd.Parameters.Add(permIdParam);

                var permCountObj = await checkPermCmd.ExecuteScalarAsync();
                var permCount = Convert.ToInt32(permCountObj ?? 0);
                if (permCount == 0)
                {
                    return NotFound(new { message = "Permission not found" });
                }
            }

            // 3) Check duplicate assignment
            using (var checkExistingCmd = connection.CreateCommand())
            {
                checkExistingCmd.CommandText = @"
                SELECT COUNT(1)
                FROM ""ROLE_PERMISSIONS"" rp
                WHERE rp.""ROLE_ID"" = :roleId
                AND rp.""PERMISSION_ID"" = :permId";

                var roleIdParam2 = checkExistingCmd.CreateParameter();
                roleIdParam2.ParameterName = "roleId";
                roleIdParam2.Value = assignPermissionDto.RoleId;
                checkExistingCmd.Parameters.Add(roleIdParam2);

                var permIdParam2 = checkExistingCmd.CreateParameter();
                permIdParam2.ParameterName = "permId";
                permIdParam2.Value = assignPermissionDto.PermissionId;
                checkExistingCmd.Parameters.Add(permIdParam2);

                var existsObj = await checkExistingCmd.ExecuteScalarAsync();
                var exists = Convert.ToInt32(existsObj ?? 0) > 0;
                if (exists)
                {
                    return BadRequest(new { message = "Role already has this permission" });
                }
            }

            // 4) Insert assignment (Oracle 11g compatible)
            using (var insertCmd = connection.CreateCommand())
            {
                insertCmd.CommandText = @"
    INSERT INTO ""ROLE_PERMISSIONS""
        (""ROLE_ID"", ""PERMISSION_ID"", ""GRANTED_AT"")
    VALUES
        (:roleId, :permId, SYSDATE)";

                var roleIdParam = insertCmd.CreateParameter();
                roleIdParam.ParameterName = "roleId";
                roleIdParam.Value = assignPermissionDto.RoleId;
                insertCmd.Parameters.Add(roleIdParam);

                var permIdParam = insertCmd.CreateParameter();
                permIdParam.ParameterName = "permId";
                permIdParam.Value = assignPermissionDto.PermissionId;
                insertCmd.Parameters.Add(permIdParam);

                await insertCmd.ExecuteNonQueryAsync();
            }

            return Ok(new { message = "Permission assigned to role successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permission to role. RoleId: {RoleId}, PermissionId: {PermissionId}",
                assignPermissionDto.RoleId, assignPermissionDto.PermissionId);
            return StatusCode(500, new { message = "An error occurred while assigning permission to role" });
        }
    }

    [HttpPost("remove-permission")]
    [Authorize(Policy = Permissions.Roles.AssignPermissions)]
    public async Task<IActionResult> RemovePermissionFromRole([FromBody] AssignPermissionDto assignPermissionDto)
    {
        try
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            // 1) Validate Role exists
            using (var checkRoleCmd = connection.CreateCommand())
            {
                checkRoleCmd.CommandText = @"SELECT COUNT(1) FROM ""ROLES"" r WHERE r.""ROLE_ID"" = :roleId";
                var roleIdParam = checkRoleCmd.CreateParameter();
                roleIdParam.ParameterName = "roleId";
                roleIdParam.Value = assignPermissionDto.RoleId;
                checkRoleCmd.Parameters.Add(roleIdParam);

                var roleCountObj = await checkRoleCmd.ExecuteScalarAsync();
                var roleCount = Convert.ToInt32(roleCountObj ?? 0);
                if (roleCount == 0)
                {
                    return NotFound(new { message = "Role not found" });
                }
            }

            // 2) Validate Permission exists
            using (var checkPermCmd = connection.CreateCommand())
            {
                checkPermCmd.CommandText = @"SELECT COUNT(1) FROM ""PERMISSIONS"" p WHERE p.""PERMISSION_ID"" = :permId";
                var permIdParam = checkPermCmd.CreateParameter();
                permIdParam.ParameterName = "permId";
                permIdParam.Value = assignPermissionDto.PermissionId;
                checkPermCmd.Parameters.Add(permIdParam);

                var permCountObj = await checkPermCmd.ExecuteScalarAsync();
                var permCount = Convert.ToInt32(permCountObj ?? 0);
                if (permCount == 0)
                {
                    return NotFound(new { message = "Permission not found" });
                }
            }

            // 3) Check if the role has this permission
            using (var checkExistingCmd = connection.CreateCommand())
            {
                checkExistingCmd.CommandText = @"
SELECT COUNT(1)
FROM ""ROLE_PERMISSIONS"" rp
WHERE rp.""ROLE_ID"" = :roleId
AND rp.""PERMISSION_ID"" = :permId";

                var roleIdParam2 = checkExistingCmd.CreateParameter();
                roleIdParam2.ParameterName = "roleId";
                roleIdParam2.Value = assignPermissionDto.RoleId;
                checkExistingCmd.Parameters.Add(roleIdParam2);

                var permIdParam2 = checkExistingCmd.CreateParameter();
                permIdParam2.ParameterName = "permId";
                permIdParam2.Value = assignPermissionDto.PermissionId;
                checkExistingCmd.Parameters.Add(permIdParam2);

                var existsObj = await checkExistingCmd.ExecuteScalarAsync();
                var exists = Convert.ToInt32(existsObj ?? 0) > 0;
                if (!exists)
                {
                    return BadRequest(new { message = "Role does not have this permission" });
                }
            }

            // 4) Delete assignment (Oracle 11g compatible)
            using (var deleteCmd = connection.CreateCommand())
            {
                deleteCmd.CommandText = @"
DELETE FROM ""ROLE_PERMISSIONS""
WHERE ""ROLE_ID"" = :roleId
AND ""PERMISSION_ID"" = :permId";

                var roleIdParam = deleteCmd.CreateParameter();
                roleIdParam.ParameterName = "roleId";
                roleIdParam.Value = assignPermissionDto.RoleId;
                deleteCmd.Parameters.Add(roleIdParam);

                var permIdParam = deleteCmd.CreateParameter();
                permIdParam.ParameterName = "permId";
                permIdParam.Value = assignPermissionDto.PermissionId;
                deleteCmd.Parameters.Add(permIdParam);

                await deleteCmd.ExecuteNonQueryAsync();
            }

            return Ok(new { message = "Permission removed from role successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permission from role. RoleId: {RoleId}, PermissionId: {PermissionId}",
                assignPermissionDto.RoleId, assignPermissionDto.PermissionId);
            return StatusCode(500, new { message = "An error occurred while removing permission from role" });
        }
    }

    [HttpGet("permissions")]
    [Authorize(Policy = Permissions.Roles.AssignPermissions)]
    public async Task<ActionResult<List<Permission>>> GetAllPermissions()
    {
        try
        {
            var permissions = new List<Permission>();
            var query = @"
        SELECT 
            p.""PERMISSION_ID"",
            p.""NAME"",
            p.""MODULE"",
            p.""ACTION"",
            p.""DESCRIPTION""
        FROM ""PERMISSIONS"" p
        ORDER BY p.""NAME""";
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = query;
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                permissions.Add(new Permission
                {
                    Id = reader.GetInt32(reader.GetOrdinal("PERMISSION_ID")),
                    Name = reader.GetString(reader.GetOrdinal("NAME")),
                    Module = reader.GetString(reader.GetOrdinal("MODULE")),
                    Action = reader.GetString(reader.GetOrdinal("ACTION")),
                    Description = reader.IsDBNull(reader.GetOrdinal("DESCRIPTION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPTION"))
                });
            }

            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all permissions");
            return StatusCode(500, new { message = "An error occurred while retrieving permissions" });
        }
    }



    [HttpGet("permissions/admin")]
    [Authorize(Roles = RegistrationPortal.Server.Constants.Roles.Admin)]
    public async Task<IActionResult> GetAdminPermissions()
    {
        try
        {
            var permissions = new List<object>();
            
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    p.PERMISSION_ID as Id,
                    p.NAME as Name,
                    p.DESCRIPTION as Description,
                    p.CREATED_AT as CreatedAt,
                    CASE WHEN rp.ROLE_ID IS NOT NULL THEN 1 ELSE 0 END as IsAssignedToAdmin
                FROM PERMISSIONS p
                LEFT JOIN ROLE_PERMISSIONS rp ON p.PERMISSION_ID = rp.PERMISSION_ID 
                    AND rp.ROLE_ID = (SELECT r.ROLE_ID FROM ROLES r WHERE r.NAME = :adminRole)
                ORDER BY p.NAME";
            
            var adminRoleParam = command.CreateParameter();
            adminRoleParam.ParameterName = "adminRole";
            adminRoleParam.Value = RegistrationPortal.Server.Constants.Roles.Admin;
            command.Parameters.Add(adminRoleParam);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                permissions.Add(new
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    IsAssignedToAdmin = reader.GetInt32(reader.GetOrdinal("IsAssignedToAdmin")) == 1
                });
            }

            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin permissions");
            return StatusCode(500, new { message = "An error occurred while retrieving admin permissions" });
        }
    }

    [HttpGet("{id}/permissions")]
    [Authorize(Policy = Permissions.Roles.ViewDetails)]
    public async Task<IActionResult> GetRolePermissions(int id)
    {
        try
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .Join(_context.Permissions, rp => rp.PermissionId, p => p.Id, (rp, p) => new
                {
                    p.Id,
                    p.Name,
                    p.Description
                })
                .ToListAsync();

            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for role with ID: {RoleId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving role permissions" });
        }
    }
}

// Additional DTOs for role management
public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
