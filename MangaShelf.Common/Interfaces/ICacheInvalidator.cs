namespace MangaShelf.Common.Interfaces;

public interface ICacheInvalidator
{
    Task RebuildCache(CancellationToken cancellationToken = default);
}
