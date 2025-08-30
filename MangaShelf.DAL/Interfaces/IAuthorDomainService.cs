using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface IAuthorDomainService : IDomainService<Author>
{
    Task<Author?> GetByName(string name);
    Task<ICollection<Author>> GetOrCreateByNames(string[] autorsList);
}