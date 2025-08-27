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

        var host = builder.Build();

        await host.Services.MakeSureDbCreatedAsync();
        await host.Services.SeedDatabase();

        host.Run();
    }
}