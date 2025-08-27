using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces
{
    public interface IPublisherRepository : IRepository<Publisher>
    {
        Task<Publisher?> GetByName(string name);
    }
}