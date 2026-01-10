using MangaShelf.BL.Dto;
using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Mappers;
using MangaShelf.DAL;
using MangaShelf.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services;

public class CountryService : ICountryService
{
    private readonly ILogger<CountryService> _logger;
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;

    public CountryService(ILogger<CountryService> logger, IDbContextFactory<MangaDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CountryDto?> GetCountryByCodeAsync(string countryCode, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var serviceFactory = new DomainServiceFactory(context);
        var countryDomainService = serviceFactory.GetDomainService<ICountryDomainService>();

        var country = await countryDomainService.GetByCountryCodeAsync(countryCode, token);

        return country?.ToDto();
    }

    public async Task<string?> GetCountryNameByCodeAsync(string countryCode, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var serviceFactory = new DomainServiceFactory(context);
        var countryDomainService = serviceFactory.GetDomainService<ICountryDomainService>();

        var country = await countryDomainService.GetByCountryCodeAsync(countryCode, token);

        return country?.Name;
    }
}
