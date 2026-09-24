using AbxPilot.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AbxPilot.Tests.Golden;

public sealed class GoldenExpectation
{
    public string? Status { get; set; }
    public string? FirstChoice { get; set; }
    public List<string>? Drugs { get; set; }
    public List<string>? Excluded { get; set; }

    public GoldenExpectation Over(GoldenExpectation? other) => other is null
        ? this
        : new GoldenExpectation
        {
            Status = other.Status ?? Status,
            FirstChoice = other.FirstChoice ?? FirstChoice,
            Drugs = other.Drugs ?? Drugs,
            Excluded = other.Excluded ?? Excluded
        };
}

public sealed class GoldenCase
{
    public const string Syndrome = "cap";
    public static readonly string[] Sets = ["idsa-ats-2019", "ttd-2021"];

    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Source { get; set; } = "";
    public string Section { get; set; } = "";
    public string Region { get; set; } = "tr";
    public Dictionary<string, object> Answers { get; set; } = [];
    public GoldenExpectation Expect { get; set; } = new();
    public Dictionary<string, GoldenExpectation> ExpectBySet { get; set; } = [];

    public static string Folder => Path.Combine(RepoPaths.Root, "tests", "AbxPilot.Tests", "Golden", Syndrome);

    private static readonly IDeserializer Reader = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    public static IReadOnlyList<GoldenCase> All() =>
        Directory.GetFiles(Folder, "*.yaml")
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => Reader.Deserialize<GoldenCase>(File.ReadAllText(path)))
            .ToArray();

    public static IEnumerable<object[]> Runs() =>
        All().SelectMany(item => Sets.Select(set => new object[] { item.Id, set }));

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
