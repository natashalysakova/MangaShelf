namespace MangaShelf.BL.Services;

public interface IVolumeHistoryService
{
    Task<IEnumerable<VolumeHistoryDto>> GetVolumeHistoryAsync(DateTime from, DateTime to, CancellationToken cancellationToken);
}
