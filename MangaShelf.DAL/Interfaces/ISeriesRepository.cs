using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces
{
    public interface ISeriesRepository : IRepository<Series>
    {
        Task<Series?> GetByTitle(string series);
    }
}