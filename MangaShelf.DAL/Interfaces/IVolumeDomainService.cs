using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface IVolumeDomainService : IDomainService<Volume>, IShelfDomainService
{
    Volume? FindBySeriesNameTitleAndNumber(string series, int volumeNumber, string volumeTitle);
    IQueryable<Volume> GetAllFullPaginated(IFilterOptions? paginationOptions = default);
    IQueryable<Volume> GetAllWithSeries(IFilterOptions? paginationOptions = default);

    Volume? GetFullVolume(Guid id);
    IQueryable<Volume> GetLatestPreorders(int count);
    IQueryable<Volume> GetNewestReleases(int count);


    IQueryable<Volume>GetAllFull();
}
