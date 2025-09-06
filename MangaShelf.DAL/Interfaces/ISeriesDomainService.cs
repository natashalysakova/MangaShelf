using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface ISeriesDomainService : IDomainService<Series>, IShelfDomainService
{
    Task<Series?> GetByTitleAsync(string series, CancellationToken token = default);
}