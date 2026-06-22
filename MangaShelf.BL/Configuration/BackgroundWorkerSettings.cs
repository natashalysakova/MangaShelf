using MangaShelf.BL.Contracts;

namespace MangaShelf.BL.Configuration;

public class BackgroundWorkerSettings : IConfigurationSection
{
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
