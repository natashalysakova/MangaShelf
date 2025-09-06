using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class PublisherMapper
{
    [MapProperty(nameof(Publisher.Country.CountryCode), nameof(PublisherSimpleDto.CountryCode))]
    [MapProperty(nameof(Publisher.Country.Name), nameof(PublisherSimpleDto.Country))]

    public static partial PublisherSimpleDto ToDto(this Publisher publisher);
}
