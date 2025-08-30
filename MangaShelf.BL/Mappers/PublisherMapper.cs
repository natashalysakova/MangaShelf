using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper]
public static partial class PublisherMapper
{
    public static partial PublisherDto ToDto(this Publisher country);
}
