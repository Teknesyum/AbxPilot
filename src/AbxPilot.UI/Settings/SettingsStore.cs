using System.Text.Json;

namespace AbxPilot.UI.Settings;

public sealed record AppSettings
{
    public bool ShowScore { get; init; }
    public bool ConfirmUpdate { get; init; }
    public bool TipSeen { get; init; }
    public string? Language { get; init; }
    public string? Region { get; init; }
    public string? LastSyndrome { get; init; }
    public IReadOnlyDictionary<string, string>? GuidelineSets { get; init; }
}

public interface ISettingsStore
{
    AppSettings Load();

    void Save(AppSettings settings);
}

public sealed class FileSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _path;

    public FileSettingsStore(string path) => _path = path;

    public string Path => _path;

    public static FileSettingsStore ForUser() =>
        new(System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AbxPilot",
            "settings.json"));

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_path)) return new AppSettings();
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path)) ?? new AppSettings();
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(settings, Options));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
        }
    }
}

public sealed class MemorySettingsStore : ISettingsStore
{
    public AppSettings Current { get; private set; }

    public MemorySettingsStore(AppSettings? initial = null) => Current = initial ?? new AppSettings();

    public AppSettings Load() => Current;

    public void Save(AppSettings settings) => Current = settings;
}
