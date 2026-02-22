using MangaShelf.DAL.Exceptions;

namespace MangaShelf.DAL.Extensions;

public static class BaseEntityExtensions
{
    public static void ThrowIfNotFound(this BaseEntity? entity)
    {
        if (entity is null)
        {
            throw new EntityNotFoundException();
        }
    }
}
