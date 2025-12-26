using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Constants;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.Entities.Identity;

namespace RegistrationPortal.Server.Services
{
    public class SeederService : ISeederService
    {
        private readonly RegistrationPortalDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public SeederService(RegistrationPortalDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task SeedDataAsync()
        {
            try
            {
                await SeedPermissionsAsync();
                await SeedRolesAsync();
                await SeedAdminUserAsync();
                await SeedReviewerUserAsync();
                await SeedManagerUserAsync();
                await SeedUserRolesAsync();
                await AssignPermissionsToRolesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seeding error: {ex.Message}");
            }
        }

        private async Task SeedPermissionsAsync()
        {
            try
            {
                var allPermissions = Permissions.All;
                
                foreach (var permissionName in allPermissions)
                {
                    var checkSql = $"SELECT COUNT(*) FROM SSDBONLINE.PERMISSIONS WHERE NAME = '{permissionName.Replace("'", "''")}'";
                    var result = await GetSingleValueAsync(checkSql);
                    
                    if (result == 0)
                    {
                        // Extract module and action from permission name (e.g., "customers.list" -> module="customers", action="list")
                        var parts = permissionName.Split('.');
                        var module = parts.Length > 1 ? parts[0] : "Unknown";
                        var action = parts.Length > 1 ? parts[1] : permissionName;
                        var description = $"{char.ToUpper(module[0])}{module.Substring(1)} {char.ToUpper(action[0])}{action.Substring(1)}";
                        
                        var insertSql = $"INSERT INTO SSDBONLINE.PERMISSIONS (NAME, DESCRIPTION, MODULE, ACTION, IS_ACTIVE, CREATED_AT) VALUES ('{permissionName.Replace("'", "''")}', '{description.Replace("'", "''")}', '{module.Replace("'", "''")}', '{action.Replace("'", "''")}', 1, SYSDATE)";
                        await _context.Database.ExecuteSqlRawAsync(insertSql);
                        Console.WriteLine($"Created permission: {permissionName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding permissions: {ex.Message}");
            }
        }

        private async Task SeedRolesAsync()
        {
            try
            {
                var roles = new List<Role>
                {
                    new Role { Name = "Admin", Description = "System administrator with full access", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Role { Name = "Manager", Description = "Manager with customer review access", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Role { Name = "Reviewer", Description = "Reviewer with review and customer management access", IsActive = true, CreatedAt = DateTime.UtcNow }
                };

                foreach (var role in roles)
                {
                    var checkSql = $"SELECT COUNT(*) FROM \"SSDBONLINE\".\"ROLES\" WHERE \"NAME\" = '{role.Name.Replace("'", "''")}'";
                    var result = await GetSingleValueAsync(checkSql);
                    
                    if (result == 0)
                    {
                        var insertSql = $"INSERT INTO \"SSDBONLINE\".\"ROLES\" (NAME, DESCRIPTION, IS_ACTIVE, CREATED_AT) VALUES ('{role.Name.Replace("'", "''")}', '{role.Description.Replace("'", "''")}', 1, SYSDATE)";
                        await _context.Database.ExecuteSqlRawAsync(insertSql);
                        Console.WriteLine($"Created role: {role.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding roles: {ex.Message}");
            }
        }

        private async Task SeedAdminUserAsync()
        {
            try
            {
                // Check if admin user exists using string interpolation
                var checkSql = "SELECT COUNT(*) FROM \"SSDBONLINE\".\"USERS\" WHERE \"USERNAME\" = 'admin'";
                var result = await GetSingleValueAsync(checkSql);
                
                if (result == 0)
                {
                    Console.WriteLine("Creating admin user...");
                    
                    var passwordHash = _passwordHasher.HashPassword(null!, "Admin123!");
                    // Use string interpolation to avoid Oracle parameter binding issues
                    var insertSql = $"INSERT INTO \"SSDBONLINE\".\"USERS\" (USERNAME, EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, IS_ACTIVE, CREATED_AT) VALUES ('admin', 'admin@bank.com', '{passwordHash.Replace("'", "''")}', 'System', 'Administrator', 1, SYSDATE)";
                    
                    await _context.Database.ExecuteSqlRawAsync(insertSql);
                    Console.WriteLine("Admin user created successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding admin user: {ex.Message}");
            }
        }

        private async Task SeedReviewerUserAsync()
        {
            try
            {
                // Check if reviewer user exists
                var checkSql = "SELECT COUNT(*) FROM \"SSDBONLINE\".\"USERS\" WHERE \"USERNAME\" = 'reviewer'";
                var result = await GetSingleValueAsync(checkSql);
                
                if (result == 0)
                {
                    Console.WriteLine("Creating reviewer user...");
                    
                    var passwordHash = _passwordHasher.HashPassword(null!, "Reviewer123!");
                    var insertSql = $"INSERT INTO \"SSDBONLINE\".\"USERS\" (USERNAME, EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, IS_ACTIVE, CREATED_AT) VALUES ('reviewer', 'reviewer@bank.com', '{passwordHash.Replace("'", "''")}', 'Review', 'User', 1, SYSDATE)";
                    
                    await _context.Database.ExecuteSqlRawAsync(insertSql);
                    Console.WriteLine("Reviewer user created successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding reviewer user: {ex.Message}");
            }
        }

        private async Task SeedManagerUserAsync()
        {
            try
            {
                // Check if manager user exists
                var checkSql = "SELECT COUNT(*) FROM \"SSDBONLINE\".\"USERS\" WHERE \"USERNAME\" = 'manager'";
                var result = await GetSingleValueAsync(checkSql);
                
                if (result == 0)
                {
                    Console.WriteLine("Creating manager user...");
                    
                    var passwordHash = _passwordHasher.HashPassword(null!, "Manager123!");
                    var insertSql = $"INSERT INTO \"SSDBONLINE\".\"USERS\" (USERNAME, EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, IS_ACTIVE, CREATED_AT) VALUES ('manager', 'manager@bank.com', '{passwordHash.Replace("'", "''")}', 'Manager', 'User', 1, SYSDATE)";
                    
                    await _context.Database.ExecuteSqlRawAsync(insertSql);
                    Console.WriteLine("Manager user created successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding manager user: {ex.Message}");
            }
        }

        private async Task SeedUserRolesAsync()
        {
            try
            {
                // Get admin user ID
                var adminUserSql = "SELECT USER_ID FROM \"SSDBONLINE\".\"USERS\" WHERE \"USERNAME\" = 'admin'";
                var adminUserId = await GetSingleValueAsync(adminUserSql);
                
                if (adminUserId == null)
                {
                    Console.WriteLine("Admin user not found, cannot assign roles");
                    return;
                }
                
                // Get role IDs
                var adminRoleSql = "SELECT ROLE_ID FROM \"SSDBONLINE\".\"ROLES\" WHERE \"NAME\" = 'Admin'";
                var adminRoleId = await GetSingleValueAsync(adminRoleSql);
                
                var reviewerRoleSql = "SELECT ROLE_ID FROM \"SSDBONLINE\".\"ROLES\" WHERE \"NAME\" = 'Reviewer'";
                var reviewerRoleId = await GetSingleValueAsync(reviewerRoleSql);
                
                var managerRoleSql = "SELECT ROLE_ID FROM \"SSDBONLINE\".\"ROLES\" WHERE \"NAME\" = 'Manager'";
                var managerRoleId = await GetSingleValueAsync(managerRoleSql);
                
                // Get reviewer user ID
                var reviewerUserSql = "SELECT USER_ID FROM \"SSDBONLINE\".\"USERS\" WHERE \"USERNAME\" = 'reviewer'";
                var reviewerUserId = await GetSingleValueAsync(reviewerUserSql);
                
                // Get manager user ID
                var managerUserSql = "SELECT USER_ID FROM \"SSDBONLINE\".\"USERS\" WHERE \"USERNAME\" = 'manager'";
                var managerUserId = await GetSingleValueAsync(managerUserSql);
                
                if (adminRoleId == null || reviewerRoleId == null || managerRoleId == null)
                {
                    Console.WriteLine("Roles not found, cannot assign user roles");
                    return;
                }
                
                // Assign admin role to admin user
                var checkAdminUserRoleSql = $"SELECT COUNT(*) FROM \"SSDBONLINE\".\"USER_ROLES\" WHERE \"USER_ID\" = {adminUserId} AND \"ROLE_ID\" = {adminRoleId}";
                var adminUserRoleCount = await GetSingleValueAsync(checkAdminUserRoleSql);
                
                if (adminUserRoleCount == 0)
                {
                    var insertAdminUserRoleSql = $"INSERT INTO \"SSDBONLINE\".\"USER_ROLES\" (USER_ID, ROLE_ID, ASSIGNED_AT, ASSIGNED_BY) VALUES ({adminUserId}, {adminRoleId}, SYSDATE, {adminUserId})";
                    await _context.Database.ExecuteSqlRawAsync(insertAdminUserRoleSql);
                    Console.WriteLine("Assigned Admin role to admin user");
                }
                
                // Assign reviewer role to reviewer user
                if (reviewerUserId != null)
                {
                    var checkReviewerUserRoleSql = $"SELECT COUNT(*) FROM \"SSDBONLINE\".\"USER_ROLES\" WHERE \"USER_ID\" = {reviewerUserId} AND \"ROLE_ID\" = {reviewerRoleId}";
                    var reviewerUserRoleCount = await GetSingleValueAsync(checkReviewerUserRoleSql);
                    
                    if (reviewerUserRoleCount == 0)
                    {
                        var insertReviewerUserRoleSql = $"INSERT INTO \"SSDBONLINE\".\"USER_ROLES\" (USER_ID, ROLE_ID, ASSIGNED_AT, ASSIGNED_BY) VALUES ({reviewerUserId}, {reviewerRoleId}, SYSDATE, {adminUserId})";
                        await _context.Database.ExecuteSqlRawAsync(insertReviewerUserRoleSql);
                        Console.WriteLine("Assigned Reviewer role to reviewer user");
                    }
                }
                else
                {
                    Console.WriteLine("Reviewer user not found, cannot assign role");
                }
                
                // Assign manager role to manager user
                if (managerUserId != null)
                {
                    var checkManagerUserRoleSql = $"SELECT COUNT(*) FROM \"SSDBONLINE\".\"USER_ROLES\" WHERE \"USER_ID\" = {managerUserId} AND \"ROLE_ID\" = {managerRoleId}";
                    var managerUserRoleCount = await GetSingleValueAsync(checkManagerUserRoleSql);
                    
                    if (managerUserRoleCount == 0)
                    {
                        var insertManagerUserRoleSql = $"INSERT INTO \"SSDBONLINE\".\"USER_ROLES\" (USER_ID, ROLE_ID, ASSIGNED_AT, ASSIGNED_BY) VALUES ({managerUserId}, {managerRoleId}, SYSDATE, {adminUserId})";
                        await _context.Database.ExecuteSqlRawAsync(insertManagerUserRoleSql);
                        Console.WriteLine("Assigned Manager role to manager user");
                    }
                }
                else
                {
                    Console.WriteLine("Manager user not found, cannot assign role");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding user roles: {ex.Message}");
            }
        }

        private async Task AssignPermissionsToRolesAsync()
        {
            try
            {
                // Get permissions from Roles.RolePermissions mapping
                var rolePermissions = Roles.RolePermissions;
                
                foreach (var rolePermission in rolePermissions)
                {
                    var roleName = rolePermission.Key;
                    var permissions = rolePermission.Value;

                    await AssignPermissionsToRole(roleName, permissions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error assigning permissions to roles: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private async Task AssignPermissionsToRole(string roleName, List<string> permissionNames)
        {
            try
            {
                // Get role ID
                var roleSql = $"SELECT ROLE_ID FROM SSDBONLINE.ROLES WHERE NAME = '{roleName.Replace("'", "''")}'";
                var roleId = await GetSingleValueAsync(roleSql);
                
                if (roleId == null)
                {
                    Console.WriteLine($"Role not found: {roleName}");
                    return;
                }
                
                foreach (var permissionName in permissionNames)
                {
                    try
                    {
                        // Get permission ID
                        var permissionSql = $"SELECT PERMISSION_ID FROM SSDBONLINE.PERMISSIONS WHERE NAME = '{permissionName.Replace("'", "''")}'";
                        var permissionId = await GetSingleValueAsync(permissionSql);
                        
                        if (permissionId == null)
                        {
                            Console.WriteLine($"Permission not found: {permissionName}");
                            continue;
                        }
                        
                        // Check if role permission already exists
                        var checkSql = $"SELECT COUNT(*) FROM SSDBONLINE.ROLE_PERMISSIONS WHERE ROLE_ID = {roleId} AND PERMISSION_ID = {permissionId}";
                        var count = await GetSingleValueAsync(checkSql);
                        
                        if (count == 0)
                        {
                            var insertSql = $"INSERT INTO SSDBONLINE.ROLE_PERMISSIONS (ROLE_ID, PERMISSION_ID, GRANTED_AT) VALUES ({roleId}, {permissionId}, SYSDATE)";
                            await _context.Database.ExecuteSqlRawAsync(insertSql);
                            Console.WriteLine($"Assigned permission {permissionName} to role {roleName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error assigning permission {permissionName} to role {roleName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignPermissionsToRole for {roleName}: {ex.Message}");
            }
        }

        private async Task<int?> GetSingleValueAsync(string sql)
        {
            try
            {
                // Use EF Core's raw SQL query to avoid connection issues
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                
                var result = await command.ExecuteScalarAsync();
                await connection.CloseAsync();
                return result != null ? Convert.ToInt32(result) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing SQL: {sql}. Error: {ex.Message}");
                return null;
            }
        }
    }
}
