using MangaShelf.BL.Contracts;
using MangaShelf.Common.Interfaces;
using MangaShelf.Infrastructure.Installer;
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

        await host.RunAsync();
    }
}

public static class ParserInstaller
{
    public static IHostApplicationBuilder AddParserServices(this IHostApplicationBuilder builder)
    {
        // Parser services
        builder.Services.AddSingleton<IParseJobRunner, ParseJobRunner>();

        builder.Services.AddScoped<IParseService, ParserService>();
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