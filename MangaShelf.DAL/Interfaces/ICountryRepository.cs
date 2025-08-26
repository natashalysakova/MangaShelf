using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface ICountryRepository : IRepository<Country>
{
    Task<ICollection<Country>> GetAllCountriesAsync();
    Task<Country?> GetByCountryCodeAsync(string countryCode);
}