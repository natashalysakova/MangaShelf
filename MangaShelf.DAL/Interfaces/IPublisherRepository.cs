using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.Infrastructure.Installer
{
    public interface IPublisherRepository : IRepository<Publisher>
    {
        Task<Publisher?> GetByName(string name);
    }
}