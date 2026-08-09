using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;
using MangaShelf.BL.Mappers;
using Microsoft.EntityFrameworkCore;
using MangaShelf.DAL;
using MangaShelf.BL.Contracts;
using System.Globalization;

namespace MangaShelf.BL.Services;

public class PublisherService : IPublisherService
{
    private readonly ILogger<Publisher> _logger;
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;

    public PublisherService(ILogger<Publisher> logger, IDbContextFactory<MangaDbContext> dbContextFactory) 
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<string>> GetAllNamesAsync(CancellationToken stoppingToken)
    {
        using var context = _dbContextFactory.CreateDbContext();
        _logger.LogInformation("Getting all publiser names.{0}", context.ContextId);

        var serviceFactory = new DomainServiceFactory(context);
        var publisherDomainService = serviceFactory.GetDomainService<IPublisherDomainService>();

        var publishers = await publisherDomainService.GetAllNamesAsync(stoppingToken);

        return publishers;
    }

    public async Task<PublisherSimpleDto?> GetByNameAsync(string publisherName, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var serviceFactory = new DomainServiceFactory(context);
        var publisherDomainService = serviceFactory.GetDomainService<IPublisherDomainService>();

        var publisher = await publisherDomainService.GetByNameAsync(publisherName, token);

        return publisher?.ToDto();
    }

    public async Task<IReadOnlyCollection<AdminPublisherDto>> GetAllForAdminAsync(CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var publishers = await context.Publishers
            .IgnoreQueryFilters()
            .Include(x => x.Country)
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(token);

        return publishers.Select(ToAdminDto).ToList();
    }

    public async Task<IReadOnlyCollection<PublisherCountryOptionDto>> GetCountryOptionsAsync(CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var countries = await context.Countries
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new PublisherCountryOptionDto
            {
                Id = x.Id,
                Name = x.Name,
                CountryCode = x.CountryCode.ToUpper(CultureInfo.InvariantCulture)
            })
            .ToListAsync(token);

        return countries;
    }

    public async Task<AdminPublisherDto> CreateAsync(PublisherUpsertDto dto, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var name = NormalizeName(dto.Name);
        var normalizedUrl = NormalizeUrl(dto.Url);

        await EnsureUniqueNameAsync(context, name, null, token);
        await EnsureCountryExistsAsync(context, dto.CountryId, token);

        var publisher = new Publisher
        {
            Name = name,
            Url = normalizedUrl,
            CountryId = dto.CountryId
        };

        context.Publishers.Add(publisher);
        await context.SaveChangesAsync(token);
        await context.Entry(publisher).Reference(x => x.Country).LoadAsync(token);

        _logger.LogInformation("Created publisher {PublisherId}", publisher.Id);
        return ToAdminDto(publisher);
    }

    public async Task UpdateAsync(Guid publisherId, PublisherUpsertDto dto, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var publisher = await context.Publishers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == publisherId, token)
            ?? throw new InvalidOperationException("Publisher not found.");

        if (publisher.IsDeleted)
        {
            throw new InvalidOperationException("Cannot edit deleted publisher. Restore it first.");
        }

        var name = NormalizeName(dto.Name);
        var normalizedUrl = NormalizeUrl(dto.Url);

        await EnsureUniqueNameAsync(context, name, publisherId, token);
        await EnsureCountryExistsAsync(context, dto.CountryId, token);

        publisher.Name = name;
        publisher.Url = normalizedUrl;
        publisher.CountryId = dto.CountryId;

        await context.SaveChangesAsync(token);
        _logger.LogInformation("Updated publisher {PublisherId}", publisher.Id);
    }

    public async Task DeleteAsync(Guid publisherId, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var publisher = await context.Publishers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == publisherId, token)
            ?? throw new InvalidOperationException("Publisher not found.");

        if (publisher.IsDeleted)
        {
            return;
        }

        publisher.IsDeleted = true;
        publisher.DeletedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(token);
        _logger.LogInformation("Deleted publisher {PublisherId}", publisher.Id);
    }

    public async Task RestoreAsync(Guid publisherId, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var publisher = await context.Publishers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == publisherId, token)
            ?? throw new InvalidOperationException("Publisher not found.");

        if (!publisher.IsDeleted)
        {
            return;
        }

        publisher.IsDeleted = false;
        publisher.DeletedAt = null;

        await context.SaveChangesAsync(token);
        _logger.LogInformation("Restored publisher {PublisherId}", publisher.Id);
    }

    private static string NormalizeName(string name)
    {
        var normalizedName = name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new InvalidOperationException("Publisher name is required.");
        }

        return normalizedName;
    }

    private static string? NormalizeUrl(string? url)
    {
        var normalizedUrl = string.IsNullOrWhiteSpace(url) ? null : url.Trim();
        return normalizedUrl;
    }

    private static async Task EnsureCountryExistsAsync(MangaDbContext context, Guid countryId, CancellationToken token)
    {
        if (countryId == Guid.Empty)
        {
            throw new InvalidOperationException("Country is required.");
        }

        var exists = await context.Countries.AnyAsync(x => x.Id == countryId, token);
        if (!exists)
        {
            throw new InvalidOperationException("Selected country was not found.");
        }
    }

    private static async Task EnsureUniqueNameAsync(MangaDbContext context, string name, Guid? excludedPublisherId, CancellationToken token)
    {
        var existingPublisher = await context.Publishers
            .IgnoreQueryFilters()
            .Where(x => excludedPublisherId == null || x.Id != excludedPublisherId.Value)
            .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), token);

        if (existingPublisher is null)
        {
            return;
        }

        if (existingPublisher.IsDeleted)
        {
            throw new InvalidOperationException("Publisher with this name already exists and is deleted. Restore it instead.");
        }

        throw new InvalidOperationException("Publisher with this name already exists.");
    }

    private static AdminPublisherDto ToAdminDto(Publisher publisher)
    {
        return new AdminPublisherDto
        {
            Id = publisher.Id,
            Name = publisher.Name,
            Url = publisher.Url,
            CountryId = publisher.CountryId,
            CountryName = publisher.Country?.Name,
            CountryCode = publisher.Country?.CountryCode,
            IsDeleted = publisher.IsDeleted
        };
    }
}