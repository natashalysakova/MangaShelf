using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.DomainServices;

public class FailedSyncRecordsDomainService : BaseDomainService<FailedSyncRecords>, IFailedSyncRecordsDomainService
{
    public FailedSyncRecordsDomainService(MangaDbContext dbContext) : base(dbContext)
    {
    }

    public FailedSyncRecords? FindExisintg(string url, string? error = null, Exception? exception = null)
    {
        return _context.FailedSyncRecords.FirstOrDefault(x => x.Url == url && (error == x.ErrorMessage || exception != null && exception.Message == x.ErrorMessage));
    }
}
