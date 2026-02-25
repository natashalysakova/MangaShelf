using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MangaShelf.Infrastructure.Installer;
using System.Diagnostics;

public class Program
{
    public async static Task Main(string[] args)
    {
        Debugger.Launch();
        var builder = Host.CreateApplicationBuilder(args);

        builder.RegisterIdentityContextAndServices();
        builder.RegisterContextAndServices();
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