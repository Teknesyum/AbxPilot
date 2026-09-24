using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace AbxPilot.KbCompiler;

public sealed class SchemaSet
{
    private static readonly JsonSerializerOptions Relaxed = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private readonly Dictionary<string, JsonSchema> _schemas;

    private SchemaSet(Dictionary<string, JsonSchema> schemas) => _schemas = schemas;

    public static SchemaSet? Load(string directory, DiagnosticBag bag)
    {
        if (!Directory.Exists(directory))
        {
            bag.Error(Codes.Layout, directory, 1, "schema directory is missing");
            return null;
        }

        var options = new BuildOptions { SchemaRegistry = new SchemaRegistry() };
        var schemas = new Dictionary<string, JsonSchema>(StringComparer.Ordinal);
        var files = Directory.GetFiles(directory, "*.schema.json")
            .OrderBy(path => Path.GetFileName(path) == "common.schema.json" ? 0 : 1)
            .ThenBy(path => path, StringComparer.Ordinal);

        foreach (var path in files)
        {
            try
            {
                var schema = JsonSchema.FromText(File.ReadAllText(path), options);
                schemas[Path.GetFileName(path)[..^".schema.json".Length]] = schema;
            }
            catch (Exception ex) when (ex is JsonException or ArgumentException or InvalidOperationException)
            {
                bag.Error(Codes.Parse, path, 1, "invalid schema: " + ex.Message);
            }
        }

        return bag.HasErrors ? null : new SchemaSet(schemas);
    }

    public bool Validate(string name, SourceRecord record, DiagnosticBag bag)
    {
        if (!_schemas.TryGetValue(name, out var schema))
        {
            bag.Error(Codes.Layout, record.File, record.Line, $"schema '{name}' is missing");
            return false;
        }

        var element = JsonSerializer.SerializeToElement(record.Node);
        var result = schema.Evaluate(element, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });
        if (result.IsValid) return true;

        var found = new List<(int Line, string Text, bool Unevaluated)>();
        Collect(result, record, found);
        var relevant = found.Any(item => !item.Unevaluated) ? found.Where(item => !item.Unevaluated) : found;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (line, text, _) in relevant)
            if (seen.Add(text)) bag.Error(Codes.Schema, record.File, line, text);

        if (seen.Count == 0)
            bag.Error(Codes.Schema, record.File, record.Line, $"record does not match {name}.schema.json");
        return false;
    }

    private static void Collect(EvaluationResults node, SourceRecord record, List<(int, string, bool)> found)
    {
        if (node.IsValid) return;
        var path = node.EvaluationPath.ToString();
        if (path.EndsWith("/if", StringComparison.Ordinal)) return;

        var pointer = node.InstanceLocation.ToString();
        var branches = node.Details?.Any(child =>
            child.EvaluationPath.ToString() is var childPath &&
            (childPath.StartsWith(path + "/oneOf/", StringComparison.Ordinal) || childPath.StartsWith(path + "/anyOf/", StringComparison.Ordinal))) ?? false;
        if (branches)
        {
            var value = Resolve(record.Node, pointer);
            var hint = value is null ? "" : $" (value {value.ToJsonString(Relaxed)})";
            found.Add((record.LineOf(pointer), $"{(pointer.Length == 0 ? "/" : pointer)}: matches none of the allowed forms{hint}", false));
            return;
        }

        if (node.Errors is not null)
            foreach (var (keyword, message) in node.Errors)
            {
                var text = Describe(pointer, keyword, message, record.Node, path.Contains("/propertyNames", StringComparison.Ordinal));
                found.Add((record.LineOf(pointer), text, text.EndsWith("property is not allowed here", StringComparison.Ordinal)));
            }

        if (node.Details is null) return;
        foreach (var child in node.Details) Collect(child, record, found);
    }

    private static string Describe(string pointer, string keyword, string message, JsonNode root, bool propertyName)
    {
        var where = pointer.Length == 0 ? "/" : pointer;
        if (message.Contains("false schema", StringComparison.Ordinal))
            return $"{where}: property is not allowed here";

        if (propertyName) return $"{where}: property name does not match the allowed pattern";
        var value = Resolve(root, pointer);
        var hint = keyword is "enum" or "const" or "pattern" or "type" && value is not null
            ? $" (value {value.ToJsonString(Relaxed)})"
            : "";
        return $"{where}: {message}{hint}";
    }

    private static JsonNode? Resolve(JsonNode root, string pointer)
    {
        if (pointer.Length == 0) return root;
        JsonNode? current = root;
        foreach (var raw in pointer.Split('/').Skip(1))
        {
            var part = raw.Replace("~1", "/").Replace("~0", "~");
            current = current switch
            {
                JsonObject obj => obj[part],
                JsonArray array when int.TryParse(part, out var index) && index < array.Count => array[index],
                _ => null
            };
            if (current is null) return null;
        }

        return current;
    }
}
