using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface ICountryDomainService : IDomainService<Country>, IShelfDomainService
{
    Task<Country?> GetByCountryCodeAsync(string countryCode, CancellationToken token = default);
}