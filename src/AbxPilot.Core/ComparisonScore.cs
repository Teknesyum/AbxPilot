namespace AbxPilot.Core;

public sealed record ComparisonScore(
    double Total,
    double SpectrumFit,
    double BreadthPenalty,
    double AdverseEffects,
    double DosingConvenience,
    double OralSwitch,
    double RegionalResistance,
    double Cost,
    string WeightsVersion);
