using MangaShelf.DAL.Interfaces;

namespace MangaShelf.DAL.System.Models;

public class ParserError : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset RunTime { get; set; }
    public string? Url { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? StackTrace { get; set; }
    public string? ErrorMessage { get; set; }
    public string? VolumeJson { get; set; }

    public Guid ParserRunId { get; set; }
    public virtual ParserJob ParserRun{ get; set; } = null!;
}
