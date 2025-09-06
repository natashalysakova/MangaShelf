using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public class PublisherDomainService : BaseDomainService<Publisher>, IPublisherDomainService
{
    internal PublisherDomainService(MangaDbContext context) : base(context)
    {
    }

    public async Task<Publisher?> GetByNameAsync(string name, CancellationToken token = default)
    {
        return await _context.Publishers.FirstOrDefaultAsync(x => x.Name == name, token);
    }
}