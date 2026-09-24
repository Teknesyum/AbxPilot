namespace AbxPilot.Core;

public interface IGuidelineEngine
{
    Recommendation Evaluate(GuidelineContext context);
}

public sealed record GuidelineContext(
    string SyndromeId,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Answers,
    string Region,
    string GuidelineSet)
{
    public static GuidelineContext Create(string syndromeId, string region, string guidelineSet,
        params (string Question, string[] Values)[] answers) =>
        new(syndromeId,
            answers.ToDictionary(pair => pair.Question, pair => (IReadOnlyList<string>)pair.Values, StringComparer.Ordinal),
            region,
            guidelineSet);

    public GuidelineContext WithAnswer(string question, params string[] values)
    {
        var answers = new Dictionary<string, IReadOnlyList<string>>(Answers, StringComparer.Ordinal) { [question] = values };
        return this with { Answers = answers };
    }

    public GuidelineContext WithoutAnswer(string question)
    {
        var answers = new Dictionary<string, IReadOnlyList<string>>(Answers, StringComparer.Ordinal);
        answers.Remove(question);
        return this with { Answers = answers };
    }
}
