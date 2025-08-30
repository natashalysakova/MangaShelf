using MangaShelf.Common;

namespace MangaShelf.BL.Interfaces;

public interface  IParsedVolumeService
{
    Task CreateOrUpdateFromParsedInfoAsync(ParsedInfo parsedInfo);
}
