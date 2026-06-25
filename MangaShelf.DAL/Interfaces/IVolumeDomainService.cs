using MangaShelf.Common.Interfaces;
using MangaShelf.Common.Models;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface IVolumeDomainService : IDomainService<Volume>, IShelfDomainService
{
    Volume? FindVolumeFromParsedInfo(Guid seriesId, VolumeInfoRequest volumeInfo);
    IQueryable<Volume> GetAllFullPaginated(IFilterOptions? paginationOptions = default);
    IQueryable<Volume> GetAllWithSeries(IFilterOptions? paginationOptions = default);

    Volume? GetFullVolume(Guid id);
    IQueryable<Volume> GetLatestPreorders(int count);
    IQueryable<Volume> GetNewestReleases(int count);


    IQueryable<Volume>GetAllFull();
}


