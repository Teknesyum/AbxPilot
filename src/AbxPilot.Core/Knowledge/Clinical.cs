using System.Text.Json;

namespace AbxPilot.Core.Knowledge;

public sealed record Pathogen : KbRecord
{
    public string Kind { get; init; } = "";
    public string Gram { get; init; } = "";
    public string Group { get; init; } = "";
    public IReadOnlyList<string> ResistanceMechanisms { get; init; } = [];
}

public sealed record Regimen : KbRecord
{
    public IReadOnlyList<RegimenComponent> Components { get; init; } = [];
}

public sealed record RegimenComponent
{
    public string Drug { get; init; } = "";
    public string Dose { get; init; } = "";
    public string Role { get; init; } = "";
}

public sealed record Question : KbRecord
{
    public string Type { get; init; } = "";
    public IReadOnlyList<string> Options { get; init; } = [];
    public JsonElement Default { get; init; }
    public string? DefaultFrom { get; init; }
    public Condition? VisibleWhen { get; init; }
}

public sealed record Condition
{
    public string? Question { get; init; }
    public string? Eq { get; init; }
    public IReadOnlyList<string>? In { get; init; }
    public IReadOnlyList<string>? ContainsAny { get; init; }
    public bool? Empty { get; init; }
    public IReadOnlyList<Condition>? All { get; init; }
    public IReadOnlyList<Condition>? Any { get; init; }
    public Condition? Not { get; init; }
}

public sealed record Syndrome : KbRecord
{
    public IReadOnlyList<string> Pathogens { get; init; } = [];
    public IReadOnlyList<string> Questions { get; init; } = [];
    public IReadOnlyList<DerivedFlag> Derived { get; init; } = [];
}

public sealed record DerivedFlag
{
    public string Id { get; init; } = "";
    public Condition When { get; init; } = new();
}

public sealed record GuidelineSet : KbRecord
{
    public string? Extends { get; init; }
}

public sealed record GuidelineRow : KbRecord
{
    public string Set { get; init; } = "";
    public string Syndrome { get; init; } = "";
    public int Order { get; init; }
    public string Stage { get; init; } = "";
    public string Action { get; init; } = "";
    public string? Role { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Conditions { get; init; } =
        new Dictionary<string, IReadOnlyList<string>>();
    public IReadOnlyList<IReadOnlyList<string>> Candidates { get; init; } = [];
    public int? DurationDays { get; init; }
    public string RationaleKey { get; init; } = "";
}

public sealed record SpectrumEntry : KbRecord
{
    public string Drug { get; init; } = "";
    public IReadOnlyDictionary<string, string> Coverage { get; init; } = new Dictionary<string, string>();
}
