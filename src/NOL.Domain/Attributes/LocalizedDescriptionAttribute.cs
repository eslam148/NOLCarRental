namespace NOL.Domain.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class LocalizedDescriptionAttribute : Attribute
{
    public string EnglishName { get; set; }
    public string ArabicName { get; set; }

    public LocalizedDescriptionAttribute(string englishName, string arabicName)
    {
        EnglishName = englishName;
        ArabicName = arabicName;
    }

    public string GetLocalizedName(string culture)
    {
        return culture?.ToLower() == "ar" ? ArabicName : EnglishName;
    }
}

