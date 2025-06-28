using System.Globalization;

namespace NOL.API.Middleware;

public class LanguageMiddleware
{
    private readonly RequestDelegate _next;

    public LanguageMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string culture = "ar"; // Default to Arabic

        // 1. Check query string first (for testing)
        if (context.Request.Query.ContainsKey("culture"))
        {
            culture = context.Request.Query["culture"].ToString();
        }
        // 2. Check user preference if authenticated
        else if (context.User.Identity?.IsAuthenticated == true)
        {
            var preferredLanguage = context.User.FindFirst("preferredLanguage")?.Value;
            if (!string.IsNullOrEmpty(preferredLanguage))
            {
                culture = preferredLanguage == "Arabic" ? "ar" : "en";
            }
        }
        // 3. Check Accept-Language header
        else
        {
            var acceptLanguage = context.Request.Headers["Accept-Language"].FirstOrDefault();
            if (acceptLanguage?.Contains("en") == true)
            {
                culture = "en";
            }
        }

        // Ensure valid culture
        if (culture != "ar" && culture != "en")
        {
            culture = "ar"; // Default fallback
        }

        // Set the culture for this request
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        // Also set it on the thread for the localization service
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;

        await _next(context);
    }
} 