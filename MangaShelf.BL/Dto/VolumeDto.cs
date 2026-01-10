using System.Diagnostics.CodeAnalysis;
using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Dto;

[ExcludeFromCodeCoverage]   
public class VolumeDto
{
    public Guid Id { get; set; }
    public required string PublicId { get; set; }

    public required string Title { get; set; }
    public int Number { get; set; }
    public string? ISBN { get; set; }

    public bool OneShot { get; set; }
    public bool SingleIssue { get; set; }
    public int AgeRestriction { get; set; }

    public string? CoverImageUrl { get; set; }
    public string? PurchaseUrl { get; set; }

    public string? Description { get; set; }

    public bool IsPreorder { get; set; }
    public DateTimeOffset? PreorderStart { get; set; }
    public DateTimeOffset? ReleaseDate { get; set; }

    public double AvgRating { get; set; }

    public bool IsPublishedOnSite { get; set; }
    public VolumeType Type { get; set; }

    public SeriesSimpleDto Series { get; set; }
}