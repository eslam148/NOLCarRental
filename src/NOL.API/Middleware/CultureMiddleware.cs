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
        var culture = "en"; // default culture

        // Check query parameter
        if (context.Request.Query.ContainsKey("culture"))
        {
            var requestedCulture = context.Request.Query["culture"].ToString();
            if (!string.IsNullOrEmpty(requestedCulture) && (requestedCulture == "ar" || requestedCulture == "en"))
            {
                culture = requestedCulture;
            }
        }
        // Check Accept-Language header
        else if (context.Request.Headers.ContainsKey("Accept-Language"))
        {
            var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
            if (acceptLanguage.Contains("ar"))
            {
                culture = "ar";
            }
        }

        // Set the culture
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        // Continue to next middleware
        await _next(context);
    }
} 