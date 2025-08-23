using MangaShelf.DAL.MangaShelf;
using MangaShelf.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading.Tasks;

namespace MangaShelf.SeedService;

public class SeedProdShelfService : ISeedDataService
{
    public SeedProdShelfService(ILogger<SeedProdShelfService> logger)
    {
        _logger = logger;
    }

    public string ActivitySourceName => "Seed prod shelf";

    public int Priority => 2;

    private async Task SeedCountries(MangaDbContext context)
    {
        var existing = await context.Countries.ToListAsync();

        if (!existing.Any())
        {
            var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.Name)).ToList();

            foreach (var region in regions.DistinctBy(x => x.TwoLetterISORegionName))
            {
                if (region.TwoLetterISORegionName.Length != 2)
                {
                    continue; // Skip regions with invalid country codes
                }

                context.Countries.Add(new Country()
                {
                    CountryCode = region.TwoLetterISORegionName.ToLowerInvariant(),
                    Name = region.EnglishName,
                    FlagUrl = SaveFlagFromCDN(region.TwoLetterISORegionName.ToLowerInvariant())
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
                    country.FlagUrl = SaveFlagFromCDN(country.CountryCode);
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private const string serverRoot = "wwwroot";
    private const string imageDir = "images";
    private readonly ILogger<SeedProdShelfService> _logger;

    public static string SaveFlagFromCDN(string countryCode)
    {
        var urls = new List<string> {
            $"https://flagcdn.com/40x30/{countryCode}.webp" };

        var destiantionFolder = Path.Combine(imageDir, "countries");
        var localDirectory = Path.Combine(serverRoot, destiantionFolder);


        foreach (var url in urls)
        {
            var extention = Path.GetExtension(url);
            var filename = $"{countryCode}{extention}";

            using (var client = new HttpClient())
            {
                using (var response = client.GetAsync(url))
                {
                    byte[] imageBytes =
                        response.Result.Content.ReadAsByteArrayAsync().Result;

                    var localPath = Path.Combine(localDirectory, filename);

                    if (!Directory.Exists(localDirectory))
                        Directory.CreateDirectory(localDirectory);

                    File.WriteAllBytes(localPath, imageBytes);
                }
            }
        }

        return Path.Combine(destiantionFolder, $"{countryCode}.webp");
    }

    public string IsoCountryCodeToFlagEmoji(string countryCode) => string.Concat(countryCode.ToUpper().Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));

    private async Task SeedPublishers(MangaDbContext context)
    {
        var ukraine = context.Countries.SingleOrDefault(x => x.CountryCode == "ua");
        var us = context.Countries.SingleOrDefault(x => x.CountryCode == "us");

        var publishers = new Publisher[]
        {
                new() { Name = "Artbooks", Country = ukraine, Url = "https://artbooks.com.ua/" },
                new() { Name = "Bimba", Country = ukraine, Url = "https://bimba-publisher.com/" },
                new() { Name = "BookChef", Country = ukraine, Url = "https://bookchef.com.ua/" },
                new() { Name = "Fireclaw", Country = ukraine, Url = "https://fireclaw.com.ua/" },
                new() { Name = "Lantsuta", Country = ukraine, Url = "https://lantsuta-publishing.com/" },
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
                new() { Name = "КСД", Country = ukraine },
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
        var existingNames = await context.Publishers.Select(x => x.Name).ToListAsync();
        var notExisting = publishers.Where(x => !existingNames.Contains(x.Name)).ToList();

        foreach (var publisher in notExisting)
        {
            publisher.Id = Guid.NewGuid();
            context.Publishers.Add(publisher);
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

    public async Task Run(IServiceProvider scopedServiceProvider, CancellationToken cancellationToken)
    {
        var context = scopedServiceProvider.GetRequiredService<MangaDbContext>();
        await SeedCountries(context);
        await SeedPublishers(context);
    }
}
