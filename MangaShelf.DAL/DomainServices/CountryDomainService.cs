using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public class CountryDomainService : BaseDomainService<Country>, ICountryDomainService
{
    private readonly MangaDbContext _context;

    public CountryDomainService(MangaDbContext context) : base(context)
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

    public async Task UpdateFlagUrl(Guid id, string v)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country != null)
        {
            country.FlagUrl = v;
            await _context.SaveChangesAsync();
        }
    }
}
