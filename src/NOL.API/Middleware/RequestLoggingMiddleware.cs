using System.Diagnostics;
using System.Text;

namespace NOL.API.Middleware;

/// <summary>
/// Middleware to log all HTTP requests with their equivalent cURL commands
/// Logs to Seq with full request details including headers, body, and response
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for static files and health checks
        if (context.Request.Path.StartsWithSegments("/uploads") ||
            context.Request.Path.StartsWithSegments("/css") ||
            context.Request.Path.StartsWithSegments("/js") ||
            context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        
        // Read and log request
        var requestDetails = await CaptureRequestAsync(context);
        var curlCommand = GenerateCurlCommand(context, requestDetails.Body);
        
        // Log the incoming request with cURL
        _logger.LogInformation(
            "HTTP Request: {Method} {Path} | User: {User} | IP: {IP} | TraceId: {TraceId} | CurlCommand: {CurlCommand}",
            context.Request.Method,
            context.Request.Path + context.Request.QueryString,
            context.User?.Identity?.Name ?? "Anonymous",
            context.Connection.RemoteIpAddress?.ToString(),
            context.TraceIdentifier,
            curlCommand);

        // Log request body if present
        if (!string.IsNullOrEmpty(requestDetails.Body))
        {
            _logger.LogInformation(
                "Request Body for {Method} {Path} | Body: {RequestBody}",
                context.Request.Method,
                context.Request.Path,
                requestDetails.Body);
        }

        // Capture response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Log response
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            
            _logger.Log(logLevel,
                "HTTP Response: {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | User: {User} | TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path + context.Request.QueryString,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                context.User?.Identity?.Name ?? "Anonymous",
                context.TraceIdentifier);

            // Log response body for errors or if it's small
            if (context.Response.StatusCode >= 400 || responseBodyText.Length < 5000)
            {
                _logger.Log(logLevel,
                    "Response Body for {Method} {Path} | Status: {StatusCode} | Body: {ResponseBody}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    responseBodyText);
            }

            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Exception during request: {Method} {Path} | Duration: {Duration}ms | TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path + context.Request.QueryString,
                stopwatch.ElapsedMilliseconds,
                context.TraceIdentifier);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task<(string Body, Dictionary<string, string> Headers)> CaptureRequestAsync(HttpContext context)
    {
        var body = string.Empty;
        var headers = new Dictionary<string, string>();

        // Capture headers
        foreach (var header in context.Request.Headers)
        {
            // Skip sensitive headers from logging
            if (!header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) &&
                !header.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
            {
                headers[header.Key] = header.Value.ToString();
            }
            else
            {
                headers[header.Key] = "***REDACTED***";
            }
        }

        // Capture body for POST, PUT, PATCH
        if (context.Request.Method == "POST" || 
            context.Request.Method == "PUT" || 
            context.Request.Method == "PATCH")
        {
            context.Request.EnableBuffering();
            
            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);
            
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        return (body, headers);
    }

    private string GenerateCurlCommand(HttpContext context, string? body)
    {
        var request = context.Request;
        var curlBuilder = new StringBuilder();
        
        // Base curl command
        curlBuilder.Append($"curl -X {request.Method}");
        
        // Add URL
        var scheme = request.Scheme;
        var host = request.Host.Value;
        var path = request.Path.Value;
        var queryString = request.QueryString.Value;
        var fullUrl = $"{scheme}://{host}{path}{queryString}";
        curlBuilder.Append($" '{fullUrl}'");
        
        // Add headers
        foreach (var header in request.Headers)
        {
            // Skip headers that are automatically added by curl or not needed
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Redact sensitive headers but keep in curl for reproduction
            var headerValue = header.Value.ToString();
            if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                // Keep first few characters for identification
                if (headerValue.Length > 20)
                {
                    headerValue = headerValue.Substring(0, 20) + "...***REDACTED***";
                }
            }
            else if (header.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
            {
                headerValue = "***REDACTED***";
            }

            curlBuilder.Append($" -H '{header.Key}: {headerValue}'");
        }
        
        // Add body if present
        if (!string.IsNullOrEmpty(body))
        {
            // Escape single quotes in body
            var escapedBody = body.Replace("'", "'\\''");
            curlBuilder.Append($" -d '{escapedBody}'");
        }
        
        return curlBuilder.ToString();
    }
}

/// <summary>
/// Extension method to register the request logging middleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}

