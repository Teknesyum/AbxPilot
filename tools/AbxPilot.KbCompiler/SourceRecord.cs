using System.Text.Json.Nodes;

namespace AbxPilot.KbCompiler;

public sealed record SourceRecord(JsonObject Node, string File, int Line, IReadOnlyDictionary<string, int> Lines)
{
    public int LineOf(string pointer)
    {
        var current = pointer;
        while (true)
        {
            if (Lines.TryGetValue(current, out var line)) return line;
            if (current.Length == 0) return Line;
            var cut = current.LastIndexOf('/');
            current = cut <= 0 ? "" : current[..cut];
        }
    }

    public string? Id => Node["id"] is JsonValue value && value.TryGetValue<string>(out var id) ? id : null;
}
