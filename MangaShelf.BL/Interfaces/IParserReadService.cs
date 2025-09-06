using MangaShelf.BL.Dto;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.System.Models;

namespace MangaShelf.BL.Interfaces;

public interface IParserReadService : IService
{
    Task<IEnumerable<ParserStatusDto>> GetStatusesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ParserJob>> GetJobs(int count = 100, CancellationToken cancellationToken = default);
    Task<IEnumerable<ParserJob>> GetNewJobsSince(DateTimeOffset dateTime, CancellationToken token = default);
    Task<ParserJob?> GetJobById(Guid jobId, CancellationToken token = default);
    Task<RunStatus> GetJobStatusById(Guid jobId, CancellationToken token);
}
