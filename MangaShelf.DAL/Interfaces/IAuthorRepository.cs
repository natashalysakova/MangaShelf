using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.Infrastructure.Installer
{
    public interface IAuthorRepository : IRepository<Author>
    {
        Task<Author?> GetByName(string name);
    }
}