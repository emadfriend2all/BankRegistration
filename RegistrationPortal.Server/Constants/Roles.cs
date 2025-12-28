using RegistrationPortal.Server.Constants;

namespace RegistrationPortal.Server.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Reviewer = "Reviewer";

    // Get all roles as a list
    public static readonly List<string> All = new()
    {
        Admin, Manager, Reviewer
    };

    // Define role permissions mapping
    public static readonly Dictionary<string, List<string>> RolePermissions = new()
    {
        [Admin] = new List<string>(Permissions.All),

        [Manager] = new List<string>
        {
            // Customer permissions - can review customer
            Permissions.Customers.List,Permissions.Customers.ViewDetails,
            
            // Dashboard permissions - without anonymous buttons
            Permissions.Dashboard.View
        },

        [Reviewer] = new List<string>
        {
            // Review permissions + view all customers
            Permissions.Customers.Review, Permissions.Customers.Create, 
            Permissions.Customers.Update, Permissions.Customers.List, Permissions.Customers.ViewDetails,
            
            // Dashboard permissions - without anonymous buttons
            Permissions.Dashboard.View
        }
    };

    // Get permissions for a specific role
    public static List<string> GetPermissionsForRole(string roleName)
    {
        return RolePermissions.TryGetValue(roleName, out var permissions) ? permissions : new List<string>();
    }

    // Check if a role has a specific permission
    public static bool RoleHasPermission(string roleName, string permission)
    {
        var permissions = GetPermissionsForRole(roleName);
        return permissions.Contains(permission);
    }
}
