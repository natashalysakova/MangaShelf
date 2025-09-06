using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface IPublisherDomainService : IDomainService<Publisher>, IShelfDomainService
{
    Task<Publisher?> GetByNameAsync(string name, CancellationToken token = default);
}