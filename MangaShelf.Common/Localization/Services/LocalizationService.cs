using MangaShelf.Common.Localization.Interfaces;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace MangaShelf.Common.Localization.Services;

public static class LocalizationService
{
    private static readonly CultureInfo[] supportedCultures =
    [
        new CultureInfo("uk-UA"),
        new CultureInfo("en-US"),
     ];
    public static CultureInfo[] SupportedCultures { get => supportedCultures; }
}

public abstract class LocalizationService<T>(IStringLocalizer<T> localizer, ILogger<LocalizationService<T>> logger) : ILocalizationService<T> where T : class
{


    public LocalizedString this[string name]
    {
        get
        {
            return CheckResult(localizer.GetString(name));
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            try
            {
                return CheckResult(localizer.GetString(name, arguments));
            }
            catch
            {
                return CheckResult(localizer.GetString(name));
            }
        }
    }

    public LocalizedString this[Enum name] => this[name.ToString()];

    public LocalizedString this[Enum name, params object[] arguments] => this[name.ToString(), arguments];

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return localizer.GetAllStrings(includeParentCultures);
    }

    private LocalizedString CheckResult(LocalizedString result)
    {
        if (result.ResourceNotFound || string.IsNullOrEmpty(result.Value))
        {
            result = new LocalizedString(result.Name, "{" + result.Name + "}", result.ResourceNotFound, result.SearchedLocation);
            logger.LogWarning("Translation for '{name}' {state}. Fallback value {value}", result.Name, result.ResourceNotFound ? "not found" : "is empty", result.Value);
        }

        return result;
    }
}
