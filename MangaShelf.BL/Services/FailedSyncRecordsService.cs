using MangaShelf.BL.Interfaces;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Services;

public class FailedSyncRecordsService : IFailedSyncRecordsService
{
    private readonly IFailedSyncRecordsDomainService _repository;

    public FailedSyncRecordsService(IFailedSyncRecordsDomainService repository)
    {
        _repository = repository;
    }
    public void CreateFailedSyncRecord(string url, string parser, string? error = null, string? volumeJson = null, Exception? exception = null)
    {
        var record = new FailedSyncRecords
        {
            Id = Guid.NewGuid(),
            Url = url,
            Parser = parser,
            ErrorMessage = error,
            ExceptionMessage = exception?.Message,
            StackTrace = exception?.StackTrace,
            VolumeJson = volumeJson
        };

        var existing = _repository.FindExisintg(url, error, exception);

        if (existing == null)
        {
            _repository.Add(record);
        }
    }
}