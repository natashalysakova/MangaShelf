using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Contracts;

public interface IPublisherService : IService
{
    Task<IEnumerable<string>> GetAllNamesAsync(CancellationToken stoppingToken);
    Task<PublisherSimpleDto?> GetByNameAsync(string publisherName, CancellationToken token = default);
}