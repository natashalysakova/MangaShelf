using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Contracts;

public interface IPublisherService : IService
{
    Task<IEnumerable<string>> GetAllNamesAsync(CancellationToken stoppingToken);
    Task<PublisherSimpleDto?> GetByNameAsync(string publisherName, CancellationToken token = default);
    Task<IReadOnlyCollection<AdminPublisherDto>> GetAllForAdminAsync(CancellationToken token = default);
    Task<IReadOnlyCollection<PublisherCountryOptionDto>> GetCountryOptionsAsync(CancellationToken token = default);
    Task<AdminPublisherDto> CreateAsync(PublisherUpsertDto dto, CancellationToken token = default);
    Task UpdateAsync(Guid publisherId, PublisherUpsertDto dto, CancellationToken token = default);
    Task DeleteAsync(Guid publisherId, CancellationToken token = default);
    Task RestoreAsync(Guid publisherId, CancellationToken token = default);
}