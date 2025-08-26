using MangaShelf.BL.Dto;

namespace MangaShelf.BL.Interfaces;

public interface ICountryService : IService
{
    /// <summary>
    /// Gets a list of all countries.
    /// </summary>
    /// <returns>A collection of <see cref="CountryDto"/> objects.</returns>
    Task<IEnumerable<CountryDto>> GetAllCountriesAsync();

    /// <summary>
    /// Gets the name of a country by its ISO 3166-1 alpha-2 code.
    /// </summary>
    /// <param name="countryCode">The ISO 3166-1 alpha-2 country code.</param>
    /// <returns>The name of the country, or null if not found.</returns>
    Task<string?> GetCountryNameByCodeAsync(string countryCode);

    /// <summary>
    /// Gets a country by its ISO 3166-1 alpha-2 code.
    /// </summary>
    /// <param name="countryCode">The ISO 3166-1 alpha-2 country code.</param>
    /// <returns>A <see cref="CountryDto"/> for the found country, or null if no country is found with the specified code.</returns>
    Task<CountryDto?> GetCountryByCodeAsync(string countryCode);
}