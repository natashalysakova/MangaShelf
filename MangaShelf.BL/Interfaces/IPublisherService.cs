using MangaShelf.BL.Dto;
using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Interfaces;

public interface IPublisherService : IService
{
    Task<PublisherDto?> GetByName(string publisherName);
}