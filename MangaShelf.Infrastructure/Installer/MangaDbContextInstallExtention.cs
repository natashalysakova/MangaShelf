using MangaShelf.DAL.MangaShelf;
using MangaShelf.DAL.MangaShelf.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MangaShelf.Infrastructure.Installer
{
    public static class MangaDbContextInstallExtention
    {

        public static IHostApplicationBuilder RegisterMangaDbContext(this IHostApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("MangaDb") ?? throw new InvalidOperationException("Connection string 'MangaDb' not found.");

            var accontDbVersion = ServerVersion.AutoDetect(connectionString);
            builder.Services.AddDbContext<MangaDbContext>(
                options =>
                {
                    options
                    .UseMySql(connectionString, accontDbVersion,
                    mysqlOptions =>
                    {
                        mysqlOptions.EnableRetryOnFailure();
                    })
                    .AddInterceptors(new AuditInterceptor());
                    
                    if (builder.Environment.IsDevelopment())
                    {
                        options.EnableSensitiveDataLogging();
                    }
                });

            return builder;
        }

        public static async Task MakeSureMangaDbCreatedAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MangaDbContext>();
                await dbContext.Database.MigrateAsync();
            }
        }
    }
}