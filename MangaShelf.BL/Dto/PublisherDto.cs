namespace MangaShelf.BL.Dto;

public class PublisherSimpleDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Url { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
}
