namespace AbxPilot.Core.Knowledge;

public abstract record KbRecord
{
    public string Id { get; init; } = "";
    public int Version { get; init; }
    public string Source { get; init; } = "";
    public string Section { get; init; } = "";
    public DateOnly SourceDate { get; init; }
    public DateOnly? ReviewedAt { get; init; }
    public string ReviewStatus { get; init; } = "";
    public string? Note { get; init; }

    public SourceRef ToSourceRef() =>
        new(Id, Version, Source, Section, SourceDate, ReviewedAt, ReviewStatus);
}

public sealed record KbSource
{
    public string Id { get; init; } = "";
    public int Version { get; init; }
    public string Kind { get; init; } = "";
    public string Citation { get; init; } = "";
    public string? Url { get; init; }
    public DateOnly Published { get; init; }
    public string DatePrecision { get; init; } = "";
    public string? Note { get; init; }
}
