using MangaShelf.BL.Dto;
using MangaShelf.Common;
using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Interfaces;

public interface IVolumeService : IService
{
    Task<IEnumerable<string>> FilterExistingVolumes(IEnumerable<string> volumesToParse);
    Task<(IEnumerable<VolumeDto>, int)> GetAllVolumesAsync(PaginationOptions? paginationOptions = null);
    Task<VolumeDto?> GetbyIdAsync(Guid id);

    Task<IEnumerable<VolumeDto>> GetLatestPreorders(int count = 6);
    Task<IEnumerable<VolumeDto>> GetNewestReleases(int count = 6);

}