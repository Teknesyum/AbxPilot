namespace AbxPilot.Core;

public sealed record DoseChange(string DrugId, ComponentLine Before, ComponentLine After);

public sealed record SpectrumChange(string PathogenId, CoverageBar? Before, CoverageBar? After);

public sealed record RecommendationDiff(
    bool StatusChanged,
    bool FirstChoiceChanged,
    RegimenLine? FirstChoiceBefore,
    RegimenLine? FirstChoiceAfter,
    IReadOnlyList<DoseChange> DoseChanges,
    bool DurationChanged,
    IReadOnlyList<string> AlternativesAdded,
    IReadOnlyList<string> AlternativesRemoved,
    IReadOnlyList<ExcludedRegimen> NewlyExcluded,
    IReadOnlyList<ExcludedRegimen> NoLongerExcluded,
    IReadOnlyList<SpectrumChange> SpectrumChanges,
    IReadOnlyList<string> ChangedAnswers,
    IReadOnlyList<string> QuestionsShown,
    IReadOnlyList<string> QuestionsHidden)
{
    public bool IsEmpty =>
        !StatusChanged && !FirstChoiceChanged && DoseChanges.Count == 0 && !DurationChanged &&
        AlternativesAdded.Count == 0 && AlternativesRemoved.Count == 0 && NewlyExcluded.Count == 0 &&
        NoLongerExcluded.Count == 0 && SpectrumChanges.Count == 0;

    public static RecommendationDiff Between(Recommendation before, Recommendation after)
    {
        var first = before.FirstChoice;
        var next = after.FirstChoice;
        var doses = new List<DoseChange>();
        if (first is not null && next is not null)
        {
            foreach (var component in next.Components)
            {
                var old = first.Components.FirstOrDefault(item => item.DrugId == component.DrugId);
                if (old is not null && (old.Amount != component.Amount || old.IntervalHours != component.IntervalHours ||
                                        old.Loading != component.Loading || old.Route != component.Route))
                    doses.Add(new DoseChange(component.DrugId, old, component));
            }
        }

        var alternativesBefore = before.Alternatives.Select(line => line.Key).ToHashSet(StringComparer.Ordinal);
        var alternativesAfter = after.Alternatives.Select(line => line.Key).ToHashSet(StringComparer.Ordinal);
        var excludedBefore = before.Excluded.Select(item => item.RegimenId).ToHashSet(StringComparer.Ordinal);
        var excludedAfter = after.Excluded.Select(item => item.RegimenId).ToHashSet(StringComparer.Ordinal);

        var barsBefore = before.Spectrum.ToDictionary(bar => bar.PathogenId, StringComparer.Ordinal);
        var barsAfter = after.Spectrum.ToDictionary(bar => bar.PathogenId, StringComparer.Ordinal);
        var spectrum = barsBefore.Keys.Union(barsAfter.Keys)
            .Select(id => new SpectrumChange(id, barsBefore.GetValueOrDefault(id), barsAfter.GetValueOrDefault(id)))
            .Where(change => change.Before != change.After)
            .ToArray();

        var statesBefore = before.Questions.ToDictionary(state => state.QuestionId, StringComparer.Ordinal);
        var statesAfter = after.Questions.ToDictionary(state => state.QuestionId, StringComparer.Ordinal);
        var ids = before.Questions.Select(state => state.QuestionId)
            .Concat(after.Questions.Select(state => state.QuestionId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var changed = ids.Where(id =>
        {
            var a = statesBefore.GetValueOrDefault(id);
            var b = statesAfter.GetValueOrDefault(id);
            return a is null || b is null || a.Visible != b.Visible || !a.Value.SequenceEqual(b.Value);
        }).ToArray();

        return new RecommendationDiff(
            before.Status != after.Status,
            first?.Key != next?.Key,
            first,
            next,
            doses,
            first?.DurationDays != next?.DurationDays,
            after.Alternatives.Select(line => line.Key).Where(key => !alternativesBefore.Contains(key)).ToArray(),
            before.Alternatives.Select(line => line.Key).Where(key => !alternativesAfter.Contains(key)).ToArray(),
            after.Excluded.Where(item => !excludedBefore.Contains(item.RegimenId)).ToArray(),
            before.Excluded.Where(item => !excludedAfter.Contains(item.RegimenId)).ToArray(),
            spectrum,
            changed,
            ids.Where(id => statesAfter.GetValueOrDefault(id)?.Visible == true && statesBefore.GetValueOrDefault(id)?.Visible != true).ToArray(),
            ids.Where(id => statesBefore.GetValueOrDefault(id)?.Visible == true && statesAfter.GetValueOrDefault(id)?.Visible != true).ToArray());
    }
}
