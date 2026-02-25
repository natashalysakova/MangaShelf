using MangaShelf.BL.Interfaces;
using MangaShelf.DAL;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.BL.Services;

public class SeedService : ISeedService
{
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;

    public SeedService(IDbContextFactory<MangaDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }


    public Country GetCountryByCode(string countryCode)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Countries.FirstOrDefault(c => c.CountryCode == countryCode);
    }
}