using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Interfaces;

public interface IParserWriteService : IService
{
    Task RecordError( Guid runId, string url, Exception ex, CancellationToken token = default);
    Task RecordError( Guid runId, string url, string json, Exception ex, CancellationToken token = default);
    Task RecordErrorAndStop(Guid runId, Exception? exception = null, CancellationToken token = default);
    Task SetToParsingStatus(Guid runId, IEnumerable<string> volumesToParse, CancellationToken token = default);
    Task SetProgress(Guid runId, double progress, string volume, bool isUpdated, CancellationToken token = default);
    Task SetToFinishedStatus(Guid runId, CancellationToken token = default);
    Task<bool> RunJob(Guid jobId, CancellationToken token = default);
    
    
    Task<Guid> CreateSingleJob(string parserName, string url, CancellationToken token);
    Task<Guid> CreateParserJob(Guid parserId, CancellationToken token);
    Task<int> CreateScheduledJobs(TimeSpan delayBetweenRuns, CancellationToken token = default);
    
    Task CancelJob(Guid parserId, CancellationToken token);
    
    
    Task<IEnumerable<Guid>> PrepareWaitingJobs();
    Task InitializeParsers(IEnumerable<string> parsers, bool resetTime);
    Task RunSingleJob(Guid jobId);
    Task SetSingleJobToFinishedStatus(Guid jobId);
    Task SetSingleJobToErrorStatus(Guid jobId);
}
