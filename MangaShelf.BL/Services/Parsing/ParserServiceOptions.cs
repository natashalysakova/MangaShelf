namespace MangaShelf.BL.Services.Parsing;

public class ParserServiceOptions
{
    private int _delayBetweenParse; // in milliseconds

    /// <summary>
    /// Section name in appsettings.json
    /// </summary>
    public static string SectionName => "ParserService";



    /// <summary>
    /// Delay between parsing volumes in milliseconds. Set in seconds. When set, automatically converted to milliseconds.
    /// </summary>
    public int DelayBetweenParse
    {
        get => _delayBetweenParse;
        set
        {
            _delayBetweenParse = value * 1000;
        }
    }

    /// <summary>
    /// Existing volumes will be ignored if true (except for preorders)
    /// </summary>
    public bool IgnoreExistingVolumes { get; set; }
    
}
