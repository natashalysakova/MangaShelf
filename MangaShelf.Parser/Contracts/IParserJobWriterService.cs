using MangaShelf.Common.Interfaces;

namespace MangaShelf.Parser.Contracts;

public interface IParserJobWriterService : IService
{
    Task RecordError( Guid jobId, string url, Exception ex, CancellationToken token = default);
    Task RecordError( Guid jobId, string url, string json, Exception ex, CancellationToken token = default);
    Task RecordErrorAndStop(Guid jobId, Exception? exception = null, CancellationToken token = default);
    Task SetToParsingStatus(Guid jobId, IEnumerable<string> volumesToParse, CancellationToken token = default);
    Task SetProgress(Guid jobId, double progress, string volume, bool isUpdated, CancellationToken token = default);
    Task SetToFinishedStatus(Guid jobId, CancellationToken token = default);
    Task<bool> RunJob(Guid jobId, CancellationToken token = default);
    Task RunSingleJob(Guid jobId);
    Task SetSingleJobToFinishedStatus(Guid jobId);
    Task SetSingleJobToErrorStatus(Guid jobId);
}
