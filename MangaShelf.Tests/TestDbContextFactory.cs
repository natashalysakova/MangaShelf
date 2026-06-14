using MangaShelf.DAL;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.Tests;

// Helper class for creating DbContextFactory
public class TestDbContextFactory : IDbContextFactory<MangaDbContext>
{
    private readonly DbContextOptions<MangaDbContext> _options;

    public TestDbContextFactory(DbContextOptions<MangaDbContext> options)
    {
        _options = options;
    }

    public MangaDbContext CreateDbContext()
    {
        return new TestMangaDbContext(_options);
    }
}
