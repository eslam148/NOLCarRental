using Microsoft.Extensions.Localization;
using NOL.Application.Common.Interfaces;
using NOL.API.Resources;
using System.Globalization;

namespace NOL.API.Services;

public class LocalizationService : ILocalizationService
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LocalizationService(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public string GetLocalizedString(string key)
    {
        try
        {
            var localizedValue = _localizer[key];
            
            // Check if localization was found
            if (localizedValue.ResourceNotFound)
            {
                // Return fallback value or key
                return GetFallbackValue(key);
            }
            
            return localizedValue.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Localization error for key '{key}': {ex.Message}");
            return GetFallbackValue(key);
        }
    }

    public string GetLocalizedString(string key, params object[] args)
    {
        try
        {
            var localizedValue = _localizer[key, args];
            
            if (localizedValue.ResourceNotFound)
            {
                return string.Format(GetFallbackValue(key), args);
            }
            
            return localizedValue.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Localization error for key '{key}': {ex.Message}");
            return string.Format(GetFallbackValue(key), args);
        }
    }

    public void SetCulture(string culture)
    {
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
    }

    public string GetCurrentCulture()
    {
        return CultureInfo.CurrentUICulture.Name;
    }

    private string GetFallbackValue(string key)
    {
        // Provide fallback values for common keys
        return key switch
        {
            "BranchesRetrieved" => CultureInfo.CurrentUICulture.Name == "ar" ? "تم استرداد الفروع بنجاح" : "Branches retrieved successfully",
            "BranchRetrieved" => CultureInfo.CurrentUICulture.Name == "ar" ? "تم استرداد الفرع بنجاح" : "Branch retrieved successfully",
            "OperationSuccessful" => CultureInfo.CurrentUICulture.Name == "ar" ? "تمت العملية بنجاح" : "Operation completed successfully",
            "ResourceNotFound" => CultureInfo.CurrentUICulture.Name == "ar" ? "المورد غير موجود" : "Resource not found",
            "InternalServerError" => CultureInfo.CurrentUICulture.Name == "ar" ? "خطأ داخلي في الخادم" : "Internal server error",
            _ => key
        };
    }
} 