using System.Globalization;
using System.Reflection;
using NOL.Domain.Attributes;

namespace NOL.Domain.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// Gets the localized description text of an enum value based on the current culture
    /// Shorthand for GetLocalizedDescription()
    /// </summary>
    /// <example>
    /// var status = BookingStatus.Confirmed;
    /// var text = status.GetDescription(); // Returns "Confirmed" or "مؤكد"
    /// </example>
    public static string GetDescription(this Enum enumValue)
    {
        return GetLocalizedDescription(enumValue);
    }

    /// <summary>
    /// Gets the localized description text of an enum value for a specific culture
    /// </summary>
    /// <param name="enumValue">The enum value</param>
    /// <param name="culture">Culture code: "en" for English, "ar" for Arabic</param>
    /// <example>
    /// var status = BookingStatus.Confirmed;
    /// var arabicText = status.GetDescription("ar"); // Returns "مؤكد"
    /// var englishText = status.GetDescription("en"); // Returns "Confirmed"
    /// </example>
    public static string GetDescription(this Enum enumValue, string culture)
    {
        return GetLocalizedDescription(enumValue, culture);
    }

    /// <summary>
    /// Gets the localized description of an enum value based on the current culture
    /// </summary>
    public static string GetLocalizedDescription(this Enum enumValue)
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        return GetLocalizedDescription(enumValue, culture);
    }

    /// <summary>
    /// Gets the localized description of an enum value based on a specific culture
    /// </summary>
    public static string GetLocalizedDescription(this Enum enumValue, string culture)
    {
        var field = enumValue.GetType().GetField(enumValue.ToString());
        
        if (field == null)
            return enumValue.ToString();

        var attribute = field.GetCustomAttribute<LocalizedDescriptionAttribute>();
        
        if (attribute != null)
        {
            return attribute.GetLocalizedName(culture);
        }

        // Fallback to enum name if no attribute is found
        return enumValue.ToString();
    }

    /// <summary>
    /// Gets all enum values with their localized descriptions
    /// </summary>
    public static Dictionary<TEnum, string> GetAllLocalizedValues<TEnum>(string culture = null) where TEnum : Enum
    {
        culture ??= CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        
        var result = new Dictionary<TEnum, string>();
        
        foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
        {
            result[value] = value.GetLocalizedDescription(culture);
        }
        
        return result;
    }

    /// <summary>
    /// Gets all enum values as a list of key-value pairs for API responses
    /// </summary>
    public static List<EnumItem> GetEnumList<TEnum>(string culture = null) where TEnum : Enum
    {
        culture ??= CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        
        var result = new List<EnumItem>();
        
        foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
        {
            result.Add(new EnumItem
            {
                Value = Convert.ToInt32(value),
                Name = value.ToString(),
                LocalizedName = value.GetLocalizedDescription(culture)
            });
        }
        
        return result;
    }
}

public class EnumItem
{
    public int Value { get; set; }
    public string Name { get; set; }
    public string LocalizedName { get; set; }
}

