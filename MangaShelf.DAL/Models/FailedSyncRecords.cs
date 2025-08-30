namespace MangaShelf.DAL.Models;

public class FailedSyncRecords : BaseEntity
{
    public required string Url { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? StackTrace { get; set; }
    public string? ErrorMessage { get; set; }
    public string? VolumeJson { get; set; }
    public required string Parser { get; set; }
}
