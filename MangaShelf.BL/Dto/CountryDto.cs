using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class CountryDto
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string FlagUrl { get; set; }
}