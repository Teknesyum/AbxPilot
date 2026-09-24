namespace AbxPilot.Core;

public sealed record ComparisonScore(
    double Total,
    IReadOnlyList<ScoreComponent> Components,
    string WeightsId,
    int WeightsVersion);

public sealed record ScoreComponent(string Id, double Value, double Weight)
{
    public double Contribution => Value * Weight;
}
