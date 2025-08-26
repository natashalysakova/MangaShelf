using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Interfaces;

public interface IAuthorService : IService
{
    Task<ICollection<Author>> GetByNames(IEnumerable<string> authors, bool createIfNotExists = false);
}