namespace AbxPilot.Core;

public sealed record Recommendation(
    string SyndromeId,
    RegimenLine? FirstChoice,
    IReadOnlyList<RegimenLine> Alternatives,
    IReadOnlyList<ExcludedRegimen> Excluded,
    IReadOnlyList<CoverageBar> Spectrum,
    IReadOnlyList<TraceLine> Trace);

public sealed record RegimenLine(
    string RegimenId,
    string Dose,
    string Route,
    string Duration,
    SourceRef Source,
    ComparisonScore? Score);

public sealed record ExcludedRegimen(string RegimenId, string ReasonKey, SourceRef Source);

public sealed record CoverageBar(string PathogenId, double Coverage);

public sealed record TraceLine(string RuleId, string MessageKey, SourceRef Source);

public sealed record SourceRef(
    string Id,
    string Version,
    string Source,
    string Section,
    DateOnly SourceDate,
    DateOnly ReviewedAt);
