using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public class CountryDomainService : BaseDomainService<Country>, ICountryDomainService
{
    internal CountryDomainService(MangaDbContext context) : base(context)
    {
    }

    public Task<Country?> GetByCountryCodeAsync(string countryCode, CancellationToken token = default)
    {
        return _context.Countries.SingleOrDefaultAsync(x => x.CountryCode.ToLower() == countryCode.ToLower(), token);
    }
}
