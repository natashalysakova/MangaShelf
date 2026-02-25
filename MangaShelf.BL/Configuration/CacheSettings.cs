using MangaShelf.BL.Interfaces;

namespace MangaShelf.BL.Configuration;

public class CacheSettings : IConfigurationSection
{
    public bool Enabled { get; set; }
    public TimeSpan AbsoluteExpiration { get; set; }
    public TimeSpan UpdateInterval { get; set; }
}
