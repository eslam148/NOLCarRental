using Microsoft.Extensions.Localization;
using NOL.Application.Common.Interfaces;
using System.Globalization;

namespace NOL.Infrastructure.Services;

public class LocalizationService : ILocalizationService
{
    private readonly IStringLocalizer _localizer;

    public LocalizationService(IStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    public string GetLocalizedString(string key)
    {
        return _localizer[key];
    }

    public string GetLocalizedString(string key, params object[] args)
    {
        return _localizer[key, args];
    }

    public void SetCulture(string culture)
    {
        var cultureInfo = new CultureInfo(culture);
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
    }

    public string GetCurrentCulture()
    {
        return Thread.CurrentThread.CurrentUICulture.Name;
    }
} 