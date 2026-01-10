using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface ISeriesDomainService : IDomainService<Series>, IShelfDomainService
{
    Task<IEnumerable<string>> GetAllTitlesAsync(CancellationToken stoppingToken);
    Task<Series?> GetByTitleAsync(string series, CancellationToken token = default);
}