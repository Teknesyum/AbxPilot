using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AbxPilot.Core.Knowledge;

namespace AbxPilot.KbCompiler;

public sealed record CompileResult(
    IReadOnlyList<Diagnostic> Diagnostics,
    KnowledgeBase? Knowledge,
    string? KbJson,
    IReadOnlyDictionary<string, string> StringTables)
{
    public bool Success => Knowledge is not null && Diagnostics.All(item => item.Severity != Severity.Error);

    public IEnumerable<Diagnostic> Errors => Diagnostics.Where(item => item.Severity == Severity.Error);

    public IEnumerable<Diagnostic> Warnings => Diagnostics.Where(item => item.Severity == Severity.Warning);
}

public sealed record Mapped<T>(T Model, SourceRecord Source);

public static class KbCompilation
{
    public const string ReferenceLanguage = "tr";
    public const string SecondLanguage = "en";

    public static readonly JsonSerializerOptions ModelOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly JsonSerializerOptions StringTableOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly HashSet<string> RowColumns = new(StringComparer.Ordinal)
    {
        "id", "version", "stage", "action", "role", "candidates", "duration_days", "rationale_key",
        "source", "section", "source_date", "reviewed_at", "review_status", "note"
    };

    private static readonly HashSet<string> SpectrumColumns = new(StringComparer.Ordinal)
    {
        "drug", "version", "source", "section", "source_date", "reviewed_at", "review_status", "note"
    };

    public static CompileResult Compile(string kbDirectory)
    {
        var bag = new DiagnosticBag();
        var root = Path.GetFullPath(kbDirectory);
        var schemas = SchemaSet.Load(Path.Combine(root, "schema"), bag);
        if (schemas is null) return new CompileResult(bag.Items, null, null, new Dictionary<string, string>());

        var manifest = LoadSingle<KbManifestSource>(root, "kb.yaml", "kb", schemas, bag);
        var sources = LoadMany<KbSource>(Yaml(root, "sources.yaml"), "source", schemas, bag);
        var drugs = LoadMany<Drug>(Yaml(root, "drugs"), "drug", schemas, bag);
        var pathogens = LoadMany<Pathogen>(Yaml(root, "pathogens"), "pathogen", schemas, bag);
        var regimens = LoadMany<Regimen>(Yaml(root, "regimens"), "regimen", schemas, bag);
        var questions = LoadMany<Question>(Yaml(root, "questions"), "question", schemas, bag);
        var syndromes = LoadMany<Syndrome>(Yaml(root, "syndromes"), "syndrome", schemas, bag);
        var regions = LoadMany<Region>(Yaml(root, "regions"), "region", schemas, bag);
        var scoring = LoadSingle<Scoring>(root, "scoring.yaml", "scoring", schemas, bag);
        var constraints = LoadMany<Constraint>(Yaml(root, "constraints.yaml"), "constraint", schemas, bag);
        var (sets, rows) = LoadGuidelines(root, schemas, bag);
        var spectrum = LoadSpectrum(root, schemas, bag);
        var strings = LoadStrings(root, schemas, bag);

        var checker = new CrossReferences(
            sources, drugs, pathogens, regimens, questions, syndromes, sets, rows, spectrum, regions, scoring, constraints, strings, bag);
        if (!bag.HasErrors) checker.Run();
        var resolved = bag.HasErrors ? [] : Resolve(sets, rows);
        if (!bag.HasErrors) CheckCoverage(resolved, sets, syndromes, questions, bag);

        if (bag.HasErrors || manifest is null || scoring is null)
            return new CompileResult(bag.Items, null, null, new Dictionary<string, string>());

        var knowledge = new KnowledgeBase
        {
            Schema = manifest.Model.Schema,
            Version = manifest.Model.Version,
            Sources = Sorted(sources, item => item.Id),
            Drugs = Sorted(drugs, item => item.Id),
            Pathogens = Sorted(pathogens, item => item.Id),
            Regimens = Sorted(regimens, item => item.Id),
            Questions = Sorted(questions, item => item.Id),
            Syndromes = syndromes.Select(item => item.Model)
                .OrderBy(item => item.Order ?? int.MaxValue)
                .ThenBy(item => item.Id, StringComparer.Ordinal)
                .ToArray(),
            GuidelineSets = Sorted(sets, item => item.Id),
            GuidelineRows = resolved
                .OrderBy(item => item.Set, StringComparer.Ordinal)
                .ThenBy(item => item.Syndrome, StringComparer.Ordinal)
                .ThenBy(item => item.Order)
                .ToArray(),
            Spectrum = Sorted(spectrum, item => item.Id),
            Regions = Sorted(regions, item => item.Id),
            Scoring = scoring.Model,
            Constraints = constraints.Select(item => item.Model).ToArray()
        };

        var tables = strings.ToDictionary(
            pair => pair.Key,
            pair => JsonSerializer.Serialize(
                new SortedDictionary<string, string>(
                    pair.Value.ToDictionary(entry => entry.Key, entry => entry.Value.Text), StringComparer.Ordinal),
                StringTableOptions),
            StringComparer.Ordinal);

        return new CompileResult(bag.Items, knowledge, JsonSerializer.Serialize(knowledge, ModelOptions), tables);
    }

