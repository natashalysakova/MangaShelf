using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Interfaces;

public interface ISeriesService : IService
{
    Task<Series> CreateFromParsedVolumeInfo(ParsedInfo volumeInfo);
    Task<Series?> FindByName(string series);
}