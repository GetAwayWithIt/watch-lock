using System.IO;
using System.Text.Json;

namespace WatchLock;

/// <summary>Where Watch Lock keeps its files: %APPDATA%\WatchLock</summary>
public static class AppPaths
{
    public static readonly string Folder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WatchLock");

    /// <summary>Folder used by v0.2 and earlier, when the app was called "Ekran Kilidi".</summary>
    public static readonly string OldFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EkranKilidi");
}

/// <summary>Small user preferences: %APPDATA%\WatchLock\settings.json</summary>
public sealed class Settings
{
    public bool TestMode { get; set; } = true;
    public bool TrayTipShown { get; set; }
    public string Language { get; set; } = "en";

    private static readonly string FilePath = Path.Combine(AppPaths.Folder, "settings.json");

    public static Settings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new Settings();
        }
        catch { /* corrupt file: fall back to defaults */ }
        return new Settings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(AppPaths.Folder);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
        }
        catch { /* losing a preference is not critical */ }
    }
}
