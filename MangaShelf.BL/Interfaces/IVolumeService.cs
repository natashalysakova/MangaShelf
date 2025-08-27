using MangaShelf.Common;
using MangaShelf.Common.Dto;
using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Interfaces;

public interface IVolumeService : IService
{
    Task CreateOrUpdateFromParsedInfoAsync(ParsedInfo volumeInfo);
    Task<IEnumerable<string>> FilterExistingVolumes(IEnumerable<string> volumesToParse);
    Task<IEnumerable<VolumeDto>> GetAllVolumesAsync(PaginationOptions? paginationOptions = null);
    Task<VolumeDto?> GetbyIdAsync(Guid id);

}



public record ParsedInfo(
    string title,
    string? authors,
    int volumeNumber,
    string series,
    string cover,
    DateTimeOffset? release,
    string publisher,
    string type,
    string isbn,
    int totalVolumes,
    string? seriesStatus,
    string? originalSeriesName,
    string url,
    DateTimeOffset? preorderStartDate,
    string countryCode,
    bool isPreorder
    );