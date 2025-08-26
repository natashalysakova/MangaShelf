using MangaShelf.SeedService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace MangaShelf.Infrastructure.Installer;

internal static class SeedExtensions
{
    internal static void RegisterSeedServices(this IHostApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddScoped<ISeedDataService, SeedDevUsersService>();
            builder.Services.AddScoped<ISeedDataService, SeedDevShelfService>();
        }

        builder.Services.AddScoped<ISeedDataService, SeedProdUsersService>();
        builder.Services.AddScoped<ISeedDataService, SeedProdShelfService>();
    }

}
