using System.Diagnostics;

namespace RegistrationPortal.Server.Middleware
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLoggingMiddleware> _logger;

        public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            var userId = context.User.Identity?.Name ?? "Anonymous";
            
            // Log request start
            _logger.LogInformation(
                "API Request Started: {Method} {Path} | User: {UserId} | IP: {RemoteIP}",
                request.Method,
                request.Path,
                userId,
                context.Connection.RemoteIpAddress?.ToString());

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var response = context.Response;
                
                // Log request completion
                var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
                _logger.Log(logLevel,
                    "API Request Completed: {Method} {Path} | User: {UserId} | Status: {StatusCode} | Duration: {Duration}ms",
                    request.Method,
                    request.Path,
                    userId,
                    response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                // Log slow requests (> 2 seconds)
                if (stopwatch.ElapsedMilliseconds > 2000)
                {
                    _logger.LogWarning(
                        "Slow API Request: {Method} {Path} | Duration: {Duration}ms",
                        request.Method,
                        request.Path,
                        stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }

    // Extension method for easy registration
    public static class ApiLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiLoggingMiddleware>();
        }
    }
}
