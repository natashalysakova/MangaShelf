using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Services;
using MangaShelf.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MangaShelf.Common.Localization.Interfaces;
using MangaShelf.Common.Localization.Services;
using MangaShelf.BL.Enums;
using MangaShelf.BL.Parsers;
using MangaShelf.Infrastructure.Network;
using MangaShelf.DAL.DomainServices;
using MangaShelf.Common.Interfaces;


namespace MangaShelf.Infrastructure.Installer;

public static class ServicesInstallExtention
{
    public static IHostApplicationBuilder AddBusinessServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICountryService, CountryService>();
        builder.Services.AddScoped<ICountryDomainService, CountryDomainService>();

        builder.Services.AddScoped<IVolumeService, VolumeService>();
        builder.Services.AddScoped<IVolumeDomainService, VolumeDomainService>();

        builder.Services.AddScoped<ISeriesService, SeriesService>();
        builder.Services.AddScoped<ISeriesDomainService, SeriesDomainService>();

        builder.Services.AddScoped<IAuthorService, AuthorService>();
        builder.Services.AddScoped<IAuthorDomainService, AuthorDomainService>();

        builder.Services.AddScoped<IPublisherService, PublisherService>();
        builder.Services.AddScoped<IPublisherDomainService, PublisherDomainService>();

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IUserDomainService, UserDomainService>();

        builder.Services.AddScoped<IFailedSyncRecordsService, FailedSyncRecordsService>();
        builder.Services.AddScoped<IFailedSyncRecordsDomainService, FailedSyncRecordsDomainService>();



        builder.Services.AddKeyedScoped<IHtmlDownloader, BasicHtmlDownloader>(HtmlDownloaderKeys.Basic);
        builder.Services.AddKeyedScoped<IHtmlDownloader, AdvancedHtmlDownloader>(HtmlDownloaderKeys.Advanced);
        builder.Services.AddScoped<IParsedVolumeService, ParsedVolumeService>();
        builder.Services.AddScoped<IPublisherParser, MalopusParser>();
        builder.Services.AddScoped<IPublisherParser, NashaIdeaParser>();
        builder.Services.AddScoped<IPublisherParser, LantsutaParser>();

        builder.Services.AddScoped<IImageManager, ImageManager>();


        builder.RegisterSeedServices();
        return builder;
    }
    public static void AddLocalizationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ICountryLocalizationService, CountryLocalizationService>();
    }
}
