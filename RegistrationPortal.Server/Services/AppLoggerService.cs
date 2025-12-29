using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RegistrationPortal.Server.Services
{
    public interface IAppLoggerService
    {
        Task LogErrorAsync(Exception exception, string message, params object[] args);
        Task LogWarningAsync(string message, params object[] args);
        Task LogInformationAsync(string message, params object[] args);
        Task LogDebugAsync(string message, params object[] args);
        Task LogApiCallAsync(string method, string path, string userId, int statusCode, long duration);
        Task LogDatabaseErrorAsync(Exception exception, string query, params object[] parameters);
    }

    public class AppLoggerService : IAppLoggerService
    {
        private readonly ILogger<AppLoggerService> _logger;

        public AppLoggerService(ILogger<AppLoggerService> logger)
        {
            _logger = logger;
        }

        public Task LogErrorAsync(Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args);
            return Task.CompletedTask;
        }

        public Task LogWarningAsync(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
            return Task.CompletedTask;
        }

        public Task LogInformationAsync(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
            return Task.CompletedTask;
        }

        public Task LogDebugAsync(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
            return Task.CompletedTask;
        }

        public Task LogApiCallAsync(string method, string path, string userId, int statusCode, long duration)
        {
            _logger.LogInformation(
                "API Call: {Method} {Path} | User: {UserId} | Status: {StatusCode} | Duration: {Duration}ms",
                method, path, userId, statusCode, duration);
            return Task.CompletedTask;
        }

        public Task LogDatabaseErrorAsync(Exception exception, string query, params object[] parameters)
        {
            _logger.LogError(exception, 
                "Database Error. Query: {Query} | Parameters: {Parameters}",
                query, string.Join(", ", parameters));
            return Task.CompletedTask;
        }
    }
}
