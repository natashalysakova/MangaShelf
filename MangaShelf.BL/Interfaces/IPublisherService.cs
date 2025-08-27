using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Interfaces;

public interface IPublisherService : IService
{
    Task<Publisher> CreateFromParsedVolumeInfo(ParsedInfo volumeInfo);
    Task<Publisher?> GetByName(string publisher);
}