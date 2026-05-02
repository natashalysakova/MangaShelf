using MangaShelf.BL.Contracts;

namespace MangaShelf.BL.Configuration;

public class CacheSettings : IConfigurationSection
{
    public bool Enabled { get; set; }
    public TimeSpan AbsoluteExpiration { get; set; }
    public TimeSpan UpdateInterval { get; set; }
}
