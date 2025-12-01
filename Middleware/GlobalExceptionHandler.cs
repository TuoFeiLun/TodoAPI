using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace MyApi.Middleware;

// RULE TEST OK
// Global exception handler for catching all unhandled exceptions
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, title, detail) = exception switch
        {
            // JSON parsing errors
            JsonException jsonEx => (
                StatusCodes.Status400BadRequest,
                "Invalid JSON format",
                GetFriendlyJsonError(jsonEx.Message)
            ),

            // Bad HTTP request errors (includes JSON deserialization)
            Microsoft.AspNetCore.Http.BadHttpRequestException badHttpEx => (
                StatusCodes.Status400BadRequest,
                "Bad request format",
                GetFriendlyJsonError(badHttpEx.Message)
            ),

            // Validation errors
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                "Validation error",
                argEx.Message
            ),

            // Not found errors
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                "The requested resource was not found"
            ),

            // Unauthorized
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "You are not authorized to access this resource"
            ),

            // Default case
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred. Please try again later."
            )
        };

        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            detail,
            instance = httpContext.Request.Path,
            timestamp = DateTime.UtcNow
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Exception handled
    }

    // Convert technical JSON errors to user-friendly messages
    private static string GetFriendlyJsonError(string technicalMessage)
    {
        if (technicalMessage.Contains("trailing comma", StringComparison.OrdinalIgnoreCase))
        {
            return "JSON contains a trailing comma. Please remove commas after the last property in objects or arrays.";
        }

        if (technicalMessage.Contains("missing required property", StringComparison.OrdinalIgnoreCase))
        {
            return "One or more required fields are missing from the request.";
        }

        if (technicalMessage.Contains("unexpected character", StringComparison.OrdinalIgnoreCase))
        {
            return "JSON format is invalid. Please check for syntax errors like missing quotes or brackets.";
        }

        if (technicalMessage.Contains("expected", StringComparison.OrdinalIgnoreCase))
        {
            return "JSON structure is incorrect. Please verify the format matches the expected schema.";
        }

        // Enum conversion errors (e.g., invalid role value)
        if (technicalMessage.Contains("could not be converted to") && technicalMessage.Contains("UserRole"))
        {
            return "Invalid role value. Allowed values are: Admin, Editor, Viewer (case-insensitive).";
        }

        // Generic enum conversion error
        if (technicalMessage.Contains("could not be converted to"))
        {
            return "Invalid value for an enum field. Please check the allowed values.";
        }

        // Default friendly message
        return "The request body contains invalid JSON. Please check the format and try again.";
    }
}

