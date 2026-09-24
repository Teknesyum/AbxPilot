using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AbxPilot.Core.Knowledge;

namespace AbxPilot.Data;

public static class KbResources
{
    private const string KbPrefix = "AbxPilot.Data.Kb.";
    private const string I18nPrefix = "AbxPilot.Data.I18n.";

    private static readonly Lazy<KnowledgeBase> Cached = new(Load);

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

    public static KnowledgeBase Knowledge() => Cached.Value;

    public static KnowledgeBase Read(Stream stream) =>
        JsonSerializer.Deserialize(stream, KbJsonContext.Default.KnowledgeBase)
        ?? throw new InvalidOperationException("kb.json is empty");

    private static KnowledgeBase Load()
    {
        using var stream = Assembly.GetManifestResourceStream(KbPrefix + "kb.json")
                           ?? throw new InvalidOperationException("kb.json is not embedded");
        return Read(stream);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(KnowledgeBase))]
internal sealed partial class KbJsonContext : JsonSerializerContext;
