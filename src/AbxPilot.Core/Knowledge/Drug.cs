using System.Text.Json.Serialization;

namespace AbxPilot.Core.Knowledge;

public sealed record Drug : KbRecord
{
    public string Class { get; init; } = "";
    public string ClassGroup { get; init; } = "";
    public string? Subclass { get; init; }
    public string Aware { get; init; } = "";
    public IReadOnlyList<string> Routes { get; init; } = [];
    public string OralBioavailability { get; init; } = "";
    public BetaLactamInfo? BetaLactam { get; init; }
    public IReadOnlyList<Dose> Doses { get; init; } = [];
    public IReadOnlyList<string> AdverseEffects { get; init; } = [];
    public IReadOnlyList<string> Interactions { get; init; } = [];
    public PregnancyInfo Pregnancy { get; init; } = new();
    public string QtRisk { get; init; } = "";
}

public sealed record BetaLactamInfo
{
    public string Core { get; init; } = "";
    public string R1Group { get; init; } = "";
    public IReadOnlyList<string> R1Similar { get; init; } = [];
}

public sealed record PregnancyInfo
{
    public string Category { get; init; } = "";
    public string? NoteKey { get; init; }
}

public sealed record Dose
{
    public string Id { get; init; } = "";
    public string Indication { get; init; } = "";
    public string Route { get; init; } = "";
    public string RenalBand { get; init; } = "";
    public string Amount { get; init; } = "";

    [JsonPropertyName("amount_70kg")]
    public string? Amount70Kg { get; init; }

    public string? Loading { get; init; }
    public int IntervalHours { get; init; }
    public string Source { get; init; } = "";
    public string Section { get; init; } = "";
    public string? Note { get; init; }
}
