using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Services;
using MangaShelf.Infrastructure.Installer;

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

        builder.Services.AddSingleton<IParseJobManager, ParseJobManger>();

        var host = builder.Build();

        using (var scope = host.Services.CreateScope())
        {
            var _options = scope.ServiceProvider.GetRequiredService<IConfigurationService>().BackgroundWorker;
            await Task.Delay(_options.StartDelay);
        }

        await host.MakeSureDbCreatedAsync();
        await host.SeedDatabase();

        host.Run();
    }
}