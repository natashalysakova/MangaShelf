using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public class PublisherDomainService : BaseDomainService<Publisher>, IPublisherDomainService
{
    public PublisherDomainService(MangaDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Publisher?> GetByName(string name)
    {
        return await _context.Publishers.FirstOrDefaultAsync(x => x.Name == name);
    }
}