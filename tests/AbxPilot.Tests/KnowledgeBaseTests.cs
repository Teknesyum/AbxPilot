using AbxPilot.Data;
using AbxPilot.KbCompiler;

namespace AbxPilot.Tests;

public sealed class KnowledgeBaseTests
{
    private static readonly string RepoKb = Path.Combine(FindRepoRoot(), "kb");

    [Fact]
    public void CleanKnowledgeBaseCompiles()
    {
        var result = KbCompilation.Compile(RepoKb);

        Assert.Empty(result.Errors);
        Assert.True(result.Success);
        var kb = result.Knowledge!;
        Assert.True(kb.Drugs.Count >= 22);
        Assert.Equal(19, kb.Pathogens.Count);
        Assert.Equal("pharyngitis", kb.Syndromes[0].Id);
        Assert.InRange(kb.Questions.Count, 30, 60);
        Assert.Contains(kb.GuidelineSets, set => set.Id == "idsa-ats-2019");
        Assert.Contains(kb.GuidelineSets, set => set.Id == "ttd-2021");
        Assert.Contains("tr", result.StringTables.Keys);
        Assert.Contains("en", result.StringTables.Keys);
    }

    [Fact]
    public void MissingRequiredFieldFailsWithFileAndLine()
    {
        using var copy = new KbCopy();
        copy.Edit("drugs/amoxicillin.yaml", text => text.Replace("class: penicillin\n", ""));

        var errors = KbCompilation.Compile(copy.Root).Errors.ToArray();

        var error = Assert.Single(errors, item => item.Code == Codes.Schema);
        Assert.EndsWith("amoxicillin.yaml", error.File);
        Assert.Equal(1, error.Line);
        Assert.Contains("class", error.Message);
    }

    [Fact]
    public void UnknownRegimenInTableFails()
    {
        using var copy = new KbCopy();
        copy.Edit("guidelines/idsa-ats-2019/cap.csv", text => text.Replace("lt25,*,*,*,*,*,amx_po;", "lt25,*,*,*,*,*,missing_regimen;"));

        var errors = KbCompilation.Compile(copy.Root).Errors.ToArray();

        var error = Assert.Single(errors);
        Assert.Equal(Codes.UnknownReference, error.Code);
        Assert.EndsWith("cap.csv", error.File);
        Assert.Equal(2, error.Line);
        Assert.Contains("missing_regimen", error.Message);
    }

    [Fact]
    public void MissingTurkishKeyIsAnErrorAndMissingEnglishKeyIsAWarning()
    {
        using var copy = new KbCopy();
        copy.Edit("i18n/tr/drugs.json", text => RemoveLine(text, "drug.linezolid.name"));
        copy.Edit("i18n/en/drugs.json", text => RemoveLine(text, "drug.vancomycin.name"));

        var result = KbCompilation.Compile(copy.Root);

        var error = Assert.Single(result.Errors);
        Assert.Equal(Codes.MissingTr, error.Code);
        Assert.Contains("drug.linezolid.name", error.Message);
        Assert.EndsWith("linezolid.yaml", error.File);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal(Codes.MissingEn, warning.Code);
        Assert.Contains("drug.vancomycin.name", warning.Message);
    }

    [Fact]
    public void FailedCompilationExitsWithOneAndWritesNothing()
    {
        using var copy = new KbCopy();
        copy.Edit("drugs/amoxicillin.yaml", text => text.Replace("class: penicillin\n", ""));
        var output = Path.Combine(copy.Root, "..", "out");
        var error = new StringWriter();

        var code = KbCompilation.Run(copy.Root, output, TextWriter.Null, error);

        Assert.Equal(1, code);
        Assert.False(File.Exists(Path.Combine(output, "kb.json")));
        Assert.Contains("amoxicillin.yaml(1): error KB002", error.ToString());
    }

    [Fact]
    public void EmbeddedKnowledgeBaseMatchesTheSources()
    {
        var embedded = KbResources.Knowledge();
        var compiled = KbCompilation.Compile(RepoKb).Knowledge!;

        Assert.Equal(compiled.Version, embedded.Version);
        Assert.Equal(compiled.Drugs.Count, embedded.Drugs.Count);
        Assert.Equal(compiled.GuidelineRows.Count, embedded.GuidelineRows.Count);
        Assert.Equal(["cap", "iai", "pharyngitis", "ssti", "uti"], embedded.Syndromes.Select(item => item.Id).Order());

        var vancomycin = embedded.Drugs.Single(item => item.Id == "vancomycin").Doses.Single(dose => dose.Id == "cap_iv");
        Assert.Equal("15 mg/kg", vancomycin.Amount);
        Assert.False(string.IsNullOrEmpty(vancomycin.Amount70Kg));
        Assert.All(embedded.Drugs, drug => Assert.Null(drug.ReviewedAt));
        Assert.Contains(embedded.GuidelineRows, row => row.Set == "ttd-2021" && row.Id == "psa_fq");
        Assert.Equal("Türkiye", KbResources.Strings("tr")["region.tr.name"]);
    }

    private static string RemoveLine(string text, string key) =>
        string.Join('\n', text.Split('\n').Where(line => !line.Contains($"\"{key}\"", StringComparison.Ordinal)));

    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "AbxPilot.sln")))
                return dir.FullName;
        throw new DirectoryNotFoundException("AbxPilot.sln not found above the test output");
    }

    private sealed class KbCopy : IDisposable
    {
        private readonly string _base = Path.Combine(Path.GetTempPath(), "abxpilot-kb-" + Guid.NewGuid().ToString("N"));

        public KbCopy()
        {
            Root = Path.Combine(_base, "kb");
            foreach (var file in Directory.GetFiles(RepoKb, "*", SearchOption.AllDirectories))
            {
                var target = Path.Combine(Root, Path.GetRelativePath(RepoKb, file));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target);
            }
        }

        public string Root { get; }

        public void Edit(string relative, Func<string, string> change)
        {
            var path = Path.Combine(Root, relative);
            var before = File.ReadAllText(path).Replace("\r\n", "\n");
            var after = change(before);
            Assert.NotEqual(before, after);
            File.WriteAllText(path, after);
        }

        public void Dispose()
        {
            if (Directory.Exists(_base)) Directory.Delete(_base, true);
        }
    }
}
