namespace AbxPilot.Core;

public enum RecommendationStatus
{
    Selected,
    NoGuidelineRow,
    NoCandidateLeft,
    Referral,
    NoAntibiotic
}

public enum AnswerOrigin
{
    Answered,
    Default,
    RegionDefault,
    Hidden
}

public sealed record Recommendation(
    string SyndromeId,
    string GuidelineSet,
    string Region,
    string KbVersion,
    RecommendationStatus Status,
    string? MatchedRuleId,
    RegimenLine? FirstChoice,
    IReadOnlyList<RegimenLine> Alternatives,
    IReadOnlyList<ExcludedRegimen> Excluded,
    IReadOnlyList<CoverageBar> Spectrum,
    IReadOnlyList<TraceLine> Rationale,
    IReadOnlyList<string> RiskFlags,
    IReadOnlyList<QuestionState> Questions,
    IReadOnlyList<TraceLine> Trace)
{
    public bool ConsultSpecialist => Status is RecommendationStatus.NoGuidelineRow or RecommendationStatus.NoCandidateLeft
        or RecommendationStatus.Referral;

    public IEnumerable<string> VisibleQuestions => Questions.Where(item => item.Visible).Select(item => item.QuestionId);
}

public sealed record QuestionState(string QuestionId, bool Visible, IReadOnlyList<string> Value, AnswerOrigin Origin);

public sealed record RegimenLine(
    string RegimenId,
    string RuleId,
    int Tier,
    IReadOnlyList<ComponentLine> Components,
    int? DurationDays,
    SourceRef Source,
    ComparisonScore? Score)
{
    public IEnumerable<string> DrugIds => Components.Select(component => component.DrugId);

    public string Key => string.Join("+", Components.Select(component => component.DrugId));

    public IReadOnlyList<TraceLine> Warnings { get; init; } = [];
}

public sealed record ComponentLine(
    string DrugId,
    string Role,
    string DoseId,
    string Amount,
    string? Amount70Kg,
    string? Loading,
    int IntervalHours,
    string Route,
    string? AddedByRule,
    SourceRef Source);

public sealed record ExcludedRegimen(
    string RegimenId,
    string RuleId,
    int Tier,
    string ReasonKey,
    string? DrugId,
    SourceRef Source);

public sealed record CoverageBar(string PathogenId, string Level, double Coverage, bool AtRisk);

public sealed record TraceLine(string RuleId, string MessageKey, SourceRef Source, string? Subject = null);

public sealed record SourceRef(
    string Id,
    int Version,
    string Source,
    string Section,
    DateOnly SourceDate,
    DateOnly? ReviewedAt,
    string ReviewStatus);
