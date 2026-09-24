using AbxPilot.Core.Knowledge;

namespace AbxPilot.Core.Engine;

public sealed class ComparisonScorer(Scoring scoring, IReadOnlyDictionary<string, SpectrumEntry> spectrum)
{
    private const string NoCoverage = "none";
    private static readonly double HoursPerDay = TimeSpan.FromDays(1).TotalHours;

    public (string Level, double Value) Level(string drugId, string pathogenId)
    {
        var level = spectrum.TryGetValue(drugId, out var entry) && entry.Coverage.TryGetValue(pathogenId, out var found)
            ? found
            : NoCoverage;
        return (level, scoring.CoverageValue.GetValueOrDefault(level));
    }

    public IReadOnlyList<ComparisonScore> Score(IReadOnlyList<RegimenLine> lines, IReadOnlyDictionary<string, Drug> catalog,
        IReadOnlyList<string> pathogens, IReadOnlyList<string> syndromePathogens, Region? region)
    {
        var candidates = lines.Select(line => line.Components.Select(component => catalog[component.DrugId]).ToArray()).ToArray();
        var effects = candidates.Select(drugs => drugs.SelectMany(drug => drug.AdverseEffects).Distinct().Count()).ToArray();
        var maxEffects = effects.DefaultIfEmpty().Max();
        var dosesPerDay = lines.Select(line => line.Components.Sum(component => component.IntervalHours > 0 ? HoursPerDay / component.IntervalHours : 0)).ToArray();
        var fewestDoses = dosesPerDay.DefaultIfEmpty().Min();

        var result = new ComparisonScore[candidates.Length];
        for (var i = 0; i < candidates.Length; i++)
        {
            var drugs = candidates[i];
            var values = new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["spectrum_fit"] = pathogens.Count == 0
                    ? 0
                    : pathogens.Average(pathogen => drugs.Max(drug => Level(drug.Id, pathogen).Value)),
                ["breadth_penalty"] = 1 - drugs.Max(drug => scoring.AwarePenalty.GetValueOrDefault(drug.Aware)),
                ["adverse_effects"] = maxEffects == 0 ? 1 : 1 - (double)effects[i] / maxEffects,
                ["dosing_convenience"] = dosesPerDay[i] <= 0 ? 0 : fewestDoses / dosesPerDay[i],
                ["oral_switch"] = drugs.Min(drug => scoring.BioavailabilityValue.GetValueOrDefault(drug.OralBioavailability)),
                ["regional_resistance"] = 1 - drugs.Max(drug => ResistancePenalty(drug, syndromePathogens, region))
            };

            var components = scoring.Components
                .Select(component => new ScoreComponent(component.Id, values.GetValueOrDefault(component.Id), component.Weight))
                .ToArray();
            result[i] = new ComparisonScore(components.Sum(component => component.Contribution), components, scoring.Id, scoring.Version);
        }

        return result;
    }

    private double ResistancePenalty(Drug drug, IReadOnlyList<string> pathogens, Region? region) =>
        region?.Resistance
            .Where(entry => entry.DrugClass == drug.ClassGroup && pathogens.Contains(entry.Pathogen))
            .Select(entry => scoring.ResistancePenalty.GetValueOrDefault(entry.Category))
            .DefaultIfEmpty()
            .Max() ?? 0;
}
