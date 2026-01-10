using MangaShelf.DAL.Interfaces;

namespace MangaShelf.DAL.System.Models;

public class ParserJob : IEntity
{
    public Guid Id { get; set; }

    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Started { get; set; }
    public DateTimeOffset? Finished { get; set; }

    public RunStatus Status { get; set; }
    public ParserRunType Type { get; set; }
    public int VolumesFound { get; set; }
    public ICollection<string> VolumesAdded { get; set; } = new List<string>();
    public ICollection<string> VolumesUpdated { get; set; } = new List<string>();
    public virtual ICollection<ParserError> Errors { get; set; } = new List<ParserError>();

    public Guid ParserStatusId { get; set; }
    public virtual Parser ParserStatus { get; set; } = null!;
    public double Progress { get; set; }
    public string? Url { get; set; }
}

public enum ParserRunType
{
    SingleUrl,
    FullSite
}