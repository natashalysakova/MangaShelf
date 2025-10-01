using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface IPublisherDomainService : IDomainService<Publisher>, IShelfDomainService
{
    Task<IEnumerable<string>> GetAllNamesAsync(CancellationToken stoppingToken);
    Task<Publisher?> GetByNameAsync(string name, CancellationToken token = default);
}