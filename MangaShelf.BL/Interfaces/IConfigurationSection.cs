namespace MangaShelf.BL.Interfaces;

public interface IConfigurationSection
{
    public string Name
    {
        get
        {
            return GetType().Name.Replace("Settings", string.Empty);
        }
    }
}
