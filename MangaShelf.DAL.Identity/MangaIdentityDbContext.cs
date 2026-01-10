using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MangaShelf.DAL.Identity;

public class MangaIdentityDbContext : IdentityDbContext<MangaIdentityUser>
{
    public MangaIdentityDbContext(DbContextOptions<MangaIdentityDbContext> options) : base(options)
    {

    }
}

public class MangaIdentityUser : IdentityUser
{
}

public class MangaIdentityDbContextFactory : IDesignTimeDbContextFactory<MangaIdentityDbContext>
{
    public MangaIdentityDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json")
             .Build();

        var connectionString = configuration.GetConnectionString("AccountsDb") ?? throw new InvalidOperationException("Connection string 'AccountsDb' not found.");

        var shelfDbVersion = ServerVersion.AutoDetect(connectionString);
        var options = new DbContextOptionsBuilder<MangaIdentityDbContext>()
            .UseMySql(connectionString, shelfDbVersion);
        return new MangaIdentityDbContext(options.Options);
    }
}
