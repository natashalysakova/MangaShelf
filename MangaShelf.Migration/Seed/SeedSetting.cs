using MangaShelf.DAL.System.Models;

namespace MangaShelf.Infrastructure.Seed;

public record SeedSetting(string Section, string Key, string Value, SettingType type);