    public static int Run(string kbDirectory, string outDirectory, TextWriter output, TextWriter error)
    {
        var result = Compile(kbDirectory);
        foreach (var item in result.Diagnostics) error.WriteLine(item.ToString());

        if (!result.Success || result.KbJson is null || result.Knowledge is null)
        {
            error.WriteLine($"kb: {result.Errors.Count()} error(s), nothing written");
            return 1;
        }

        Directory.CreateDirectory(outDirectory);
        var i18nDirectory = Path.Combine(outDirectory, "i18n");
        Directory.CreateDirectory(i18nDirectory);
        WriteIfChanged(Path.Combine(outDirectory, "kb.json"), result.KbJson);
        foreach (var stale in Directory.GetFiles(i18nDirectory, "*.json"))
            if (!result.StringTables.ContainsKey(Path.GetFileNameWithoutExtension(stale)))
                File.Delete(stale);
        foreach (var (language, json) in result.StringTables)
            WriteIfChanged(Path.Combine(i18nDirectory, language + ".json"), json);

        var kb = result.Knowledge;
        output.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"kb {kb.Version}: {kb.Drugs.Count} drugs, {kb.Pathogens.Count} pathogens, {kb.Regimens.Count} regimens, " +
            $"{kb.Questions.Count} questions, {kb.Syndromes.Count} syndromes, {kb.GuidelineRows.Count} table rows, " +
            $"{kb.Regions.Count} regions, {result.StringTables.Count} languages, {result.Warnings.Count()} warning(s)"));
        return 0;
    }

    private static List<GuidelineRow> Resolve(List<Mapped<GuidelineSet>> sets, List<Mapped<GuidelineRow>> rows)
    {
        var memo = new Dictionary<string, List<GuidelineRow>>(StringComparer.Ordinal);

        List<GuidelineRow> Effective(GuidelineSet set, string syndrome)
        {
            var key = set.Id + "/" + syndrome;
            if (memo.TryGetValue(key, out var cached)) return cached;
            var parent = set.Extends is { } parentId ? sets.FirstOrDefault(item => item.Model.Id == parentId)?.Model : null;
            var table = parent is null
                ? []
                : Effective(parent, syndrome).Select(row => row with { Set = set.Id }).ToList();
            foreach (var own in rows.Where(item => item.Model.Set == set.Id && item.Model.Syndrome == syndrome).Select(item => item.Model))
            {
                var index = table.FindIndex(row => row.Id == own.Id);
                if (index >= 0) table[index] = own;
                else table.Add(own);
            }

            var ordered = table.Select((row, index) => row with { Order = index + 1 }).ToList();
            memo[key] = ordered;
            return ordered;
        }

        var syndromes = rows.Select(item => item.Model.Syndrome).Distinct(StringComparer.Ordinal).ToArray();
        return sets.SelectMany(set => syndromes.SelectMany(syndrome => Effective(set.Model, syndrome))).ToList();
    }

    private static void CheckCoverage(List<GuidelineRow> rows, List<Mapped<GuidelineSet>> sets, List<Mapped<Syndrome>> syndromes,
        List<Mapped<Question>> questions, DiagnosticBag bag)
    {
        foreach (var group in rows.Where(row => row.Stage == "base").GroupBy(row => (row.Set, row.Syndrome)))
        {
            var syndrome = syndromes.FirstOrDefault(item => item.Model.Id == group.Key.Syndrome)?.Model;
            var set = sets.First(item => item.Model.Id == group.Key.Set).Source;
            if (syndrome is null) continue;

            var columns = group.SelectMany(row => row.Conditions.Keys).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            var domains = columns.Select(column =>
                questions.FirstOrDefault(item => item.Model.Id == column)?.Model.Options ?? (IReadOnlyList<string>)["yes", "no"]).ToArray();
            var total = domains.Aggregate(1L, (product, domain) => product * domain.Count);
            if (total > 100_000) continue;

            var gaps = new List<string>();
            var cursor = new int[columns.Length];
            for (var n = 0; n < total; n++)
            {
                var index = n;
                for (var c = columns.Length - 1; c >= 0; c--)
                {
                    cursor[c] = (int)(index % domains[c].Count);
                    index /= domains[c].Count;
                }

                var covered = group.Any(row => row.Conditions.All(condition =>
                    condition.Value.Contains(domains[Array.IndexOf(columns, condition.Key)][cursor[Array.IndexOf(columns, condition.Key)]])));
                if (!covered)
                    gaps.Add(string.Join(", ", columns.Select((column, c) => $"{column}={domains[c][cursor[c]]}")));
            }

            foreach (var gap in gaps.Take(3))
                bag.Warning(Codes.Coverage, set.File, set.Line, $"table {group.Key.Set}/{group.Key.Syndrome}: no base row matches {gap}");
            if (gaps.Count > 3)
                bag.Warning(Codes.Coverage, set.File, set.Line, $"table {group.Key.Set}/{group.Key.Syndrome}: {gaps.Count - 3} more uncovered combinations");
        }
    }

    private static void WriteIfChanged(string path, string content)
    {
        if (File.Exists(path) && File.ReadAllText(path) == content) return;
        File.WriteAllText(path, content);
    }

    private static IReadOnlyList<T> Sorted<T>(IEnumerable<Mapped<T>> items, Func<T, string> key) =>
        items.Select(item => item.Model).OrderBy(key, StringComparer.Ordinal).ToArray();

    private static IEnumerable<string> Yaml(string root, string relative)
    {
        var path = Path.Combine(root, relative);
        if (File.Exists(path)) return [path];
        if (!Directory.Exists(path)) return [];
        return Directory.GetFiles(path, "*.yaml", SearchOption.TopDirectoryOnly).OrderBy(file => file, StringComparer.Ordinal);
    }

    private static Mapped<T>? LoadSingle<T>(string root, string file, string schema, SchemaSet schemas, DiagnosticBag bag)
    {
        var path = Path.Combine(root, file);
        if (!File.Exists(path))
        {
            bag.Error(Codes.Layout, path, 1, $"{file} is missing");
            return default;
        }

        var items = LoadMany<T>([path], schema, schemas, bag);
        if (items.Count == 1) return items[0];
        if (items.Count > 1) bag.Error(Codes.Layout, path, 1, $"{file} must hold exactly one record");
        return default;
    }

    private static List<Mapped<T>> LoadMany<T>(IEnumerable<string> files, string schema, SchemaSet schemas, DiagnosticBag bag)
    {
        var result = new List<Mapped<T>>();
        foreach (var file in files)
        foreach (var record in YamlLoader.Load(file, bag))
        {
            if (!schemas.Validate(schema, record, bag)) continue;
            var model = Map<T>(record, bag);
            if (model is not null) result.Add(new Mapped<T>(model, record));
        }

        return result;
    }

    internal static T? Map<T>(SourceRecord record, DiagnosticBag bag)
    {
        try
        {
            return record.Node.Deserialize<T>(ModelOptions);
        }
        catch (JsonException ex)
        {
            var pointer = PathToPointer(ex.Path);
            bag.Error(Codes.Model, record.File, record.LineOf(pointer), $"{pointer}: cannot map to {typeof(T).Name}: {ex.Message}");
            return default;
        }
    }

    private static string PathToPointer(string? path)
    {
        if (string.IsNullOrEmpty(path) || path == "$") return "";
        var text = path.StartsWith('$') ? path[1..] : path;
        text = text.Replace("['", ".").Replace("']", "").Replace("[", ".").Replace("]", "");
        return string.Concat(text.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(part => "/" + YamlLoader.Escape(part)));
    }

    private static (List<Mapped<GuidelineSet>> Sets, List<Mapped<GuidelineRow>> Rows) LoadGuidelines(
        string root, SchemaSet schemas, DiagnosticBag bag)
    {
        var sets = new List<Mapped<GuidelineSet>>();
        var rows = new List<Mapped<GuidelineRow>>();
        var directory = Path.Combine(root, "guidelines");
        if (!Directory.Exists(directory)) return (sets, rows);

        foreach (var setDirectory in Directory.GetDirectories(directory).OrderBy(path => path, StringComparer.Ordinal))
        {
            var setId = Path.GetFileName(setDirectory);
            var setFile = Path.Combine(setDirectory, "set.yaml");
            if (!File.Exists(setFile))
            {
                bag.Error(Codes.Layout, setDirectory, 1, "guideline set has no set.yaml");
                continue;
            }

            foreach (var set in LoadMany<GuidelineSet>([setFile], "guideline-set", schemas, bag))
            {
                if (set.Model.Id != setId)
                    bag.Error(Codes.Layout, setFile, set.Source.Line, $"set id '{set.Model.Id}' must match its folder '{setId}'");
                sets.Add(set);
            }

            foreach (var csv in Directory.GetFiles(setDirectory, "*.csv").OrderBy(path => path, StringComparer.Ordinal))
            {
                var syndrome = Path.GetFileNameWithoutExtension(csv);
                var (_, csvRows) = CsvLoader.Load(csv, bag);
                var order = 0;
                foreach (var row in csvRows)
                {
                    var node = new JsonObject
                    {
                        ["set"] = setId,
                        ["syndrome"] = syndrome,
                        ["order"] = ++order
                    };
                    var conditions = new JsonObject();
                    foreach (var (column, cell) in row.Cells)
                    {
                        if (!RowColumns.Contains(column))
                        {
                            if (cell.Length > 0 && cell != "*") conditions[column] = List(cell, '|');
                            continue;
                        }

                        switch (column)
                        {
                            case "candidates":
                                node[column] = Tiers(cell);
                                break;
                            case "role":
                                if (cell.Length > 0) node["roles"] = List(cell, '|');
                                break;
                            case "version" or "duration_days":
                                if (cell.Length > 0) node[column] = Number(cell);
                                break;
                            case "reviewed_at":
                                node[column] = cell.Length == 0 ? null : JsonValue.Create(cell);
                                break;
                            default:
                                if (cell.Length > 0) node[column] = cell;
                                break;
                        }
                    }

                    node["conditions"] = conditions;
                    var record = new SourceRecord(node, csv, row.Line, new Dictionary<string, int> { [""] = row.Line });
                    if (!schemas.Validate("guideline-row", record, bag)) continue;
                    var model = Map<GuidelineRow>(record, bag);
                    if (model is not null) rows.Add(new Mapped<GuidelineRow>(model, record));
                }
            }
        }

        return (sets, rows);
    }

    private static List<Mapped<SpectrumEntry>> LoadSpectrum(string root, SchemaSet schemas, DiagnosticBag bag)
    {
        var result = new List<Mapped<SpectrumEntry>>();
        var directory = Path.Combine(root, "spectrum");
        if (!Directory.Exists(directory)) return result;

        foreach (var csv in Directory.GetFiles(directory, "*.csv").OrderBy(path => path, StringComparer.Ordinal))
        {
            var syndrome = Path.GetFileNameWithoutExtension(csv);
            var (_, csvRows) = CsvLoader.Load(csv, bag);
            foreach (var row in csvRows)
            {
                var node = new JsonObject { ["syndrome"] = syndrome };
                var coverage = new JsonObject();
                foreach (var (column, cell) in row.Cells)
                {
                    if (!SpectrumColumns.Contains(column))
                    {
                        coverage[column] = cell;
                        continue;
                    }

                    switch (column)
                    {
                        case "version":
                            node[column] = Number(cell);
                            break;
                        case "reviewed_at":
                            node[column] = cell.Length == 0 ? null : JsonValue.Create(cell);
                            break;
                        default:
                            if (cell.Length > 0) node[column] = cell;
                            break;
                    }
                }

                if (node["drug"] is JsonValue drug) node["id"] = syndrome + "." + drug.GetValue<string>();
                node["coverage"] = coverage;
                var record = new SourceRecord(node, csv, row.Line, new Dictionary<string, int> { [""] = row.Line });
                if (!schemas.Validate("spectrum-row", record, bag)) continue;
                var model = Map<SpectrumEntry>(record, bag);
                if (model is not null) result.Add(new Mapped<SpectrumEntry>(model, record));
            }
        }

        return result;
    }

    private static Dictionary<string, Dictionary<string, StringEntry>> LoadStrings(string root, SchemaSet schemas, DiagnosticBag bag)
    {
        var result = new Dictionary<string, Dictionary<string, StringEntry>>(StringComparer.Ordinal);
        var directory = Path.Combine(root, "i18n");
        if (!Directory.Exists(directory))
        {
            bag.Error(Codes.Layout, directory, 1, "i18n directory is missing");
            return result;
        }

        foreach (var stray in Directory.GetFiles(directory, "*.json"))
            bag.Error(Codes.Layout, stray, 1, "string tables live in i18n/<lang>/*.json");

        foreach (var languageDirectory in Directory.GetDirectories(directory).OrderBy(path => path, StringComparer.Ordinal))
        {
            var language = Path.GetFileName(languageDirectory);
            var table = new Dictionary<string, StringEntry>(StringComparer.Ordinal);
            result[language] = table;

            foreach (var file in Directory.GetFiles(languageDirectory, "*.json").OrderBy(path => path, StringComparer.Ordinal))
            {
                var text = File.ReadAllText(file);
                JsonObject? obj;
                try
                {
                    obj = JsonNode.Parse(text) as JsonObject;
                }
                catch (Exception ex) when (ex is JsonException or ArgumentException)
                {
                    var line = ex is JsonException json && json.LineNumber is { } number ? (int)number + 1 : 1;
                    bag.Error(Codes.Parse, file, line, ex.Message);
                    continue;
                }

                if (obj is null)
                {
                    bag.Error(Codes.Parse, file, 1, "a string table must be a JSON object");
                    continue;
                }

                var lines = LineIndex(text);
                var record = new SourceRecord(obj, file, 1, lines);
                if (!schemas.Validate("i18n", record, bag)) continue;

                foreach (var (key, value) in obj)
                {
                    var line = record.LineOf("/" + YamlLoader.Escape(key));
                    if (table.TryGetValue(key, out var existing))
                    {
                        bag.Error(Codes.DuplicateKey, file, line,
                            $"key '{key}' is already defined in {Path.GetFileName(existing.File)}:{existing.Line}");
                        continue;
                    }

                    table[key] = new StringEntry(value!.GetValue<string>(), file, line);
                }
            }
        }

        if (!result.ContainsKey(ReferenceLanguage))
            bag.Error(Codes.Layout, directory, 1, $"reference language '{ReferenceLanguage}' is missing");
        return result;
    }

    private static Dictionary<string, int> LineIndex(string text)
    {
        var lines = new Dictionary<string, int>(StringComparer.Ordinal) { [""] = 1 };
        var all = text.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < all.Length; i++)
        {
            var trimmed = all[i].TrimStart();
            if (!trimmed.StartsWith('"')) continue;
            var end = trimmed.IndexOf('"', 1);
            if (end <= 1) continue;
            var key = trimmed[1..end];
            lines.TryAdd("/" + YamlLoader.Escape(key), i + 1);
        }

        return lines;
    }

    private static JsonArray List(string cell, char separator) =>
        new(cell.Split(separator, StringSplitOptions.TrimEntries).Select(part => (JsonNode?)JsonValue.Create(part)).ToArray());

    private static JsonArray Tiers(string cell) =>
        cell.Length == 0
            ? []
            : new JsonArray(cell.Split(';', StringSplitOptions.TrimEntries).Select(tier => (JsonNode?)List(tier, '|')).ToArray());

    private static JsonNode? Number(string cell) =>
        int.TryParse(cell, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? JsonValue.Create(value)
            : cell.Length == 0 ? null : JsonValue.Create(cell);
}

public sealed record StringEntry(string Text, string File, int Line);

public sealed record KbManifestSource
{
    public int Schema { get; init; }
    public string Version { get; init; } = "";
}
