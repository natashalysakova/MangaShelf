using MangaShelf.BL.Dto;
using MangaShelf.BL.Services;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Interfaces;

public interface IVolumeService : IService
{
    Task<IEnumerable<string>> FilterExistingVolumes(IEnumerable<string> volumesToParse, CancellationToken token = default);
    Task<(IEnumerable<CardVolumeDto>, int)> GetAllVolumesAsync(IFilterOptions? paginationOptions = default, CancellationToken token = default);
    Task<VolumeDto?> GetFullVolumeByIdAsync(Guid id, CancellationToken token = default);
    Task<IEnumerable<CardVolumeDto>> GetLatestPreorders(int count = 6, CancellationToken token = default);
    Task<IEnumerable<CardVolumeDto>> GetNewestReleases(int count = 6, CancellationToken token = default);


    Task<(IEnumerable<Volume>, int)> GetAllFullVolumesAsync(IFilterOptions paginationOptions, IEnumerable<Func<Volume, bool>>? filterFunctions, IEnumerable<SortDefinitions<Volume>> sortDefinitions);
    Task<IEnumerable<string>> GetAllTitlesAsync(CancellationToken stoppingToken);

    Task<UserVolumeStatus> GetVolumeUserStatusAsync(Guid volumeId, string userId, CancellationToken token = default);
    Task<IEnumerable<CardVolumeDto>> GetVolumesBySeriesId(Guid seriesId, CancellationToken token = default);
}
