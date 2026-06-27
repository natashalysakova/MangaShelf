namespace MangaShelf.BL.Contracts;

public interface IParseJobManagerService
{

    Task<Guid> CreateSingleJob(string parserName, string url, CancellationToken token);
    Task<Guid> CreateParserJob(Guid parserId, CancellationToken token);

    Task<int> CreateScheduledJobs(CancellationToken token);

    Task CancelJob(Guid parserId, CancellationToken token);
    Task InitializeParsers(IEnumerable<string> parsers, CancellationToken token);

    Task<IEnumerable<Guid>> PrepareWaitingJobs(CancellationToken token);

    Task RecordError(Guid jobId, Exception exception, string? url = null, CancellationToken token = default);
    Task RecordError(Guid jobId, string url, string json, Exception exception, CancellationToken token = default);
    Task RecordErrorAndStop(Guid jobId, Exception exception, string? url = null, CancellationToken token = default);    
    Task SetToParsingStatus(Guid jobId, IEnumerable<string> volumesToParse, CancellationToken token = default);
    Task SetProgress(Guid runId, double progress, ParseResult? result, CancellationToken token);
    Task SetToFinishedStatus(Guid jobId, CancellationToken token = default);
    Task SetToCancelledStatus(Guid jobId, CancellationToken token);
    Task RunJob(Guid jobId, CancellationToken token = default);

}
