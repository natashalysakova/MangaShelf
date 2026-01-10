using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MangaShelf.DAL.System;

public class MangaSystemDbContextFactory : IDesignTimeDbContextFactory<MangaSystemDbContext>
{
    public MangaSystemDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json")
             .Build();

        var connectionString = configuration.GetConnectionString("SystemDb") ?? throw new InvalidOperationException("Connection string 'SystemDb' not found.");

        var shelfDbVersion = ServerVersion.AutoDetect(connectionString);
        var options = new DbContextOptionsBuilder<MangaSystemDbContext>()
            .UseMySql(connectionString, shelfDbVersion);
        return new MangaSystemDbContext(options.Options);
    }
}
