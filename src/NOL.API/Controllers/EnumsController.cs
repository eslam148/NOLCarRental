using Microsoft.AspNetCore.Mvc;
using NOL.Domain.Enums;
using NOL.Domain.Extensions;
using System.Globalization;

namespace NOL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnumsController : ControllerBase
{
    /// <summary>
    /// Get all available booking statuses with localized names
    /// </summary>
    [HttpGet("booking-statuses")]
    public IActionResult GetBookingStatuses()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var statuses = EnumExtensions.GetEnumList<BookingStatus>(culture);
        return Ok(statuses);
    }

    /// <summary>
    /// Get all available car statuses with localized names
    /// </summary>
    [HttpGet("car-statuses")]
    public IActionResult GetCarStatuses()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var statuses = EnumExtensions.GetEnumList<CarStatus>(culture);
        return Ok(statuses);
    }

    /// <summary>
    /// Get all fuel types with localized names
    /// </summary>
    [HttpGet("fuel-types")]
    public IActionResult GetFuelTypes()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var types = EnumExtensions.GetEnumList<FuelType>(culture);
        return Ok(types);
    }

    /// <summary>
    /// Get all transmission types with localized names
    /// </summary>
    [HttpGet("transmission-types")]
    public IActionResult GetTransmissionTypes()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var types = EnumExtensions.GetEnumList<TransmissionType>(culture);
        return Ok(types);
    }

    /// <summary>
    /// Get all payment methods with localized names
    /// </summary>
    [HttpGet("payment-methods")]
    public IActionResult GetPaymentMethods()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var methods = EnumExtensions.GetEnumList<PaymentMethod>(culture);
        return Ok(methods);
    }

    /// <summary>
    /// Get all payment statuses with localized names
    /// </summary>
    [HttpGet("payment-statuses")]
    public IActionResult GetPaymentStatuses()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var statuses = EnumExtensions.GetEnumList<PaymentStatus>(culture);
        return Ok(statuses);
    }

    /// <summary>
    /// Get all extra types with localized names
    /// </summary>
    [HttpGet("extra-types")]
    public IActionResult GetExtraTypes()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var types = EnumExtensions.GetEnumList<ExtraType>(culture);
        return Ok(types);
    }

    /// <summary>
    /// Get all advertisement statuses with localized names
    /// </summary>
    [HttpGet("advertisement-statuses")]
    public IActionResult GetAdvertisementStatuses()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var statuses = EnumExtensions.GetEnumList<AdvertisementStatus>(culture);
        return Ok(statuses);
    }

    /// <summary>
    /// Get all advertisement types with localized names
    /// </summary>
    [HttpGet("advertisement-types")]
    public IActionResult GetAdvertisementTypes()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var types = EnumExtensions.GetEnumList<AdvertisementType>(culture);
        return Ok(types);
    }

    /// <summary>
    /// Get all loyalty point transaction types with localized names
    /// </summary>
    [HttpGet("loyalty-point-transaction-types")]
    public IActionResult GetLoyaltyPointTransactionTypes()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var types = EnumExtensions.GetEnumList<LoyaltyPointTransactionType>(culture);
        return Ok(types);
    }

    /// <summary>
    /// Get all loyalty point earn reasons with localized names
    /// </summary>
    [HttpGet("loyalty-point-earn-reasons")]
    public IActionResult GetLoyaltyPointEarnReasons()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var reasons = EnumExtensions.GetEnumList<LoyaltyPointEarnReason>(culture);
        return Ok(reasons);
    }

    /// <summary>
    /// Get all notification types with localized names
    /// </summary>
    [HttpGet("notification-types")]
    public IActionResult GetNotificationTypes()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var types = EnumExtensions.GetEnumList<NotificationType>(culture);
        return Ok(types);
    }

    /// <summary>
    /// Get all user roles with localized names
    /// </summary>
    [HttpGet("user-roles")]
    public IActionResult GetUserRoles()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var roles = EnumExtensions.GetEnumList<UserRole>(culture);
        return Ok(roles);
    }

    /// <summary>
    /// Get all available languages with localized names
    /// </summary>
    [HttpGet("languages")]
    public IActionResult GetLanguages()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var languages = EnumExtensions.GetEnumList<Language>(culture);
        return Ok(languages);
    }

    /// <summary>
    /// Example: Get a specific enum value with its localized description
    /// </summary>
    [HttpGet("example")]
    public IActionResult GetExample()
    {
        // Example 1: Get localized description based on current culture
        var status = BookingStatus.Confirmed;
        var localizedStatus = status.GetLocalizedDescription();

        // Example 2: Get localized description for specific culture
        var statusInArabic = status.GetLocalizedDescription("ar");
        var statusInEnglish = status.GetLocalizedDescription("en");

        // Example 3: Get all values with descriptions
        var allStatuses = EnumExtensions.GetEnumList<BookingStatus>();

        return Ok(new
        {
            CurrentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
            LocalizedStatus = localizedStatus,
            ArabicStatus = statusInArabic,
            EnglishStatus = statusInEnglish,
            AllStatuses = allStatuses
        });
    }
}

