using MangaShelf.BL.Interfaces;

namespace MangaShelf.BL.Configuration;

public class BackgroundWorkerSettings : IConfigurationSection
{
    public bool Enabled { get; set; }
    public TimeSpan StartDelay { get; set; }
    public TimeSpan LoopDelay { get; set; }
}

public static class ConfigurationExtention
{
    public static string GetName(this IConfigurationSection section)
    {
        return section.Name;
    }

}
