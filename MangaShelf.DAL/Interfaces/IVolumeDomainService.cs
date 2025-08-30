using MangaShelf.Common;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface IVolumeDomainService : IDomainService<Volume>
{
    Task<Volume?> FindBySeriesNameTitleAndNumber(string series, int volumeNumber, string volumeTitle);
    Task<(IEnumerable<Volume> volumes, int totalPages)> GetAllWithSeries(PaginationOptions? paginationOptions = null);
    Task<IEnumerable<Volume>> GetLatestPreorders(int count);
    Task<IEnumerable<Volume>> GetNewestReleases(int count);
}
