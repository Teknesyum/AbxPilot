using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbxPilot.Data;

public static class KbResources
{
    private const string KbPrefix = "AbxPilot.Data.Kb.";
    private const string I18nPrefix = "AbxPilot.Data.I18n.";

    private static Assembly Assembly => typeof(KbResources).Assembly;

    public static IReadOnlyList<string> Languages { get; } = Assembly
        .GetManifestResourceNames()
        .Where(name => name.StartsWith(I18nPrefix, StringComparison.Ordinal))
        .Select(name => name[I18nPrefix.Length..^".json".Length])
        .OrderBy(name => name, StringComparer.Ordinal)
        .ToArray();

    public static IReadOnlyDictionary<string, string> Strings(string language)
    {
        using var stream = Assembly.GetManifestResourceStream(I18nPrefix + language + ".json");
        if (stream is null) return new Dictionary<string, string>();
        return JsonSerializer.Deserialize(stream, KbJsonContext.Default.DictionaryStringString)
               ?? new Dictionary<string, string>();
    }

    public static KbManifest Manifest()
    {
        using var stream = Assembly.GetManifestResourceStream(KbPrefix + "kb.json")
                           ?? throw new InvalidOperationException("kb.json is not embedded");
        return JsonSerializer.Deserialize(stream, KbJsonContext.Default.KbManifest)
               ?? throw new InvalidOperationException("kb.json is empty");
    }
}

public sealed record KbManifest(
    [property: JsonPropertyName("schema")] int Schema,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("syndromes")] IReadOnlyList<string> Syndromes);

[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(KbManifest))]
internal sealed partial class KbJsonContext : JsonSerializerContext;
