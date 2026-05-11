using MangaShelf.BL.Contracts;

namespace MangaShelf.Parser;

public interface IParseJobManager
{
    Task<int> CreateScheduledJobs(CancellationToken token = default);
    Task InitializeParser(IEnumerable<IPublisherParser> parsers);
    Task RunScheduledJobs(CancellationToken token = default);

}