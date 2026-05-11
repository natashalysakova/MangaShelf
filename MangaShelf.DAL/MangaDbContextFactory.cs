using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MangaShelf.DAL;

public class MangaDbContextFactory : IDesignTimeDbContextFactory<MangaDbContext>
{
    public MangaDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json")
             .Build();

        var connectionString = configuration.GetConnectionString("MangaDb") ?? throw new InvalidOperationException("Connection string 'MangaDb' not found.");

        var shelfDbVersion = ServerVersion.Parse("8.0");
        var options = new DbContextOptionsBuilder<MangaDbContext>()
            .UseMySql(connectionString, shelfDbVersion);
        return new MangaDbContext(options.Options);
    }
}
