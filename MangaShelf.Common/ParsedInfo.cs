namespace MangaShelf.Common;

public record ParsedInfo(
    string title,
    string? authors,
    int volumeNumber,
    string series,
    string cover,
    DateTimeOffset? release,
    string publisher,
    string type,
    string isbn,
    int totalVolumes,
    string? seriesStatus,
    string? originalSeriesName,
    string url,
    DateTimeOffset? preorderStartDate,
    string countryCode,
    bool isPreorder,
    int? ageRestrictions
    )
{
    public string json { get; set; }
}
