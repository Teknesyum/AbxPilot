namespace AbxPilot.Core.Knowledge;

public sealed record Region : KbRecord
{
    public IReadOnlyList<RegionResistance> Resistance { get; init; } = [];
    public IReadOnlyList<RegionThreshold> Thresholds { get; init; } = [];
    public IReadOnlyList<DrugLicense> Licensing { get; init; } = [];
}

public sealed record RegionResistance : KbRecord
{
    public string Pathogen { get; init; } = "";
    public string DrugClass { get; init; } = "";
    public string Category { get; init; } = "";
    public double? Estimate { get; init; }
}

public sealed record RegionThreshold : KbRecord
{
    public double Value { get; init; }
    public string Unit { get; init; } = "";
}

public sealed record DrugLicense
{
    public string Drug { get; init; } = "";
    public string Status { get; init; } = "";
    public string? Note { get; init; }
}

public sealed record Scoring : KbRecord
{
    public IReadOnlyList<ScoringComponent> Components { get; init; } = [];
    public IReadOnlyDictionary<string, double> AwarePenalty { get; init; } = new Dictionary<string, double>();
    public IReadOnlyDictionary<string, double> CoverageValue { get; init; } = new Dictionary<string, double>();
    public IReadOnlyDictionary<string, double> BioavailabilityValue { get; init; } = new Dictionary<string, double>();
    public IReadOnlyDictionary<string, double> ResistancePenalty { get; init; } = new Dictionary<string, double>();
}

public sealed record ScoringComponent
{
    public string Id { get; init; } = "";
    public double Weight { get; init; }
    public string Rationale { get; init; } = "";
}

public sealed record KnowledgeBase
{
    public int Schema { get; init; }
    public string Version { get; init; } = "";
    public IReadOnlyList<KbSource> Sources { get; init; } = [];
    public IReadOnlyList<Drug> Drugs { get; init; } = [];
    public IReadOnlyList<Pathogen> Pathogens { get; init; } = [];
    public IReadOnlyList<Regimen> Regimens { get; init; } = [];
    public IReadOnlyList<Question> Questions { get; init; } = [];
    public IReadOnlyList<Syndrome> Syndromes { get; init; } = [];
    public IReadOnlyList<GuidelineSet> GuidelineSets { get; init; } = [];
    public IReadOnlyList<GuidelineRow> GuidelineRows { get; init; } = [];
    public IReadOnlyList<SpectrumEntry> Spectrum { get; init; } = [];
    public IReadOnlyList<Region> Regions { get; init; } = [];
    public IReadOnlyList<Constraint> Constraints { get; init; } = [];
    public Scoring? Scoring { get; init; }
}
