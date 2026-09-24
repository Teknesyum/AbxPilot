using AbxPilot.Core;
using AbxPilot.Data;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AbxPilot.Tests.Golden;

public sealed class GoldenExpectation
{
    public string? Status { get; set; }
    public string? FirstChoice { get; set; }
    public List<string>? Drugs { get; set; }
    public List<string>? Excluded { get; set; }
    public List<List<string>>? NoLineWith { get; set; }

    public GoldenExpectation Over(GoldenExpectation? other) => other is null
        ? this
        : new GoldenExpectation
        {
            Status = other.Status ?? Status,
            FirstChoice = other.FirstChoice ?? FirstChoice,
            Drugs = other.Drugs ?? Drugs,
            Excluded = other.Excluded ?? Excluded,
            NoLineWith = other.NoLineWith ?? NoLineWith
        };
}

public sealed class GoldenCase
{
    public string Syndrome { get; set; } = "";
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Source { get; set; } = "";
    public string Section { get; set; } = "";
    public string Region { get; set; } = "tr";
    public Dictionary<string, object> Answers { get; set; } = [];
    public GoldenExpectation Expect { get; set; } = new();
    public Dictionary<string, GoldenExpectation> ExpectBySet { get; set; } = [];

    public string Key => $"{Syndrome}/{Id}";

    public static string Root => Path.Combine(RepoPaths.Root, "tests", "AbxPilot.Tests", "Golden");

    public static IReadOnlyList<string> Syndromes() =>
        Directory.GetDirectories(Root).Select(Path.GetFileName).Order(StringComparer.Ordinal).ToArray()!;

    public static IReadOnlyList<string> SetsFor(string syndrome) =>
        KbResources.Knowledge().GuidelineRows
            .Where(row => row.Syndrome == syndrome)
            .Select(row => row.Set)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static readonly IDeserializer Reader = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    public static IReadOnlyList<GoldenCase> All() =>
        Syndromes().SelectMany(syndrome =>
                Directory.GetFiles(Path.Combine(Root, syndrome), "*.yaml")
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .Select(path =>
                    {
                        var item = Reader.Deserialize<GoldenCase>(File.ReadAllText(path));
                        item.Syndrome = syndrome;
                        return item;
                    }))
            .ToArray();

    public static IEnumerable<object[]> Runs() =>
        All().SelectMany(item => SetsFor(item.Syndrome).Select(set => new object[] { item.Key, set }));

    public GoldenExpectation ExpectFor(string set) => Expect.Over(ExpectBySet.GetValueOrDefault(set));

    public GuidelineContext Context(string set) => new(
        Syndrome,
        Answers.ToDictionary(
            pair => pair.Key,
            pair => pair.Value switch
            {
                IEnumerable<object> list => (IReadOnlyList<string>)list.Select(item => item.ToString()!).ToArray(),
                null => [],
                var single => [single.ToString()!]
            },
            StringComparer.Ordinal),
        Region,
        set);
}
