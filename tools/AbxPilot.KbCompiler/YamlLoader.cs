using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace AbxPilot.KbCompiler;

public static partial class YamlLoader
{
    [GeneratedRegex(@"^-?[0-9]+$")]
    private static partial Regex IntegerPattern();

    [GeneratedRegex(@"^-?[0-9]+\.[0-9]+$")]
    private static partial Regex DecimalPattern();

    public static IReadOnlyList<SourceRecord> Load(string path, DiagnosticBag bag)
    {
        var stream = new YamlStream();
        try
        {
            using var reader = new StreamReader(path);
            stream.Load(reader);
        }
        catch (YamlException ex)
        {
            bag.Error(Codes.Parse, path, (int)ex.Start.Line, ex.InnerException?.Message ?? ex.Message);
            return [];
        }

        if (stream.Documents.Count == 0)
        {
            bag.Error(Codes.Parse, path, 1, "empty YAML file");
            return [];
        }

        var root = stream.Documents[0].RootNode;
        var records = new List<SourceRecord>();

        if (root is YamlSequenceNode sequence)
        {
            foreach (var child in sequence.Children)
                AddRecord(child, path, bag, records);
        }
        else
        {
            AddRecord(root, path, bag, records);
        }

        return records;
    }

    private static void AddRecord(YamlNode node, string path, DiagnosticBag bag, List<SourceRecord> records)
    {
        var line = (int)node.Start.Line;
        if (node is not YamlMappingNode)
        {
            bag.Error(Codes.Parse, path, line, "a record must be a mapping");
            return;
        }

        var lines = new Dictionary<string, int>(StringComparer.Ordinal);
        if (Convert(node, "", lines, path, bag) is JsonObject obj)
            records.Add(new SourceRecord(obj, path, line, lines));
    }

    private static JsonNode? Convert(YamlNode node, string pointer, Dictionary<string, int> lines, string path, DiagnosticBag bag)
    {
        lines[pointer] = (int)node.Start.Line;
        switch (node)
        {
            case YamlMappingNode mapping:
            {
                var obj = new JsonObject();
                foreach (var (keyNode, valueNode) in mapping.Children)
                {
                    var key = (keyNode as YamlScalarNode)?.Value ?? "";
                    if (obj.ContainsKey(key))
                    {
                        bag.Error(Codes.Parse, path, (int)keyNode.Start.Line, $"duplicate key '{key}'");
                        continue;
                    }

                    obj[key] = Convert(valueNode, pointer + "/" + Escape(key), lines, path, bag);
                }

                return obj;
            }
            case YamlSequenceNode sequence:
            {
                var array = new JsonArray();
                var index = 0;
                foreach (var child in sequence.Children)
                    array.Add(Convert(child, pointer + "/" + index++, lines, path, bag));
                return array;
            }
            case YamlScalarNode scalar:
                return Scalar(scalar);
            default:
                bag.Error(Codes.Parse, path, (int)node.Start.Line, "aliases and anchors are not supported");
                return null;
        }
    }

    private static JsonNode? Scalar(YamlScalarNode scalar)
    {
        var text = scalar.Value ?? "";
        if (scalar.Style != ScalarStyle.Plain) return JsonValue.Create(text);

        switch (text)
        {
            case "" or "~" or "null":
                return null;
            case "true":
                return JsonValue.Create(true);
            case "false":
                return JsonValue.Create(false);
        }

        if (IntegerPattern().IsMatch(text)
            && long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
            return JsonValue.Create(integer);
        if (DecimalPattern().IsMatch(text)
            && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            return JsonValue.Create(number);
        return JsonValue.Create(text);
    }

    internal static string Escape(string key) => key.Replace("~", "~0").Replace("/", "~1");
}
