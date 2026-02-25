using MangaShelf.Common.Interfaces;
using MangaShelf.Infrastructure.Installer;
using MangaShelf.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<SeedService>();

        builder.RegisterIdentityContextAndServices();
        builder.RegisterContextAndServices();

        builder.Services.AddScoped<IImageManager, ImageManager>();

        InstallSeedServices(builder);


        var host = builder.Build();

        host.Run();
    }

    private static void InstallSeedServices(HostApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddScoped<ISeedDataService, SeedDevUsersService>();
            builder.Services.AddScoped<ISeedDataService, SeedDevShelfService>();
            builder.Services.AddScoped<ISeedDataService, SeedDevSystemService>();
        }

        builder.Services.AddScoped<ISeedDataService, SeedProdUsersService>();
        builder.Services.AddScoped<ISeedDataService, SeedProdShelfService>();
        builder.Services.AddScoped<ISeedDataService, SeedProdSystemService>();

    }
}
