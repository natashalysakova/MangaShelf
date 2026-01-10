//using MangaShelf.DAL.DomainServices;
//using MangaShelf.DAL.System.DomainServices;
//using MangaShelf.DAL.System.Interfaces;

//namespace MangaShelf.DAL.Interfaces;

//public class SystemDomainServiceFactory
//{
//    private readonly MangaSystemDbContext _systemContext;

//    public SystemDomainServiceFactory(MangaSystemDbContext context)
//    {
//        _systemContext = context;
//    }

//    public T GetDomainService<T>() where T : ISystemDomainService
//    {
//        return typeof(T) switch
//        {
//            var t when t == typeof(IParsedStatusDomainService) => (T)(object)new ParserStatusDomainService(_systemContext),
//            _ => throw new NotImplementedException($"No domain service implementation for type {typeof(T).Name}")
//        };
//    }
//}