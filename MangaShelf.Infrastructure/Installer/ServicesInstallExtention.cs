using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MangaShelf.Common.Localization.Interfaces;
using MangaShelf.Common.Localization.Services;
using MangaShelf.BL.Enums;
using MangaShelf.BL.Parsers;
using MangaShelf.Infrastructure.Network;
using MangaShelf.Common.Interfaces;
using MangaShelf.BL.Services.Parsing;

namespace MangaShelf.Infrastructure.Installer;

public static class ServicesInstallExtention
{
    public static IHostApplicationBuilder AddBusinessServices(this IHostApplicationBuilder builder)
    {
        // Data services
        builder.Services.AddScoped<ICountryService, CountryService>();
        builder.Services.AddScoped<IVolumeService, VolumeService>();
        builder.Services.AddScoped<ISeriesService, SeriesService>();
        builder.Services.AddScoped<IAuthorService, AuthorService>();
        builder.Services.AddScoped<IPublisherService, PublisherService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IParserWriteService, ParserWriteService>();
        builder.Services.AddScoped<IParserReadService, ParserReadService>();

        // Parser services
        builder.Services.AddScoped<IHtmlDownloader, BasicHtmlDownloader>();
        builder.Services.AddKeyedScoped<IHtmlDownloader, BasicHtmlDownloader>(HtmlDownloaderKeys.Basic);
        builder.Services.AddKeyedScoped<IHtmlDownloader, AdvancedHtmlDownloader>(HtmlDownloaderKeys.Advanced);
        builder.Services.AddScoped<IParserWriteService, ParserWriteService>();
        builder.Services.AddScoped<IParserFactory, ParserFactory>();
        builder.Services.AddScoped<IParseService, ParserService>();
        builder.Services.AddScoped<IJobRequester, JobRequester>();

        RegisterInterfaceWithimplementations<IPublisherParser>(builder);

        // Image services
        builder.Services.AddScoped<IImageManager, ImageManager>();

        // Seed services
        builder.RegisterSeedServices();

        return builder;
    }

    private static void RegisterInterfaceWithimplementations<T>(IHostApplicationBuilder builder)
    {
        var assembly = typeof(T).Assembly;
        var baseType = typeof(T);
    
        // Find all non-abstract types that inherit from BaseParser
        var serviceTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))
            .ToList();
        
        // Register each parser type with its own type as the service key
        foreach (var serviceType in serviceTypes)
        {
            builder.Services.AddScoped(typeof(T), serviceType);
            builder.Services.AddKeyedScoped(typeof(T), serviceType.Name, serviceType);
        }
    }

    public static void AddLocalizationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ICountryLocalizationService, CountryLocalizationService>();
    }
}
