using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.DTOs.Auth;
using RegistrationPortal.Server.Entities.Identity;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace RegistrationPortal.Server.Services;

public class AuthService : IAuthService
{
    private readonly RegistrationPortalDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IServiceProvider _serviceProvider;

    public AuthService(RegistrationPortalDbContext context, IJwtService jwtService, IPasswordHasher<User> passwordHasher, IServiceProvider serviceProvider)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _serviceProvider = serviceProvider;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        // Use raw SQL with UPPER() for case-insensitive comparison
        var userSql = $"SELECT * FROM SSDBONLINE.USERS WHERE UPPER(USERNAME) = UPPER('{loginDto.Username.Replace("'", "''")}') OR UPPER(EMAIL) = UPPER('{loginDto.Username.Replace("'", "''")}')";
        var user = await GetSingleUserAsync(userSql);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        if (!VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return null;
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Get user roles and permissions
        Console.WriteLine($"Loading roles and permissions for user ID: {user.Id}");
        var roles = await GetUserRolesAsync(user.Id);
        var permissions = await GetUserPermissionsAsync(user.Id);
        
        // Debug logging
        Console.WriteLine($"User {user.Username} roles: [{string.Join(", ", roles)}]");
        Console.WriteLine($"User {user.Username} permissions: [{string.Join(", ", permissions)}]");
        
        if (!roles.Any())
        {
            Console.WriteLine($"WARNING: User {user.Username} has no roles assigned!");
        }
        
        if (!permissions.Any())
        {
            Console.WriteLine($"WARNING: User {user.Username} has no permissions assigned!");
        }

        // Generate token
        var token = _jwtService.GenerateToken(user, roles, permissions);

        return new AuthResponseDto
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(60), // Should match JWT expiration
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Branch = user.Branch,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            },
            Roles = roles,
            Permissions = permissions
        };
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        // Check if user already exists using raw SQL with case-insensitive comparison
        var existingUserSql = $"SELECT COUNT(*) FROM SSDBONLINE.USERS WHERE UPPER(USERNAME) = UPPER('{registerDto.Username.Replace("'", "''")}') OR UPPER(EMAIL) = UPPER('{registerDto.Email.Replace("'", "''")}')";
        var existingCount = await GetSingleValueAsync(existingUserSql);
        
        if (existingCount > 0)
        {
            return null;
        }

        // Create new user
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = _passwordHasher.HashPassword(null!, registerDto.Password),
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assign default role (you might want to create a "User" role by default)
        var defaultRoleSql = $"SELECT ROLE_ID FROM SSDBONLINE.ROLES WHERE NAME = 'User'";
        var defaultRoleId = await GetSingleValueAsync(defaultRoleSql);
        
        if (defaultRoleId != null)
        {
            var insertUserRoleSql = $"INSERT INTO SSDBONLINE.USER_ROLES (USER_ID, ROLE_ID, ASSIGNED_AT) VALUES ({user.Id}, {defaultRoleId}, SYSDATE)";
            await _context.Database.ExecuteSqlRawAsync(insertUserRoleSql);
        }

        // Get user roles and permissions
        var roles = await GetUserRolesAsync(user.Id);
        var permissions = await GetUserPermissionsAsync(user.Id);

        // Generate token
        var token = _jwtService.GenerateToken(user, roles, permissions);

        return new AuthResponseDto
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(60),
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Branch = user.Branch,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            },
            Roles = roles,
            Permissions = permissions
        };
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var userSql = $"SELECT * FROM SSDBONLINE.USERS WHERE USER_ID = {userId}";
        return await GetSingleUserAsync(userSql);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var userSql = $"SELECT * FROM SSDBONLINE.USERS WHERE USERNAME = '{username.Replace("'", "''")}'";
        return await GetSingleUserAsync(userSql);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var userSql = $"SELECT * FROM SSDBONLINE.USERS WHERE EMAIL = '{email.Replace("'", "''")}'";
        return await GetSingleUserAsync(userSql);
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
    {
        try
        {
            // Use a new DbContext instance to avoid disposal issues
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<RegistrationPortalDbContext>();
            
            var rolesSql = $"SELECT r.NAME FROM \"SSDBONLINE\".\"USER_ROLES\" ur INNER JOIN \"SSDBONLINE\".\"ROLES\" r ON ur.ROLE_ID = r.ROLE_ID WHERE ur.USER_ID = {userId}";
            var roles = await dbContext.Database
                .SqlQueryRaw<string>(rolesSql)
                .ToListAsync();
            return roles;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing roles query. Error: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
    {
        try
        {
            // Use a new DbContext instance to avoid disposal issues
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<RegistrationPortalDbContext>();
            
            var permissionsSql = $"SELECT DISTINCT p.NAME FROM \"SSDBONLINE\".\"USER_ROLES\" ur INNER JOIN \"SSDBONLINE\".\"ROLE_PERMISSIONS\" rp ON ur.ROLE_ID = rp.ROLE_ID INNER JOIN \"SSDBONLINE\".\"PERMISSIONS\" p ON rp.PERMISSION_ID = p.PERMISSION_ID WHERE ur.USER_ID = {userId}";
            var permissions = await dbContext.Database
                .SqlQueryRaw<string>(permissionsSql)
                .ToListAsync();
            return permissions;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing permissions query. Error: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<bool> IsUserActiveAsync(int userId)
    {
        var userSql = $"SELECT IS_ACTIVE FROM SSDBONLINE.USERS WHERE USER_ID = {userId}";
        var result = await GetSingleValueAsync(userSql);
        return result == 1;
    }

    private bool VerifyPassword(string password, string hash)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, hash, password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }

    private async Task<User?> GetSingleUserAsync(string sql)
    {
        try
        {
            using var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var user = new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("USER_ID")),
                    Username = reader.GetString(reader.GetOrdinal("USERNAME")),
                    Email = reader.GetString(reader.GetOrdinal("EMAIL")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("PASSWORD_HASH")),
                    FirstName = reader.GetString(reader.GetOrdinal("FIRST_NAME")),
                    LastName = reader.GetString(reader.GetOrdinal("LAST_NAME")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IS_ACTIVE")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CREATED_AT")),
                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UPDATED_AT")) ? null : reader.GetDateTime(reader.GetOrdinal("UPDATED_AT")),
                    LastLoginAt = reader.IsDBNull(reader.GetOrdinal("LAST_LOGIN_AT")) ? null : reader.GetDateTime(reader.GetOrdinal("LAST_LOGIN_AT")),
                    Branch = reader.IsDBNull(reader.GetOrdinal("BRANCH")) ? null : reader.GetString(reader.GetOrdinal("BRANCH"))
                };
                return user;
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing user query: {sql}. Error: {ex.Message}");
            return null;
        }
    }

    private async Task<int?> GetSingleValueAsync(string sql)
    {
        try
        {
            using var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing SQL: {sql}. Error: {ex.Message}");
            return null;
        }
    }
}
