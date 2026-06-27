using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Contracts;

public class ParseResult
{
    public ParseVolumeReference VolumeReference { get; set; }
    public State State { get; set; }
    public ParseResult(ParseVolumeReference reference, State state)
    {
        VolumeReference = reference;
        State = state;
    }
}

public class ParseVolumeReference
{
    public Guid VolumeId { get; set; }
    public required string FullName { get; set; }
    public required string PublicId { get; set; }
}