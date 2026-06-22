using MangaShelf.BL.Contracts;
using MangaShelf.BL.Enums;
using MangaShelf.BL.Services;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.Common.Interfaces;
using MangaShelf.Infrastructure.Installer;
using MangaShelf.Infrastructure.Network;
using MangaShelf.Parser.Contracts;
using MangaShelf.Parser.Services;

namespace MangaShelf.Parser;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        builder.RegisterContextAndServices();
        builder.RegisterIdentityContextAndServices();
        builder.AddBusinessServices();
        builder.AddParserServices();

        var host = builder.Build();

        host.Run();
    }
}

public static class ParserInstaller
{
    public static IHostApplicationBuilder AddParserServices(this IHostApplicationBuilder builder)
    {
        // Parser services
        builder.Services.AddSingleton<IParseJobRunner, ParseJobRunner>();

        builder.Services.AddScoped<IParseService, ParserService>();
        builder.Services.AddScoped<IParserJobWriterService, ParserJobWiriterService>();
        builder.Services.AddScoped<IVolumeInfoParser, VolumeInfoParser>();

        var webAppBaseUrl = builder.Configuration["WebApp:BaseUrl"]
            ?? throw new InvalidOperationException("WebApp:BaseUrl is not configured.");

        var disableCacheInvalidation = builder.Configuration.GetValue<bool>("WebApp:DisableCacheInvalidation");

        if (disableCacheInvalidation)
        {
            builder.Services.AddSingleton<ICacheInvalidator, NullCacheInvalidator>();
        }
        else
        {
            builder.Services.AddHttpClient<ICacheInvalidator, HttpCacheInvalidator>(client =>
            {
                client.BaseAddress = new Uri(webAppBaseUrl);
            });
        }

        return builder;
    }
}