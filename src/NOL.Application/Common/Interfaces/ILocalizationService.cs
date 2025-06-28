namespace NOL.Application.Common.Interfaces;

public interface ILocalizationService
{
    string GetLocalizedString(string key);
    string GetLocalizedString(string key, params object[] args);
    void SetCulture(string culture);
    string GetCurrentCulture();
} 