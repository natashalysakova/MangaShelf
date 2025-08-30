using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface IPublisherDomainService : IDomainService<Publisher>
{
    Task<Publisher?> GetByName(string name);
}