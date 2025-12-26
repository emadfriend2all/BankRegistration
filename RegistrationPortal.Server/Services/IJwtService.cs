using RegistrationPortal.Server.Entities.Identity;

namespace RegistrationPortal.Server.Services;

public interface IJwtService
{
    string GenerateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    bool ValidateToken(string token);
    string? GetUserIdFromToken(string token);
    string? GetUsernameFromToken(string token);
    IEnumerable<string>? GetRolesFromToken(string token);
    IEnumerable<string>? GetPermissionsFromToken(string token);
}
