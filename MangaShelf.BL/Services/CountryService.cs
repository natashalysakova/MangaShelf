using MangaShelf.BL.Dto;
using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Profiles;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services;

public class CountryService : ICountryService
{
    private readonly ILogger<CountryService> _logger;
    private readonly ICountryRepository _countryRepository;

    public CountryService(ILogger<CountryService> logger, ICountryRepository countryRepository)
    {
        _logger = logger;
        _countryRepository = countryRepository;
    }
    public async Task<IEnumerable<CountryDto>> GetAllCountriesAsync()
    {
        var countries = await _countryRepository.GetAllCountriesAsync();

        return countries
            .Select(country => country.ToDto());
    }

    public async Task<CountryDto?> GetCountryByCodeAsync(string countryCode)
    {
        var country = await _countryRepository.GetByCountryCodeAsync(countryCode);
        if (country is null)
        {
            _logger.LogWarning("Country with code {CountryCode} not found", countryCode);
            return null;
        }

        return country.ToDto();
    }

    public async Task<string?> GetCountryNameByCodeAsync(string countryCode)
    {
        var country = await _countryRepository.GetByCountryCodeAsync(countryCode);
        if (country is null)
        {
            _logger.LogWarning("Country with code {CountryCode} not found", countryCode);
            return null;
        }

        return country.Name;
    }
}
