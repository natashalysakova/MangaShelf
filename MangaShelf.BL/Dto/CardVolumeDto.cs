using System.Diagnostics.CodeAnalysis;

namespace MangaShelf.BL.Dto;

[ExcludeFromCodeCoverage]
public class CardVolumeDto
{
    public Guid Id { get; set; }
    public required string PublicId { get; set; }
    public required string SeriesName { get; set; }
    public required string Title { get; set; }
    public required string CoverImageUrlSmall { get; set; }
    public int Number { get; set; }
    public required string PurchaseUrl { get; set; }
    public bool IsPreorder { get; set; }
    public string? ReleaseDate { get; set; }
    public double AvgRating { get; set; }
}
