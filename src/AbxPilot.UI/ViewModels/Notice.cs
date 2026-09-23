namespace AbxPilot.UI.ViewModels;

public enum NoticeKind
{
    Neutral,
    Success,
    Warning,
    Error,
}

public sealed record Notice(NoticeKind Kind, string MessageKey, TimeSpan Life)
{
    public TimeSpan? AutoCloseAfter => Kind == NoticeKind.Error ? null : Life;
}
