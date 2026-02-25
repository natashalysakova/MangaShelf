using MangaShelf.BL.Dto;
using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Interfaces;

public interface ISeriesService : IService
{
    Task<SeriesSimpleDto?> FindByName(string series);
    Task<IEnumerable<string>> GetAllTitlesAsync(CancellationToken stoppingToken);
    Task<IEnumerable<SeriesWithVolumesDto>> GetAllWithVolumesAsync(CancellationToken token = default);
    Task UpdateSeriesAsync(SeriesUpdateDto dto, CancellationToken token = default);
}