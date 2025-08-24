using MangaShelf.Infrastructure.Installer;

namespace MangaShelf.SeedService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            try
            {
                builder.RegisterMangaDbContext();

                builder.RegisterIdentityContextAndServices();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Environment.Exit(-1);
            }

            RegisterSeedServices(builder);
            builder.Services.AddHostedService<SeedWorker>();

            using var cts = new CancellationTokenSource();
            var host = builder.Build();

            try
            {
                await host.Services.MakeSureAccountDbCreatedAsync();
                await host.Services.MakeSureMangaDbCreatedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Environment.Exit(-1);
            }

            host.Run();
        }

        private static void RegisterSeedServices(HostApplicationBuilder builder)
        {
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddScoped<ISeedDataService,SeedDevUsersService>();
                builder.Services.AddScoped<ISeedDataService, SeedDevShelfService>();
            }

            builder.Services.AddScoped<ISeedDataService, SeedProdUsersService>();
            builder.Services.AddScoped<ISeedDataService, SeedProdShelfService>();
        }
    }
}