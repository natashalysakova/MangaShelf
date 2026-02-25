using MangaShelf.BL.Interfaces;
using MangaShelf.DAL.System.Models;
using IConfigurationSection = MangaShelf.BL.Interfaces.IConfigurationSection;

namespace MangaShelf.Infrastructure.Seed;

public class SettingBuilder
{
    IEnumerable<SeedSetting> _settings;

    public SettingBuilder()
    {
        _settings = new List<SeedSetting>();
    }

    public SettingBuilder Add<T>(string key, string value) where T : IConfigurationSection, new()
    {
        return Add<T>(key, value.ToString(), SettingType.String);
    }

    public SettingBuilder Add<T>(string key, int value) where T : IConfigurationSection, new()
    {
        return Add<T>(key, value.ToString(), SettingType.Int);
    }

    public SettingBuilder Add<T>(string key, decimal value) where T : IConfigurationSection, new()
    {
        return Add<T>(key, value.ToString(), SettingType.Decimal);
    }

    public SettingBuilder Add<T>(string key, bool value) where T : IConfigurationSection, new()
    {
        return Add<T>(key, value.ToString(), SettingType.Bool);
    }

    public SettingBuilder Add<T>(string key, TimeSpan value) where T : IConfigurationSection, new()
    {
        return Add<T>(key, value.ToString(), SettingType.TimeSpan);
    }

    public SettingBuilder Add<T>(string key, DateTime value) where T : IConfigurationSection, new()
    {
        return Add<T>(key, value.ToString(), SettingType.DateTime);
    }

    public SettingBuilder Add<T>(string key, object value) where T : IConfigurationSection, new()
    {
        return Add<T>(key, value.ToString(), SettingType.Unknown);
    }

    private SettingBuilder Add<T>(string key, string value, SettingType type) where T : IConfigurationSection, new()
    {
        var section = new T().Name;
        _settings = _settings.Append(new SeedSetting(section, key, value, type));
        return this;
    }

    public IEnumerable<SeedSetting> Build()
    {
        return _settings;
    }
}
