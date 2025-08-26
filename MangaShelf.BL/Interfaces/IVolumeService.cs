namespace MangaShelf.BL.Interfaces;

public interface IVolumeService : IService
{
    Task CreateOrUpdateFromParsedInfoAsync(ParsedInfo volumeInfo);
}

public record ParsedInfo(
    string title,
    string? authors,
    int volumeNumber,
    string series, 
    string cover, 
    DateTime? release, 
    string publisher, 
    string type, 
    string isbn, 
    int totalVolumes, 
    string? seriesStatus, 
    string? originalSeriesName, 
    string url,
    DateTime? preorderStartDate,
    string countryCode
    );