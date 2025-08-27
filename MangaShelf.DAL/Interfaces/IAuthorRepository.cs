using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces
{
    public interface IAuthorRepository : IRepository<Author>
    {
        Task<Author?> GetByName(string name);
    }
}