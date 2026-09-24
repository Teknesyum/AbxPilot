using System.Diagnostics;
using System.Text;
using AbxPilot.Core;
using AbxPilot.Core.Engine;
using AbxPilot.Data;

namespace AbxPilot.Tests.Golden;

public sealed class GoldenTests
{
    private const int MinimumCases = 15;
    private static readonly TimeSpan Budget = TimeSpan.FromMilliseconds(50);
    private static readonly GuidelineEngine Engine = new(KbResources.Knowledge());
    private static readonly IReadOnlyDictionary<string, GoldenCase> Cases =
        GoldenCase.All().ToDictionary(item => item.Id, StringComparer.Ordinal);

    public static IEnumerable<object[]> Runs() => GoldenCase.Runs();

    [Fact]
    public void EnoughCasesWithSources()
    {
        Assert.True(Cases.Count >= MinimumCases, $"{Cases.Count} golden cases");
        Assert.All(Cases.Values, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Source), item.Id);
            Assert.False(string.IsNullOrWhiteSpace(item.Section), item.Id);
            Assert.False(string.IsNullOrWhiteSpace(item.Expect.Status), item.Id);
        });
        Assert.Equal(Cases.Count, Directory.GetFiles(GoldenCase.Folder, "*.yaml").Length);
    }

    [Theory]
    [MemberData(nameof(Runs))]
    public void CaseMatches(string id, string set)
    {
        var item = Cases[id];
        var expect = item.ExpectFor(set);
        var result = Engine.Evaluate(item.Context(set));

        Assert.Equal(expect.Status, Status(result.Status));
        Assert.Equal(expect.FirstChoice, result.FirstChoice?.RegimenId);
        if (expect.Drugs is { } drugs)
            Assert.Equal(drugs.Order(StringComparer.Ordinal), result.FirstChoice!.DrugIds.Order(StringComparer.Ordinal));
        Assert.Equal(
            (expect.Excluded ?? []).Order(StringComparer.Ordinal),
            result.Excluded.Select(excluded => excluded.RegimenId).Order(StringComparer.Ordinal));
    }

    [Theory]
    [MemberData(nameof(Runs))]
    public void CaseIsFast(string id, string set)
    {
        var context = Cases[id].Context(set);
        var watch = Stopwatch.StartNew();
        Engine.Evaluate(context);
        watch.Stop();
        Assert.True(watch.Elapsed < Budget, $"{id}/{set}: {watch.Elapsed.TotalMilliseconds:F2} ms");
    }

    [Fact]
    public void ReportIsWritten()
    {
        Engine.Evaluate(Cases.Values.First().Context(GoldenCase.Sets[0]));
        var text = new StringBuilder();
        text.AppendLine("# Golden Report");
        text.AppendLine();
        text.AppendLine($"kb {Engine.Knowledge.Version}, generated {DateTime.Now:yyyy-MM-dd HH:mm}");
        text.AppendLine();
        text.AppendLine("| Case | Set | Status | First choice | Drugs | Excluded | ms |");
        text.AppendLine("|---|---|---|---|---|---|---|");
        var slowest = (Id: "", Ms: 0.0);
        foreach (var item in Cases.Values.OrderBy(item => item.Id, StringComparer.Ordinal))
        foreach (var set in GoldenCase.Sets)
        {
            var watch = Stopwatch.StartNew();
            var result = Engine.Evaluate(item.Context(set));
            var ms = watch.Elapsed.TotalMilliseconds;
            if (ms > slowest.Ms) slowest = ($"{item.Id}/{set}", ms);
            var excluded = string.Join(", ", result.Excluded.Select(line => $"{line.RegimenId}({line.RuleId})"));
            text.AppendLine(
                $"| {item.Id} | {set} | {Status(result.Status)} | {result.FirstChoice?.RegimenId ?? "-"} | {result.FirstChoice?.Key ?? "-"} | {excluded} | {ms:F2} |");
        }

        text.AppendLine();
        text.AppendLine($"Slowest: {slowest.Id} {slowest.Ms:F2} ms");
        var path = Path.Combine(RepoPaths.Root, "tmp", "golden-report.md");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text.ToString());
        Assert.True(File.Exists(path));
    }

    private static string Status(RecommendationStatus status) => status switch
    {
        RecommendationStatus.Selected => "selected",
        RecommendationStatus.NoGuidelineRow => "no_row",
        _ => "no_candidate"
    };
}
