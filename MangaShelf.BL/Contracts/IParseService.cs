namespace MangaShelf.BL.Contracts;

public interface IParseService
{
    Task RunParseJob(Guid jobId, CancellationToken token);
}