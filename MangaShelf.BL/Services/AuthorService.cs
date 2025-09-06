using MangaShelf.BL.Dto;
using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Mappers;
using MangaShelf.DAL;
using MangaShelf.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services;

public class AuthorService : IAuthorService
{
    private readonly ILogger<AuthorService> _logger;
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;
    public AuthorService(ILogger<AuthorService> logger, IDbContextFactory<MangaDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<AuthorDto>> GetByNamesAsync(IEnumerable<string> authors, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var serviceFactory = new DomainServiceFactory(context);
        var authorDomainService = serviceFactory.GetDomainService<IAuthorDomainService>();

        var result = await authorDomainService.GetOrCreateByNames(authors.ToArray(), token);
        return result.Select(x => x.ToDto());
    }
}


