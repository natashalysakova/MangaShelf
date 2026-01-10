using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[UseStaticMapper(typeof(PublisherMapper))]
public static partial class SeriesMapper
{
    public static partial SeriesSimpleDto ToDto(this Series country);
}