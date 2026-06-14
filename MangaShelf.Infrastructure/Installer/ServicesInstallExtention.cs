using MangaShelf.BL.Configuration;
using MangaShelf.BL.Contracts;
using MangaShelf.BL.Enums;
using MangaShelf.BL.Parsers;
using MangaShelf.BL.Services;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.Common.Interfaces;
using MangaShelf.Common.Localization.Interfaces;
using MangaShelf.Common.Localization.Services;
using MangaShelf.Infrastructure.Network;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

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
        builder.Services.AddScoped<IParserReadService, ParserReadService>();
        builder.Services.AddScoped<IParserWriteService, ParserWriteService>();
        builder.Services.AddScoped<ISettingReadService, SettingReadService>();
        builder.Services.AddScoped<IUserLibraryService, UserLibraryService>();

        // Parsing services
        builder.Services.AddScoped<IHtmlDownloader, BasicHtmlDownloader>();
        builder.Services.AddKeyedScoped<IHtmlDownloader, BasicHtmlDownloader>(HtmlDownloaderKeys.Basic);
        builder.Services.AddKeyedScoped<IHtmlDownloader, AdvancedHtmlDownloader>(HtmlDownloaderKeys.Advanced);
        builder.Services.AddKeyedScoped<IHtmlDownloader, MalopusHtmlDownloader>(HtmlDownloaderKeys.Malopus);
        builder.Services.AddScoped<IParserFactory, ParserFactory>();
        builder.Services.AddScoped<IJobRequester, JobRequester>();
        RegisterInterfaceWithimplementations<IPublisherParser>(builder);

        // Image services
        builder.Services.AddScoped<IImageManager, ImageManager>();

        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<IConfigurationService, ConfigurationService>();

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

    public static void UseRequestLocalization(this WebApplication app)
    {
        string[] supportedCultures = LocalizationService.SupportedCultures.Select(x => x.Name).ToArray();
        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture(supportedCultures[0])
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);
        localizationOptions.ApplyCurrentCultureToResponseHeaders = true;
        app.UseRequestLocalization(localizationOptions);
    }

    public static IServiceCollection AddLocalizationServicesFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        var localizationServiceType = typeof(ILocalizationService<>);

        var serviceTypes = assembly.GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == localizationServiceType))
            .ToList();

        foreach (var implementationType in serviceTypes)
        {
            // Find the most specific interface (non-generic preferred)
            var interfaceType = implementationType.GetInterfaces()
                .Where(i => i != typeof(IAutoRegisterLocalizationService) &&
                           (i.IsGenericType && i.GetGenericTypeDefinition() == localizationServiceType ||
                            i.GetInterfaces().Any(ii => ii.IsGenericType && ii.GetGenericTypeDefinition() == localizationServiceType)))
                .OrderBy(i => i.IsGenericType ? 1 : 0) // Prefer non-generic interfaces
                .FirstOrDefault();

            if (interfaceType != null)
            {
                services.AddSingleton(interfaceType, implementationType);
            }
        }

        return services;
    }
}
