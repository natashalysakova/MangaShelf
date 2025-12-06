using System.Diagnostics.CodeAnalysis;
using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Dto;

[ExcludeFromCodeCoverage]
public class SeriesSimpleDto
{
    public Guid Id { get; set; }

    public required string Title { get; set; }
    public string? OriginalName { get; set; }
    public ICollection<string> Aliases { get; set; } = new List<string>();
    public int MalId { get; set; }

    public SeriesType Type { get; set; }
    public SeriesStatus Status { get; set; }

    public int? TotalVolumes { get; set; }

    public bool IsPublishedOnSite { get; set; }

    public Guid PublisherId { get; set; }
    public PublisherSimpleDto Publisher { get; set; }

    public ICollection<string> Authors { get; set; } = new List<string>();
}
