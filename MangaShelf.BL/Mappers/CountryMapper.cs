using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper]
public static partial class CountryMapper
{
    [MapProperty(nameof(Country.CountryCode), nameof(CountryDto.Code))]
    public static partial CountryDto ToDto(this Country country);
}