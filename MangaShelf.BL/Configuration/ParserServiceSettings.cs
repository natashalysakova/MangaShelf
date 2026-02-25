using MangaShelf.BL.Interfaces;

namespace MangaShelf.BL.Configuration;

public class ParserServiceSettings : IConfigurationSection
{
    public TimeSpan DelayBetweenParse { get; set; }
    public bool IgnoreExistingVolumes { get; set; }
}
