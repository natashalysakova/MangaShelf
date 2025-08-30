namespace MangaShelf.BL.Interfaces;

public interface IFailedSyncRecordsService
{
    void CreateFailedSyncRecord(string url, string parser, string? error = null, string? volumeJson = null, Exception? exception = null);
}
