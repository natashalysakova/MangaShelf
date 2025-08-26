using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.Repositories;

public class CountryRepository : BaseRepository<Country>, ICountryRepository
{
    private readonly MangaDbContext _context;

    public CountryRepository(MangaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ICollection<Country>> GetAllCountriesAsync()
    {
        return await _context.Countries
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Country?> GetByCountryCodeAsync(string countryCode)
    {
        return await _context.Countries
            .SingleOrDefaultAsync(x => x.CountryCode.ToLower() == countryCode.ToLower());
    }
}
