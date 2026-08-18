using System.Text;
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
    [MapPropertyFromSource(nameof(ParserStatusDto.ParserName), Use = nameof(MapName))]
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

    private static string MapName(Parser parser)
    {
        var parserName = parser.ParserName;
        if (string.IsNullOrWhiteSpace(parserName))
            return string.Empty;

        var nameWithoutSuffix = parserName.Replace("Parser", string.Empty);
        if (nameWithoutSuffix.Length == 0)
            return string.Empty;

        var formattedName = new StringBuilder();
        formattedName.Append(nameWithoutSuffix[0]);

        for (var i = 1; i < nameWithoutSuffix.Length; i++)
        {
            var currentChar = nameWithoutSuffix[i];
            if (char.IsUpper(currentChar))
                formattedName.Append('\u00A0');

            formattedName.Append(currentChar);
        }

        return formattedName.ToString();
    }
}