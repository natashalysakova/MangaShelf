namespace MangaShelf.BL.Exceptions;

[Serializable]
public class ConfigurationMissingException : Exception
{
    public ConfigurationMissingException(string message) : base(message) { }
    public ConfigurationMissingException(Guid id) : base($"Cannot find setting with Id {id}") { }
}
