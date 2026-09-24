using System.Text;

namespace AbxPilot.KbCompiler;

public sealed record CsvRow(int Line, IReadOnlyDictionary<string, string> Cells);

public static class CsvLoader
{
    public static (IReadOnlyList<string> Header, IReadOnlyList<CsvRow> Rows) Load(string path, DiagnosticBag bag)
    {
        var records = Parse(File.ReadAllText(path), path, bag);
        if (records.Count == 0)
        {
            bag.Error(Codes.Parse, path, 1, "empty CSV file");
            return ([], []);
        }

        var header = records[0].Fields.Select(field => field.Trim()).ToArray();
        var duplicate = header.GroupBy(name => name, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            bag.Error(Codes.Parse, path, records[0].Line, $"duplicate column '{duplicate.Key}'");
            return ([], []);
        }

        var rows = new List<CsvRow>();
        foreach (var (line, fields) in records.Skip(1))
        {
            if (fields.All(string.IsNullOrWhiteSpace)) continue;
            if (fields.Count != header.Length)
            {
                bag.Error(Codes.Parse, path, line, $"row has {fields.Count} cells, header has {header.Length}");
                continue;
            }

            var cells = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var i = 0; i < header.Length; i++) cells[header[i]] = fields[i].Trim();
            rows.Add(new CsvRow(line, cells));
        }

        return (header, rows);
    }

    private static List<(int Line, List<string> Fields)> Parse(string text, string path, DiagnosticBag bag)
    {
        const char Quote = '"';
        var result = new List<(int, List<string>)>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var line = 1;
        var start = 1;
        var quoted = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == Quote && i + 1 < text.Length && text[i + 1] == Quote)
                {
                    field.Append(Quote);
                    i++;
                }
                else if (c == Quote)
                {
                    quoted = false;
                }
                else
                {
                    if (c == '\n') line++;
                    field.Append(c);
                }

                continue;
            }

            if (c == Quote && field.Length == 0)
            {
                quoted = true;
            }
            else if (c == ',')
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else if (c == '\n')
            {
                fields.Add(field.ToString());
                field.Clear();
                result.Add((start, fields));
                fields = [];
                line++;
                start = line;
            }
            else if (c != '\r')
            {
                field.Append(c);
            }
        }

        if (quoted) bag.Error(Codes.Parse, path, start, "unterminated quoted field");
        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            result.Add((start, fields));
        }

        return result;
    }
}
