using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MangaShelf.Infrastructure.Installer;

internal class Program
{
    private async static Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.RegisterContextAndServices();
        builder.RegisterIdentityContextAndServices();
        builder.AddBusinessServices();

        using (IHost host = builder.Build())
        {
            host.Start();

            await host.MakeSureDbCreatedAsync();
            await host.SeedDatabase();

            await host.StopAsync();
        }



    }
}