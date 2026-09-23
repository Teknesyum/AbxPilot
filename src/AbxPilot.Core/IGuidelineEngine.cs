namespace AbxPilot.Core;

public interface IGuidelineEngine
{
    Recommendation Evaluate(GuidelineContext context);
}

public sealed record GuidelineContext(
    string SyndromeId,
    IReadOnlyDictionary<string, string> Answers,
    string Region,
    string GuidelineSet);
