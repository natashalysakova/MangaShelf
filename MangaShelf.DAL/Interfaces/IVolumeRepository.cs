using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface IVolumeRepository : IRepository<Volume>
{
    Task<Volume?> FindBySeriesNameAndNumber(string series, int volumeNumber);
}
