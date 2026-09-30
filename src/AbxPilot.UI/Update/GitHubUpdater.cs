using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using AbxPilot.UI.Kabuk;

namespace AbxPilot.UI.Update;

public sealed class GitHubUpdater
{
    private const string Repository = "Teknesyum/AbxPilot";
    private const string Installer = "kur-abxpilot.ps1";

    private static readonly HttpClient Http = CreateClient();

    public static string CurrentVersion { get; } = ReadCurrentVersion();

    public static string Label => "v" + CurrentVersion;

    public async Task<SurumSonucu> CheckAsync()
    {
        try
        {
            using var response = await Http.GetAsync($"https://api.github.com/repos/{Repository}/releases?per_page=20");
            if (!response.IsSuccessStatusCode)
                return new SurumSonucu(SurumDurumu.Hata, Mesaj: ((int)response.StatusCode).ToString());
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            foreach (var release in document.RootElement.EnumerateArray())
            {
                if (release.GetProperty("draft").GetBoolean()) continue;
                var tag = release.GetProperty("tag_name").GetString() ?? "";
                var notes = release.TryGetProperty("body", out var body) ? body.GetString() ?? "" : "";
                return IsNewer(tag, CurrentVersion)
                    ? new SurumSonucu(SurumDurumu.Var, tag, notes)
                    : new SurumSonucu(SurumDurumu.Guncel, tag);
            }
            return new SurumSonucu(SurumDurumu.Guncel);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException or KeyNotFoundException)
        {
            return new SurumSonucu(SurumDurumu.Hata, Mesaj: exception.Message);
        }
    }

    public async Task InstallAsync(SurumSonucu result)
    {
        if (!OperatingSystem.IsWindows())
        {
            Open($"https://github.com/{Repository}/releases/tag/{result.Surum}");
            return;
        }
        var url = $"https://github.com/{Repository}/releases/download/{result.Surum}/{Installer}";
        var bytes = await Http.GetByteArrayAsync(url);
        var folder = Path.Combine(Path.GetTempPath(), "AbxPilot-guncelleme");
        Directory.CreateDirectory(folder);
        var script = Path.Combine(folder, Installer);
        await File.WriteAllBytesAsync(script, bytes);
        var start = new ProcessStartInfo("powershell.exe") { UseShellExecute = true };
        foreach (var argument in new[] { "-NoProfile", "-STA", "-ExecutionPolicy", "Bypass", "-File", script, "-Surum", result.Surum })
            start.ArgumentList.Add(argument);
        Process.Start(start);
    }

    public static bool IsNewer(string tag, string current) =>
        Parse(tag) is { } latest && Parse(current) is { } mine && latest > mine;

    private static Version? Parse(string text)
    {
        var core = text.Trim().TrimStart('v', 'V');
        var cut = core.IndexOfAny(['-', '+']);
        if (cut >= 0) core = core[..cut];
        return Version.TryParse(core, out var version) ? version : null;
    }

    private static string ReadCurrentVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? typeof(GitHubUpdater).Assembly;
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plus = informational.IndexOf('+');
            return plus >= 0 ? informational[..plus] : informational;
        }
        return assembly.GetName().Version?.ToString(3) ?? "0.0.0";
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("AbxPilot-Guncelleme");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return client;
    }

    private static void Open(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
        }
    }
}
