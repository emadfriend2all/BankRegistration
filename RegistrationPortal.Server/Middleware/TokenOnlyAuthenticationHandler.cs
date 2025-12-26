using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace RegistrationPortal.Server.Middleware
{
    public class TokenOnlyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly JwtBearerOptions _jwtOptions;

        public TokenOnlyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IOptions<JwtBearerOptions> jwtOptions)
            : base(options, logger, encoder, clock)
        {
            _jwtOptions = jwtOptions.Value;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }

            var authorizationHeader = Request.Headers["Authorization"].ToString();
            
            // Remove "Bearer " prefix if present, otherwise use the token as-is
            var token = authorizationHeader.StartsWith("Bearer ") 
                ? authorizationHeader.Substring("Bearer ".Length).Trim()
                : authorizationHeader.Trim();

            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Empty Authorization Token"));
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = _jwtOptions.TokenValidationParameters;
                
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                return Task.FromResult(AuthenticateResult.Fail($"Invalid Token: {ex.Message}"));
            }
        }
    }
}
