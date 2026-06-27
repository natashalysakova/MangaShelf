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
    public virtual ICollection<ParserError> Errors { get; set; } = new List<ParserError>();

    public virtual ICollection<VolumeReference> AddedVolumes { get; set; } = new List<VolumeReference>();
    public virtual ICollection<VolumeReference> UpdatedVolumes { get; set; } = new List<VolumeReference>();

    public Guid ParserStatusId { get; set; }
    public virtual Parser ParserStatus { get; set; } = null!;
    public double Progress { get; set; }
    public string? Url { get; set; }
}

public class VolumeReference : IEntity
{
    public Guid Id { get; set; }

    public Guid VolumeId { get; set; }
    public required string FullName { get; set; }
    public required string PublicId { get; set; }

    public Guid? AddedParserJobId { get; set; }
    public virtual ParserJob? AddedByJob { get; set; }

    public Guid? UpdatedParserJobId { get; set; }
    public virtual ParserJob? UpdatedByJob { get; set; }
}

public enum ParserRunType
{
    SingleUrl,
    FullSite
}