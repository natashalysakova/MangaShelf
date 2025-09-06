using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface IAuthorDomainService : IDomainService<Author>, IShelfDomainService
{
    Author? GetByName(string name);
    Task<IEnumerable<Author>> GetOrCreateByNames(IEnumerable<string> autorsList, CancellationToken token);
}