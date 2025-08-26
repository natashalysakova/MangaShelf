using MangaShelf.BL.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Services;

public interface ISeriesService : IService
{
    Task<Series> CreateFromParsedVolumeInfo(ParsedInfo volumeInfo);
    Task<Series?> FindByName(string series);
}