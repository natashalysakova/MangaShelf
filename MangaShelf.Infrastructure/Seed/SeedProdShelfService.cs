using MangaShelf.Common.Interfaces;
using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace MangaShelf.Infrastructure.Seed;

public class SeedProdShelfService : ISeedDataService
{
    private readonly ILogger<SeedProdShelfService> _logger;
    private readonly IImageManager _imageManager;
    private readonly ICountryDomainService _countryDomainService;
    private readonly IPublisherDomainService _publisherDomainService;
    private readonly MangaIdentityDbContext _identityContext;
    private readonly MangaDbContext _context;

    public SeedProdShelfService(ILogger<SeedProdShelfService> logger,
        IImageManager imageManager,
        ICountryDomainService countryDomainService,
        IPublisherDomainService publisherDomainService,
        MangaIdentityDbContext identityContext,
        MangaDbContext context
        )
    {
        _logger = logger;
        _imageManager = imageManager;
        _countryDomainService = countryDomainService;
        _publisherDomainService = publisherDomainService;
        _identityContext = identityContext;
        _context = context;
    }

    public string ActivitySourceName => "Seed prod shelf";

    public int Priority => 2;
    public async Task Run()
    {
        await Run(CancellationToken.None);
    }
    public async Task Run(CancellationToken cancellationToken)
    {
        await SeedCountries();
        await SeedPublishers();

        await SeedUsers();
    }

    private async Task SeedUsers()
    {
        var users = _identityContext.Users.Select(x => x.Id);
        var existingUserIds = await _context.Users.Select(x => x.IdentityUserId).ToListAsync();
        var notExisting = users.Where(x => !existingUserIds.Contains(x)).ToList();

        foreach (var id in notExisting)
        {
            _context.Users.Add(new User()
            {
                IdentityUserId = id
            });
        }

        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedCountries()
    {
        var existing = await _countryDomainService.GetAllCountriesAsync();

        if (!existing.Any())
        {
            var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.Name)).ToList();

            foreach (var region in regions.DistinctBy(x => x.TwoLetterISORegionName))
            {
                if (region.TwoLetterISORegionName.Length != 2)
                {
                    continue; // Skip regions with invalid country codes
                }

                await _countryDomainService.Add(new Country()
                {
                    CountryCode = region.TwoLetterISORegionName.ToLowerInvariant(),
                    Name = region.EnglishName,
                    FlagUrl = _imageManager.SaveFlagFromCDN(region.TwoLetterISORegionName.ToLowerInvariant())
                });
                _logger.LogInformation(region.Name);
            }
        }
        else
        {
            foreach (var country in existing)
            {
                if (string.IsNullOrEmpty(country.FlagUrl))
                {
                    await _countryDomainService.UpdateFlagUrl(country.Id, _imageManager.SaveFlagFromCDN(country.CountryCode));
                }
            }
        }
    }

    public string IsoCountryCodeToFlagEmoji(string countryCode) => string.Concat(countryCode.ToUpper().Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));

    private async Task SeedPublishers()
    {
        var ukraine = await _countryDomainService.GetByCountryCodeAsync("ua");
        var us = await _countryDomainService.GetByCountryCodeAsync("us");

        var publishers = new Publisher[]
        {
                new() { Name = "Artbooks", Country = ukraine, Url = "https://artbooks.com.ua/" },
                new() { Name = "Bimba", Country = ukraine, Url = "https://bimba-publisher.com/" },
                new() { Name = "BookChef", Country = ukraine, Url = "https://bookchef.com.ua/" },
                new() { Name = "Fireclaw", Country = ukraine, Url = "https://fireclaw.com.ua/" },
                new() { Name = "LANTSUTA", Country = ukraine, Url = "https://lantsuta-publishing.com/" },
                new() { Name = "Mal'opus", Country = ukraine, Url = "https://malopus.com.ua/" },
                new() { Name = "Manga Media", Country = ukraine, Url = "https://manga-media.com.ua/" },
                new() { Name = "Mimir Media", Country = ukraine, Url = "https://mimir.com.ua/" },
                new() { Name = "Molfar Comics", Country = ukraine, Url = "https://molfar-comics.com/" },
                new() { Name = "Nasha Idea", Country = ukraine, Url = "https://nashaidea.com/" },
                new() { Name = "Varvar Publishing", Country = ukraine, Url = "https://varvarpublishing.com/" },
                new() { Name = "Vivat", Country = ukraine, Url = "https://vivat.com.ua/" },
                new() { Name = "Vovkulaka", Country = ukraine, Url = "https://www.vovkulaka.net/" },
                new() { Name = "А-БА-БА-ГА-ЛА-МА-ГА", Country = ukraine },
                new() { Name = "Видавництво", Country = ukraine, Url = "https://vydavnytstvo.com/" },
                new() { Name = "КСД", Country = ukraine, Url = "https://ksd.ua/" },
                new() { Name = "Лабораторія", Country = ukraine, Url = "https://laboratory.ua/" },
                new() { Name = "Лол Кекс", Country = ukraine, Url = "https://ksd.ua/" },
                new() { Name = "РМ", Country = ukraine },
                new() { Name = "Ранок", Country = ukraine },
                new() { Name = "Сфаран", Country = ukraine, Url = "https://safranbook.com/" },
                new() { Name = "ТАК", Country = ukraine, Url = "https://vydavnytstvotak.com/" },
                new() { Name = "Фабула", Country = ukraine },
                new() { Name = "Archie Comics", Country = us },
                new() { Name = "Dark Horse", Country = us },
                new() { Name = "DC Comics", Country = us },
                new() { Name = "Kodansha Comics", Country = us, Url = "https://kodansha.us/" },
                new() { Name = "Marvel", Country = us },
                new() { Name = "Seven Seas", Country = us },
                new() { Name = "VIZ Media", Country = us, Url = "https://www.viz.com/" },
                new() { Name = "Vertical Comics", Country = us },
                new() { Name = "Yen Press", Country = us, Url = "https://yenpress.com/" }
        };

        var names = publishers.Select(x => x.Name);
        var existingNames = _publisherDomainService.GetAll().Select(x => x.Name).ToList();
        var notExisting = publishers.Where(x => !existingNames.Contains(x.Name)).ToList();

        await _publisherDomainService.AddRange(notExisting);
    }
}
