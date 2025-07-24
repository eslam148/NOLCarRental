using System.Globalization;

namespace NOL.API.Middleware;

public class CultureMiddleware
{
    private readonly RequestDelegate _next;

    public CultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // The RequestLocalization middleware should have already set the culture
        // This middleware can be used for additional logging or custom logic
        
        var currentCulture = CultureInfo.CurrentUICulture.Name;
        
        // Log current culture for debugging
        Console.WriteLine($"Current culture set to: {currentCulture}");
        
        // Add culture info to response headers for debugging
        context.Response.Headers.Add("X-Current-Culture", currentCulture);

        // Continue to next middleware
        await _next(context);
    }
} 