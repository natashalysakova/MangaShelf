using MangaShelf.Common;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces
{
    public interface IVolumeRepository : IRepository<Volume>
    {
        Task<Volume?> FindBySeriesNameAndNumber(string series, int volumeNumber);
        Task<IEnumerable<Volume>> GetAllWithSeries(PaginationOptions? paginationOptions = null);
    }
}
