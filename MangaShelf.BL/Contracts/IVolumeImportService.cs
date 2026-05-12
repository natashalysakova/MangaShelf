using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Contracts;

public interface IVolumeImportService
{
    Task<State> ImportAsync(ParsedInfo volumeInfo, CancellationToken token = default);
}
