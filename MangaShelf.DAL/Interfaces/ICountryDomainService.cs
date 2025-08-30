using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface ICountryDomainService : IDomainService<Country>
{
    Task<ICollection<Country>> GetAllCountriesAsync();
    Task<Country?> GetByCountryCodeAsync(string countryCode);
    Task UpdateFlagUrl(Guid id, string v);
}