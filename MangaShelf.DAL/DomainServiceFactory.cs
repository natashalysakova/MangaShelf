using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.DomainServices;
using MangaShelf.DAL.Interfaces;

namespace MangaShelf.DAL;

public class DomainServiceFactory
{
    private readonly MangaDbContext _context;

    public DomainServiceFactory(MangaDbContext context)
    {
        _context = context;
    }

    public T GetDomainService<T>() where T : IShelfDomainService
    {
        return typeof(T) switch
        {
            var t when t == typeof(IVolumeDomainService) => (T)(object)new VolumeDomainService(_context),
            var t when t == typeof(ISeriesDomainService) => (T)(object)new SeriesDomainService(_context),
            var t when t == typeof(IAuthorDomainService) => (T)(object)new AuthorDomainService(_context),
            var t when t == typeof(IPublisherDomainService) => (T)(object)new PublisherDomainService(_context),
            var t when t == typeof(ICountryDomainService) => (T)(object)new CountryDomainService(_context),
            var t when t == typeof(IUserDomainService) => (T)(object)new UserDomainService(_context),
            _ => throw new NotImplementedException($"No domain service implementation for type {typeof(T).Name}")
        };
    }
}