using Microsoft.Extensions.Localization;

namespace MangaShelf.Common.Localization.Interfaces;

public interface ILocalizationService<T>
    where T : class
{
    /// <summary>
    /// Gets the localized string for the specified name.
    /// </summary>
    /// <param name="name">The name of the string to retrieve.</param>
    /// <returns>The localized string.</returns>
    LocalizedString this[string name] { get; }
    /// <summary>
    /// Gets the localized string for the specified name with arguments.
    /// </summary>
    /// <param name="name">The name of the string to retrieve.</param>
    /// <param name="arguments">The arguments to format the string with.</param>
    /// <returns>The localized string.</returns>
    LocalizedString this[string name, params object[] arguments] { get; }
    /// <summary>
    /// Gets all localized strings.
    /// </summary>
    /// <param name="includeParentCultures">Whether to include strings from parent cultures.</param>
    /// <returns>An enumerable of all localized strings.</returns>
    IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures);
}
