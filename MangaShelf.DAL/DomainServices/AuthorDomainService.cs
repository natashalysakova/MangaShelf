using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public class AuthorDomainService : BaseDomainService<Author>, IAuthorDomainService
{
    internal AuthorDomainService(MangaDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<string>> GetAllNames(CancellationToken stoppingToken)
    {
        var names = await _context.Authors
            .AsNoTracking()
            .Select(a => a.Name)
            .ToListAsync(stoppingToken);
        return names;
    }

    public Author? GetByName(string name)
    {
        return _context.Authors.FirstOrDefault(x => x.Name == name);
    }

    public async Task<IEnumerable<Author>> GetOrCreateByNames(IEnumerable<string> autorsList, CancellationToken token)
    {
        var autorsInDb =  await _context.Authors.Where(a => autorsList.Contains(a.Name)).ToListAsync(token);
        var autorsToCreate = autorsList
            .Except(autorsInDb.Select(a => a.Name))
            .Select(name => new Author { Name = name });

        return autorsInDb.Concat(autorsToCreate).ToList();
    }
}