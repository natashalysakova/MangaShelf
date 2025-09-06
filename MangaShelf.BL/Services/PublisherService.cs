using MangaShelf.BL.Interfaces;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;
using MangaShelf.BL.Mappers;
using MangaShelf.BL.Dto;
using Microsoft.EntityFrameworkCore;
using MangaShelf.DAL;

namespace MangaShelf.BL.Services;

public class PublisherService : IPublisherService
{
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;

    public PublisherService(ILogger<Publisher> logger, IDbContextFactory<MangaDbContext> dbContextFactory) 
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<PublisherSimpleDto?> GetByNameAsync(string publisherName, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var serviceFactory = new DomainServiceFactory(context);
        var publisherDomainService = serviceFactory.GetDomainService<IPublisherDomainService>();

        var publisher = await publisherDomainService.GetByNameAsync(publisherName, token);

        return publisher?.ToDto();
    }
}