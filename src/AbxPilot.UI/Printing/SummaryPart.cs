namespace AbxPilot.UI.Printing;

public enum SummaryKind
{
    Title,
    Headline,
    Dose,
    Note,
    Warning,
    Heading,
    Answer,
    Meta,
    Disclaimer
}

public sealed record SummaryPart(SummaryKind Kind, string Text);
