using MangaShelf.DAL.Interfaces;

namespace MangaShelf.DAL.System.Models;

public class Settings : IEntity
{
    public Guid Id { get; set; }
    public required string Section { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }

    public SettingType Type { get; set; }
}

public enum SettingType
{
    Unknown = 0,
    String = 1,
    TimeSpan = 2,
    Int = 3,
    Bool = 4,
    DateTime = 5,
    Decimal = 6
}