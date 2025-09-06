using MangaShelf.BL.Dto;
using MangaShelf.DAL.System.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class ParserRunMapper
{
}


[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class ParserMapper
{
    [MapPropertyFromSource(nameof(ParserStatusDto.Progress), Use = nameof(MapProgress))]
    [MapPropertyFromSource(nameof(ParserStatusDto.RunningJobId), Use = nameof(MapRunningJobs))]
    public static partial ParserStatusDto ToStatusDto(this Parser parser);

    private static Guid? MapRunningJobs(Parser parser)
    {
        if (parser.Jobs == null || !parser.Jobs.Any())
            return null;

        return parser.Jobs.First().Id;
    }

    private static double MapProgress(Parser parser)
    {
        if(parser.Jobs == null || !parser.Jobs.Any())
            return -1;

        return parser.Jobs.First().Progress;
    }
}