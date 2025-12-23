using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace RegistrationPortal.Server.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = context.Response;

            var errorResponse = new
            {
                error = "An error occurred while processing your request",
                message = exception.Message,
                statusCode = (int)HttpStatusCode.InternalServerError
            };

            // Handle specific exception types
            switch (exception)
            {
                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new
                    {
                        error = "Invalid operation",
                        message = exception.Message,
                        statusCode = (int)HttpStatusCode.BadRequest
                    };
                    break;
                case ArgumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new
                    {
                        error = "Invalid argument",
                        message = exception.Message,
                        statusCode = (int)HttpStatusCode.BadRequest
                    };
                    break;
                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = new
                    {
                        error = "Resource not found",
                        message = exception.Message,
                        statusCode = (int)HttpStatusCode.NotFound
                    };
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            return context.Response.WriteAsync(jsonResponse);
        }
    }

    // Extension method for easy registration
    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}
