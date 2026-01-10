using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Parsers;

public record ParsedInfo
{
    public required string Title { get; init; }
    public string? Authors { get; init; }
    public required int VolumeNumber { get; init; }
    public required string Series { get; init; }
    public required string Cover { get; init; }
    public DateTimeOffset? Release { get; init; }
    public required string Publisher { get; init; }
    public required VolumeType VolumeType { get; init; }
    public required string Isbn { get; init; }
    public required int TotalVolumes { get; init; }
    public required SeriesStatus SeriesStatus { get; init; }
    public string? OriginalSeriesName { get; init; }
    public required string Url { get; init; }
    public DateTimeOffset? PreorderStartDate { get; init; }
    public required string CountryCode { get; init; }
    public required bool IsPreorder { get; init; }
    public int? AgeRestrictions { get; init; }
    public required bool CanBePublished { get; init; }
    public string Json { get; set; } = string.Empty;
    public required SeriesType SeriesType { get; init; }
    public string? Description { get; init; }
}