using MangaShelf.BL.Dto;
using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Interfaces;

public interface ISeriesService : IService
{
    Task<SeriesSimpleDto?> FindByName(string series);
}