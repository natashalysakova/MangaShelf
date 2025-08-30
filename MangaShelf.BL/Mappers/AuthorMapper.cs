using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper]
public static partial class AuthorMapper
{
    public static partial AuthorDto ToDto(this Author country);
}
