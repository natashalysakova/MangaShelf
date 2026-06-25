using MangaShelf.Common.Interfaces;
using MangaShelf.Infrastructure.Installer;
using MangaShelf.Infrastructure.Seed;
using MangaShelf.Migration.DataCorrections;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<SeedService>();

        builder.RegisterIdentityContextAndServices();
        builder.RegisterContextAndServices();

        builder.Services.AddScoped<IImageManager, ImageManager>();
        RegisterDataCorrections(builder);

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

    private static void RegisterDataCorrections(HostApplicationBuilder builder)
    {
        var correctionTypes = typeof(Program).Assembly
            .GetTypes()
            .Where(type => typeof(IDataCorrection).IsAssignableFrom(type)
                           && type is { IsClass: true, IsAbstract: false });

        foreach (var correctionType in correctionTypes)
        {
            builder.Services.AddScoped(typeof(IDataCorrection), correctionType);
        }
    }
}
