using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.Infrastructure.Installer;

public interface ISeriesRepository : IRepository<Series>
{
    Task<Series?> GetByTitle(string series);
}