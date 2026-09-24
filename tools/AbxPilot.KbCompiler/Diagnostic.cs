namespace AbxPilot.KbCompiler;

public enum Severity
{
    Warning,
    Error
}

public sealed record Diagnostic(Severity Severity, string Code, string File, int Line, string Message)
{
    public override string ToString() =>
        $"{File}({Math.Max(Line, 1)}): {(Severity == Severity.Error ? "error" : "warning")} {Code}: {Message}";
}

public static class Codes
{
    public const string Parse = "KB001";
    public const string Schema = "KB002";
    public const string Duplicate = "KB003";
    public const string Model = "KB004";
    public const string Layout = "KB005";
    public const string UnknownReference = "KB010";
    public const string UnknownDrug = "KB011";
    public const string UnknownDose = "KB012";
    public const string InvalidOption = "KB013";
    public const string UnknownQuestion = "KB014";
    public const string MissingTr = "KB020";
    public const string MissingEn = "KB021";
    public const string DuplicateKey = "KB022";
    public const string Scoring = "KB030";
    public const string Coverage = "KB031";
}

public sealed class DiagnosticBag
{
    private readonly List<Diagnostic> _items = [];

    public IReadOnlyList<Diagnostic> Items => _items;

    public bool HasErrors => _items.Any(item => item.Severity == Severity.Error);

    public void Error(string code, string file, int line, string message) =>
        _items.Add(new Diagnostic(Severity.Error, code, file, line, message));

    public void Warning(string code, string file, int line, string message) =>
        _items.Add(new Diagnostic(Severity.Warning, code, file, line, message));
}
