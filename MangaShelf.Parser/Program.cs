using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Services;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.Infrastructure.Installer;
using MangaShelf.Infrastructure.Network;
using Microsoft.Extensions.Options;

namespace MangaShelf.Parser;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();
        builder.Services
            .Configure<BackgroundWorkerOptions>(
                builder.Configuration
                .GetSection(BackgroundWorkerOptions.SectionName));
        builder.Services
            .Configure<JobManagerOptions>(
                builder.Configuration
                .GetSection(JobManagerOptions.SectionName));
        builder.Services
            .Configure<HtmlDownloadOptions>(
                builder.Configuration
                .GetSection(HtmlDownloadOptions.SectionName));
        builder.Services
        .Configure<ParserServiceOptions>(
            builder.Configuration
            .GetSection(ParserServiceOptions.SectionName));


        builder.RegisterContextAndServices();
        builder.RegisterIdentityContextAndServices();
        builder.AddBusinessServices();

        builder.Services.AddSingleton<IParseJobManager, ParseJobManger>();

        var host = builder.Build();

        var _options = host.Services.GetRequiredService<IOptions<BackgroundWorkerOptions>>().Value;

        await Task.Delay(_options.StartDelay);

        await host.MakeSureDbCreatedAsync();
        await host.SeedDatabase();

        host.Run();
    }
}