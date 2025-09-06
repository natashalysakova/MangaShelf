namespace MangaShelf.BL.Interfaces;

public interface IJobRequester
{
    Task<Guid> CreateSingleJob(string url, CancellationToken token = default);
    Task<Guid> CreateParserJob(Guid parserId, CancellationToken token = default);

    Task CancelJob(Guid jobId, CancellationToken token = default);
}
