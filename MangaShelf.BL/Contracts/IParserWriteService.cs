using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Contracts;

/// <summary>
/// Unified write service for backward compatibility. Prefer injecting
/// <see cref="IParserJobManager"/> or <see cref="IParserRunTracker"/> directly.
/// </summary>
public interface IParserWriteService : IService, IParserJobManager, IParserRunTracker
{
}
