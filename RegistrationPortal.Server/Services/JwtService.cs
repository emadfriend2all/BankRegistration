using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RegistrationPortal.Server.Entities.Identity;

namespace RegistrationPortal.Server.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = _configuration["JwtSettings:Issuer"] ?? "RegistrationPortal";
        _audience = _configuration["JwtSettings:Audience"] ?? "RegistrationPortalUsers";
        _expirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 60);
    }

    public string GenerateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("FirstName", user.FirstName ?? ""),
            new Claim("LastName", user.LastName ?? ""),
            new Claim("Branch", user.Branch ?? "")
        };

        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permissions as claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string? GetUserIdFromToken(string token)
    {
        return GetClaimFromToken(token, ClaimTypes.NameIdentifier);
    }

    public string? GetUsernameFromToken(string token)
    {
        return GetClaimFromToken(token, ClaimTypes.Name);
    }

    public IEnumerable<string>? GetRolesFromToken(string token)
    {
        return GetClaimsFromToken(token, ClaimTypes.Role);
    }

    public IEnumerable<string>? GetPermissionsFromToken(string token)
    {
        return GetClaimsFromToken(token, "permission");
    }

    public string? GetBranchFromToken(string token)
    {
        return GetClaimFromToken(token, "Branch");
    }

    private string? GetClaimFromToken(string token, string claimType)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            return jsonToken?.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
        }
        catch
        {
            return null;
        }
    }

    private IEnumerable<string>? GetClaimsFromToken(string token, string claimType)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            return jsonToken?.Claims.Where(x => x.Type == claimType).Select(x => x.Value);
        }
        catch
        {
            return null;
        }
    }
}
