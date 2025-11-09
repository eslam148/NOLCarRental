using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Responses;
using NOL.Domain.Extensions;

namespace NOL.API.Middleware;

/// <summary>
/// Global exception handler middleware that catches all unhandled exceptions
/// and logs them to Seq while returning a consistent error response
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log the exception with full details to Seq
        _logger.LogError(exception,
            "Unhandled exception occurred. Request: {Method} {Path} | User: {User} | IP: {IP} | TraceId: {TraceId}",
            context.Request.Method,
            context.Request.Path,
            context.User?.Identity?.Name ?? "Anonymous",
            context.Connection.RemoteIpAddress?.ToString(),
            context.TraceIdentifier);

        // Determine status code and response code based on exception type
        var (statusCode, responseCode) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, ResponseCode.InvalidRequest),
            ArgumentException => (HttpStatusCode.BadRequest, ResponseCode.InvalidRequest),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ResponseCode.Unauthorized),
            KeyNotFoundException => (HttpStatusCode.NotFound, ResponseCode.ResourceNotFound),
            InvalidOperationException => (HttpStatusCode.Conflict, ResponseCode.OperationNotValid),
            NotImplementedException => (HttpStatusCode.NotImplemented, ResponseCode.FeatureNotImplemented),
            TimeoutException => (HttpStatusCode.RequestTimeout, ResponseCode.RequestTimeout),
            _ => (HttpStatusCode.InternalServerError, ResponseCode.InternalServerError)
        };
        
        // Get localized title from ResponseCode
        var title = responseCode.GetDescription();

        // Build error response
        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = _env.IsDevelopment() ? exception.Message : "An error occurred while processing your request.",
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)statusCode}"
        };

        // Add trace ID and response code for tracking
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        problemDetails.Extensions["responseCode"] = responseCode.ToString();
        problemDetails.Extensions["responseCodeValue"] = (int)responseCode;

        // In development, include stack trace
        if (_env.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            
            if (exception.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = exception.InnerException.Message;
            }
        }

        // Set response
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Extension method to register the global exception handler middleware
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}

