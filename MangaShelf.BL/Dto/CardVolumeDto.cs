using System.Diagnostics.CodeAnalysis;

namespace MangaShelf.BL.Dto;

[ExcludeFromCodeCoverage]
public class CardVolumeDto
{
    public Guid Id { get; set; }
    public string PublicId { get; set; }
    public string SeriesName { get; set; }
    public string Title { get; set; }
    public string CoverImageUrlSmall { get; set; }
    public int Number { get; set; }
    public string PurchaseUrl { get; set; }
    public bool IsPreorder { get; set; }
    public string? ReleaseDate { get; set; }
    public double AvgRating { get; set; }

    public bool CoverNeedAdjustment { get; set; }

}
