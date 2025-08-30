using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.Interfaces;

public interface IFailedSyncRecordsDomainService : IDomainService<FailedSyncRecords>
{
    FailedSyncRecords? FindExisintg(string url, string? error = null, Exception? exception = null);
}
