using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[UseStaticMapper(typeof(OwnershipMapper))]
public static partial class OwnershipMapper
{
    public static partial OwnershipHistoryDto ToOwnershipHistoryDto(this Ownership ownership);
}