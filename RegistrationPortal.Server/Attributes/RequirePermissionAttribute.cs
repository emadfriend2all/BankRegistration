using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RegistrationPortal.Server.Constants;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace RegistrationPortal.Server.Attributes;

public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _permissions;

    public RequirePermissionAttribute(params string[] permissions)
    {
        _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Check if the user is authenticated
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get user permissions from claims
        var userPermissions = context.HttpContext.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToHashSet();

        // Check if user has any of the required permissions
        var hasPermission = _permissions.Any(permission => userPermissions.Contains(permission));

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    
    }
}

// Extension methods for easier usage
public static class PermissionAuthorizationExtensions
{
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        return services;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userPermissions = context.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToHashSet();

        if (requirement.Permissions.Any(permission => userPermissions.Contains(permission)))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string[] Permissions { get; }

    public PermissionRequirement(params string[] permissions)
    {
        Permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
    }
}

// Policy-based authorization attributes
public class RequireCustomerCreatePermissionAttribute : AuthorizeAttribute
{
    public RequireCustomerCreatePermissionAttribute()
    {
        Policy = Permissions.Customers.Create;
    }
}

public class RequireCustomerReadPermissionAttribute : AuthorizeAttribute
{
    public RequireCustomerReadPermissionAttribute()
    {
        Policy = Permissions.Customers.Read;
    }
}

public class RequireCustomerUpdatePermissionAttribute : AuthorizeAttribute
{
    public RequireCustomerUpdatePermissionAttribute()
    {
        Policy = Permissions.Customers.Update;
    }
}

public class RequireCustomerDeletePermissionAttribute : AuthorizeAttribute
{
    public RequireCustomerDeletePermissionAttribute()
    {
        Policy = Permissions.Customers.Delete;
    }
}

public class RequireAccountCreatePermissionAttribute : AuthorizeAttribute
{
    public RequireAccountCreatePermissionAttribute()
    {
        Policy = Permissions.Accounts.Create;
    }
}

public class RequireAccountReadPermissionAttribute : AuthorizeAttribute
{
    public RequireAccountReadPermissionAttribute()
    {
        Policy = Permissions.Accounts.Read;
    }
}

public class RequireAccountUpdatePermissionAttribute : AuthorizeAttribute
{
    public RequireAccountUpdatePermissionAttribute()
    {
        Policy = Permissions.Accounts.Update;
    }
}

public class RequireAccountDeletePermissionAttribute : AuthorizeAttribute
{
    public RequireAccountDeletePermissionAttribute()
    {
        Policy = Permissions.Accounts.Delete;
    }
}

public class RequireUserCreatePermissionAttribute : AuthorizeAttribute
{
    public RequireUserCreatePermissionAttribute()
    {
        Policy = Permissions.Users.Create;
    }
}

public class RequireUserReadPermissionAttribute : AuthorizeAttribute
{
    public RequireUserReadPermissionAttribute()
    {
        Policy = Permissions.Users.Read;
    }
}

public class RequireUserUpdatePermissionAttribute : AuthorizeAttribute
{
    public RequireUserUpdatePermissionAttribute()
    {
        Policy = Permissions.Users.Update;
    }
}

public class RequireUserDeletePermissionAttribute : AuthorizeAttribute
{
    public RequireUserDeletePermissionAttribute()
    {
        Policy = Permissions.Users.Delete;
    }
}

public class RequireRoleCreatePermissionAttribute : AuthorizeAttribute
{
    public RequireRoleCreatePermissionAttribute()
    {
        Policy = Permissions.Roles.Create;
    }
}

public class RequireRoleReadPermissionAttribute : AuthorizeAttribute
{
    public RequireRoleReadPermissionAttribute()
    {
        Policy = Permissions.Roles.Read;
    }
}

public class RequireRoleUpdatePermissionAttribute : AuthorizeAttribute
{
    public RequireRoleUpdatePermissionAttribute()
    {
        Policy = Permissions.Roles.Update;
    }
}

public class RequireRoleDeletePermissionAttribute : AuthorizeAttribute
{
    public RequireRoleDeletePermissionAttribute()
    {
        Policy = Permissions.Roles.Delete;
    }
}
