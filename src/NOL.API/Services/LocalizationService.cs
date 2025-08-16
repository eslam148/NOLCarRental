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
            Console.WriteLine($"Requesting localization for key: '{key}', Culture: '{CultureInfo.CurrentUICulture.Name}'");
            var localizedValue = _localizer[key];
            
            // Check if localization was found
            if (localizedValue.ResourceNotFound)
            {
                Console.WriteLine($"Resource not found for key: '{key}', using fallback");
                // Return fallback value or key
                return GetFallbackValue(key);
            }
            
            Console.WriteLine($"Found localized value for key '{key}': '{localizedValue.Value}'");
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
        var isArabic = CultureInfo.CurrentUICulture.Name == "ar";
        
        // Provide fallback values for all validation keys
        return key switch
        {
            // Validation errors
            "PasswordsDoNotMatch" => isArabic ? "كلمة المرور وتأكيد كلمة المرور غير متطابقتان" : "Password and confirmation password do not match",
            "PasswordRequired" => isArabic ? "كلمة المرور وتأكيد كلمة المرور مطلوبان" : "Password and confirm password are required",
            "ValidationError" => isArabic ? "خطأ في التحقق من البيانات" : "Validation error",
            "EmailAlreadyExists" => isArabic ? "البريد الإلكتروني موجود بالفعل" : "Email already exists",
            "InvalidCredentials" => isArabic ? "بيانات اعتماد غير صحيحة" : "Invalid credentials",
            "InvalidEmailORPassword" => isArabic ? "البريد الإلكتروني أو كلمة المرور غير صالحة" : "Invalid email or password",
            "EmailNotVerified" => isArabic ? "البريد الإلكتروني غير مؤكد. يرجى التحقق من بريدك الإلكتروني للحصول على رمز التحقق" : "Email not verified. Please check your email for verification code",
            
            // Authentication messages
            "EmailAlreadyConfirmed" => isArabic ? "البريد الإلكتروني مؤكد بالفعل" : "Email is already confirmed",
            "EmailSendingFailed" => isArabic ? "فشل في إرسال البريد الإلكتروني" : "Failed to send email",
            "OtpExpired" => isArabic ? "انتهت صلاحية رمز التحقق" : "OTP code has expired",
            "InvalidOtp" => isArabic ? "رمز التحقق غير صحيح" : "Invalid OTP code",
            "UserNotFound" => isArabic ? "المستخدم غير موجود" : "User not found",
            
            // Common responses
            "OperationSuccessful" => isArabic ? "تمت العملية بنجاح" : "Operation completed successfully",
            "ResourceNotFound" => isArabic ? "المورد غير موجود" : "Resource not found",
            "InternalServerError" => isArabic ? "خطأ داخلي في الخادم" : "Internal server error",
            "BranchesRetrieved" => isArabic ? "تم استرداد الفروع بنجاح" : "Branches retrieved successfully",
            "BranchRetrieved" => isArabic ? "تم استرداد الفرع بنجاح" : "Branch retrieved successfully",
            "InvalidCoordinates" => isArabic ? "إحداثيات غير صالحة" : "Invalid coordinates",
            "NearbyBranchesRetrieved" => isArabic ? "تم استرداد الفروع القريبة بنجاح" : "Nearby branches retrieved successfully",

            _ => key // Return the key as last resort
        };
    }
} 