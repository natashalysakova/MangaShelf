using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public class AuthorDomainService : BaseDomainService<Author>, IAuthorDomainService
{
    public AuthorDomainService(MangaDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Author?> GetByName(string name)
    {
        return await _context.Authors.FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<ICollection<Author>> GetOrCreateByNames(string[] autorsList)
    {
        var autorsInDb = await _context.Authors.Where(a => autorsList.Contains(a.Name)).ToListAsync();
        var autorsToCreate = autorsList
            .Except(autorsInDb.Select(a => a.Name))
            .Select(name => new Author { Name = name });

        return autorsInDb.Concat(autorsToCreate).ToList();
    }
}