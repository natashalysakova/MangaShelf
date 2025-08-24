using MangaShelf.DAL.MangaShelf;
using MangaShelf.DAL.MangaShelf.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.Interfaces
{
    public interface ICountryRepository : IRepository
    {
        Task<ICollection<Country>> GetAllCountriesAsync();
        Task<Country?> GetByCountryCodeAsync(string countryCode);
    }

    public class CountryRepository : ICountryRepository
    {
        private readonly MangaDbContext _context;

        public CountryRepository(MangaDbContext context)
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
                .SingleOrDefaultAsync(x => x.CountryCode == countryCode);
        }
    }
}
