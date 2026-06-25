using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public class PublisherDomainService : BaseDomainService<Publisher>, IPublisherDomainService
{
    internal PublisherDomainService(MangaDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<string>> GetAllNamesAsync(CancellationToken stoppingToken)
    {
        var publisherNames = await _context.Publishers
            .AsNoTracking()
            .ToListAsync(stoppingToken);

        var names = publisherNames.Select(x => x.Name).Concat(publisherNames.SelectMany(x => x.AlternativeNames));

        return names.Distinct();
    }

    public async Task<Publisher?> GetByNameAsync(string name, CancellationToken token = default)
    {
        var publishers = await _context.Publishers
            .ToListAsync(token);
        
        return publishers.FirstOrDefault(p => p.Name == name || p.AlternativeNames.Contains(name));
    }
}