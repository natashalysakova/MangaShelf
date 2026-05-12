namespace MangaShelf.BL.Contracts;

public interface IParserRunTracker
{
    Task<bool> RunJob(Guid jobId, CancellationToken token = default);
    Task RunSingleJob(Guid jobId);
    Task SetProgress(Guid runId, double progress, string volume, bool isUpdated, CancellationToken token = default);
    Task SetToParsingStatus(Guid runId, IEnumerable<string> volumesToParse, CancellationToken token = default);
    Task SetToFinishedStatus(Guid runId, CancellationToken token = default);
    Task RecordError(Guid runId, string url, Exception ex, CancellationToken token = default);
    Task RecordError(Guid runId, string url, string json, Exception ex, CancellationToken token = default);
    Task RecordErrorAndStop(Guid runId, Exception? exception = null, CancellationToken token = default);
    Task SetSingleJobToFinishedStatus(Guid jobId);
    Task SetSingleJobToErrorStatus(Guid jobId);
}
