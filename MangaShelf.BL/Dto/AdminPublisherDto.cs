using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class AdminPublisherDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Url { get; set; }
    public Guid CountryId { get; set; }
    public string? CountryName { get; set; }
    public string? CountryCode { get; set; }
    public bool IsDeleted { get; set; }
}

[ExcludeFromCodeCoverage]
public class PublisherCountryOptionDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string CountryCode { get; set; }
}

[ExcludeFromCodeCoverage]
public class PublisherUpsertDto
{
    public required string Name { get; set; }
    public string? Url { get; set; }
    public Guid CountryId { get; set; }
}
