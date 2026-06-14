namespace MangaShelf.BL.Contracts;

public interface IParseJobManagerService
{

    Task<Guid> CreateSingleJob(string parserName, string url, CancellationToken token);
    Task<Guid> CreateParserJob(Guid parserId, CancellationToken token);

    Task<int> CreateScheduledJobs(CancellationToken token);

    Task CancelJob(Guid parserId, CancellationToken token);
    Task InitializeParsers(IEnumerable<string> parsers, CancellationToken token);

    Task<IEnumerable<Guid>> PrepareWaitingJobs(CancellationToken token);
}