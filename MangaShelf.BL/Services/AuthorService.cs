using MangaShelf.BL.Dto;
using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Mappers;
using MangaShelf.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services;

public class AuthorService : IAuthorService
{
    private readonly ILogger<AuthorService> _logger;
    private readonly IAuthorDomainService _authorDomainService;
    public AuthorService(ILogger<AuthorService> logger, IAuthorDomainService authorDomainService)
    {
        _logger = logger;
        _authorDomainService = authorDomainService;
    }

    public async Task<IEnumerable<AuthorDto>> GetByNames(IEnumerable<string> authors)
    {
        var result = await _authorDomainService.GetOrCreateByNames(authors.ToArray());
        return result.Select(x => x.ToDto());
    }
}
