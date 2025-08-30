using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface ISeriesDomainService : IDomainService<Series>
{
    Task<Series?> GetByTitle(string series);
}