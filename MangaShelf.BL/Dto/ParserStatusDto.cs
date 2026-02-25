using MangaShelf.DAL.System.Models;
using System.Diagnostics.CodeAnalysis;

namespace MangaShelf.BL.Dto;

[ExcludeFromCodeCoverage]
public class ParserStatusDto
{
    public Guid Id { get; set; }
    public required string ParserName { get; set; }
    public ParserStatus Status { get; set; }
    public double Progress { get; set; }

    public DateTimeOffset NextRun { get; set; }

    public Guid? RunningJobId { get; set; }
}
