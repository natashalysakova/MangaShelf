namespace MangaShelf.BL.Contracts;

public interface IParserJobManager
{
    Task<Guid> CreateSingleJob(string parserName, string url, CancellationToken token);
    Task<Guid> CreateParserJob(Guid parserId, CancellationToken token);
    Task<int> CreateScheduledJobs(TimeSpan delayBetweenRuns, CancellationToken token = default);
    Task InitializeParsers(IEnumerable<string> parsers, bool resetTime);
    Task<IEnumerable<Guid>> PrepareWaitingJobs();
    Task CancelJob(Guid parserId, CancellationToken token);
}
