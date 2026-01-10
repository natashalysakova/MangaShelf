using MangaShelf.BL.Dto;
using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Interfaces;

public interface IAuthorService : IService
{
    Task<IEnumerable<string>> GetAllNamesAsync(CancellationToken stoppingToken);
    Task<IEnumerable<AuthorDto>> GetByNamesAsync(IEnumerable<string> authors, CancellationToken token = default);
}