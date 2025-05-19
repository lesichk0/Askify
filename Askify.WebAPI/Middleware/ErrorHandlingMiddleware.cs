using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Askify.WebAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        // Fix the null reference warning in the error response
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            // Cast to avoid nullable warning
            string? detail = exception.InnerException?.Message;
            
            object response;
            
            switch (exception)
            {
                case KeyNotFoundException:
                case UnauthorizedAccessException:
                case ArgumentException:
                    context.Response.StatusCode = exception switch
                    {
                        KeyNotFoundException => (int)HttpStatusCode.NotFound,
                        UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                        _ => (int)HttpStatusCode.BadRequest
                    };
                    
                    response = new 
                    {
                        error = new 
                        {
                            message = exception.Message,
                            detail
                        }
                    };
                    break;
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response = new 
                    {
                        error = new 
                        {
                            message = "An unexpected error occurred",
                            detail = "Please try again later or contact support if the problem persists"
                        }
                    };
                    break;
            }

            var jsonResult = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResult);
        }
    }
}
