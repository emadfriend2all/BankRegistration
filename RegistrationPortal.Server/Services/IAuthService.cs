using RegistrationPortal.Server.DTOs.Auth;
using RegistrationPortal.Server.Entities.Identity;

namespace RegistrationPortal.Server.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<IEnumerable<string>> GetUserRolesAsync(int userId);
    Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
    Task<bool> IsUserActiveAsync(int userId);
}
