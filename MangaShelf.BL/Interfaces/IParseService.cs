namespace MangaShelf.BL.Interfaces;

public interface IParseService
{
    Task RunParseJob(Guid jobId, CancellationToken token);
}