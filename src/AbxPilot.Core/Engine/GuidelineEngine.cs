using System.Text.Json;
using AbxPilot.Core.Knowledge;

namespace AbxPilot.Core.Engine;

public sealed class GuidelineEngine : IGuidelineEngine
{
    private const string BaseStage = "base";
    private const string ModifierStage = "modifier";
    private const string AddAction = "add";
    private const string ReplaceAction = "replace";
    private const string NoteAction = "note";
    private const string ReferAction = "refer";
    private const string WithholdAction = "withhold";
    private const string MultiType = "multi";
    private const string BaselineRenalBand = "normal";
    private const string UnknownLicense = "unknown";
    private static readonly IReadOnlyList<string> Yes = ["yes"];
    private static readonly IReadOnlyList<string> No = ["no"];
    private static readonly IReadOnlyList<string> Nothing = [];

    private readonly KnowledgeBase _kb;
    private readonly Dictionary<string, Drug> _drugs;
    private readonly Dictionary<string, Regimen> _regimens;
    private readonly Dictionary<string, Question> _questions;
    private readonly Dictionary<string, Syndrome> _syndromes;
    private readonly Dictionary<string, GuidelineSet> _sets;
    private readonly Dictionary<string, Region> _regions;
    private readonly Dictionary<string, KbSource> _sources;
    private readonly Dictionary<(string Set, string Syndrome), GuidelineRow[]> _rows;
    private readonly Dictionary<string, ComparisonScorer> _scorers;

    public GuidelineEngine(KnowledgeBase knowledge)
    {
        _kb = knowledge;
        _drugs = knowledge.Drugs.ToDictionary(item => item.Id, StringComparer.Ordinal);
        _regimens = knowledge.Regimens.ToDictionary(item => item.Id, StringComparer.Ordinal);
        _questions = knowledge.Questions.ToDictionary(item => item.Id, StringComparer.Ordinal);
        _syndromes = knowledge.Syndromes.ToDictionary(item => item.Id, StringComparer.Ordinal);
        _sets = knowledge.GuidelineSets.ToDictionary(item => item.Id, StringComparer.Ordinal);
        _regions = knowledge.Regions.ToDictionary(item => item.Id, StringComparer.OrdinalIgnoreCase);
        _sources = knowledge.Sources.ToDictionary(item => item.Id, StringComparer.Ordinal);
        _rows = knowledge.GuidelineRows
            .GroupBy(row => (row.Set, row.Syndrome))
            .ToDictionary(group => group.Key, group => group.OrderBy(row => row.Order).ToArray());
        _scorers = knowledge.Scoring is { } scoring
            ? knowledge.Syndromes.ToDictionary(
                item => item.Id,
                item => new ComparisonScorer(scoring, knowledge.Spectrum
                    .Where(entry => entry.Syndrome == item.Id)
                    .ToDictionary(entry => entry.Drug, StringComparer.Ordinal)),
                StringComparer.Ordinal)
            : new Dictionary<string, ComparisonScorer>(StringComparer.Ordinal);
    }

    public KnowledgeBase Knowledge => _kb;

    public Recommendation Evaluate(GuidelineContext context)
    {
        var syndrome = _syndromes.GetValueOrDefault(context.SyndromeId)
                       ?? throw new ArgumentException($"unknown syndrome '{context.SyndromeId}'", nameof(context));
        var set = _sets.GetValueOrDefault(context.GuidelineSet)
                  ?? throw new ArgumentException($"unknown guideline set '{context.GuidelineSet}'", nameof(context));
        var region = _regions.GetValueOrDefault(context.Region);
        var run = new Run(this, context, syndrome, set, region);
        return run.Execute();
    }

    private static bool Matches(Condition condition, IReadOnlyDictionary<string, IReadOnlyList<string>> values, Region? region)
    {
        if (condition.All is { } all) return all.All(item => Matches(item, values, region));
        if (condition.Any is { } any) return any.Any(item => Matches(item, values, region));
        if (condition.Not is { } not) return !Matches(not, values, region);

        IReadOnlyList<string> current;
        if (condition.Region is { } resistance)
            current = region?.Resistance.FirstOrDefault(entry => entry.Id == resistance)?.Category is { } category ? [category] : Nothing;
        else
            current = condition.Question is { } question && values.TryGetValue(question, out var value) ? value : Nothing;

        if (condition.Eq is { } eq) return current.Contains(eq);
        if (condition.In is { } list) return current.Any(list.Contains);
        if (condition.ContainsAny is { } contains) return current.Any(contains.Contains);
        if (condition.Empty is { } empty) return current.Count == 0 == empty;
        return false;
    }

    private static IReadOnlyList<string> DefaultOf(Question question) =>
        question.Default.ValueKind switch
        {
            JsonValueKind.String => [question.Default.GetString()!],
            JsonValueKind.Array => question.Default.EnumerateArray().Select(item => item.GetString() ?? "").Where(item => item.Length > 0).ToArray(),
            _ => Nothing
        };

    private HashSet<string> R1Groups(IEnumerable<string> cores)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var drug in _kb.Drugs)
        {
            if (drug.BetaLactam is not { } lactam || !cores.Contains(lactam.Core)) continue;
            result.Add(lactam.R1Group);
            result.UnionWith(lactam.R1Similar);
        }

        return result;
    }

    private SourceRef DoseSource(Drug drug, Dose dose) =>
        new($"{drug.Id}.{dose.Id}", drug.Version, dose.Source, dose.Section,
            _sources.TryGetValue(dose.Source, out var source) ? source.Published : drug.SourceDate,
            drug.ReviewedAt, drug.ReviewStatus);

    private sealed record Part(RegimenComponent Component, string? AddedBy, bool Replacing = false);

    private static string PartKey(IEnumerable<RegimenComponent> components) =>
        string.Join("+", components.Select(component => $"{component.Drug}:{component.Dose}").Order(StringComparer.Ordinal));

    private string? RegimenFor(IEnumerable<RegimenComponent> components)
    {
        var key = PartKey(components);
        return _kb.Regimens.FirstOrDefault(item => PartKey(item.Components) == key)?.Id;
    }

    private sealed record Modifier(GuidelineRow Row, Regimen Chosen);

    private sealed record Violation(Constraint Rule, string DrugId);

    private sealed class Run(
        GuidelineEngine engine,
        GuidelineContext context,
        Syndrome syndrome,
        GuidelineSet set,
        Region? region)
    {
        private readonly List<TraceLine> _trace = [];
        private readonly List<TraceLine> _rationale = [];
        private readonly List<ExcludedRegimen> _excluded = [];
        private readonly Dictionary<string, IReadOnlyList<string>> _values = new(StringComparer.Ordinal);
        private readonly List<QuestionState> _states = [];
        private readonly List<string> _risks = [];
        private readonly HashSet<string> _riskPathogens = new(StringComparer.Ordinal);
        private readonly HashSet<string> _quietPathogens = new(StringComparer.Ordinal);
        private List<Constraint> _active = [];
        private List<Constraint> _soft = [];
        private ComparisonScorer? Scorer => engine._scorers.GetValueOrDefault(syndrome.Id);

        public Recommendation Execute()
        {
            _trace.Add(new TraceLine("context", "engine.context", set.ToSourceRef(), $"{syndrome.Id}/{set.Id}/{context.Region}"));
            if (region is null)
                _trace.Add(new TraceLine("region", "engine.region_missing", syndrome.ToSourceRef(), context.Region));

            ResolveQuestions();
            DeriveFlags();
            var active = engine._kb.Constraints
                .Where(rule => rule.When is null || Matches(rule.When, _values, region))
                .ToList();
            _active = active.Where(rule => rule.Mode == ConstraintMode.Exclude).ToList();
            _soft = active.Where(rule => rule.Mode != ConstraintMode.Exclude).ToList();

            var rows = engine._rows.GetValueOrDefault((set.Id, syndrome.Id)) ?? [];
            var baseRow = rows.FirstOrDefault(row => row.Stage == BaseStage && RowMatches(row));
            if (baseRow is null)
            {
                _trace.Add(new TraceLine("table", "engine.no_row", set.ToSourceRef()));
                return Finish(RecommendationStatus.NoGuidelineRow, null, []);
            }

            _trace.Add(new TraceLine(baseRow.Id, "engine.row_matched", baseRow.ToSourceRef()));
            _rationale.Add(new TraceLine(baseRow.Id, baseRow.RationaleKey, baseRow.ToSourceRef()));
            if (baseRow.Action == WithholdAction)
            {
                _trace.Add(new TraceLine(baseRow.Id, "engine.no_antibiotic", baseRow.ToSourceRef()));
                return Finish(RecommendationStatus.NoAntibiotic, baseRow.Id, []);
            }

            if (baseRow.Action == ReferAction)
                _trace.Add(new TraceLine(baseRow.Id, "engine.referral", baseRow.ToSourceRef()));

            var modifiers = new List<Modifier>();
            var blocked = false;
            var noteDays = new List<int?>();
            foreach (var row in rows.Where(row => row.Stage == ModifierStage && RowMatches(row)))
            {
                _rationale.Add(new TraceLine(row.Id, row.RationaleKey, row.ToSourceRef()));
                if (row.Action == NoteAction)
                {
                    _trace.Add(new TraceLine(row.Id, "engine.modifier_note", row.ToSourceRef()));
                    noteDays.Add(row.DurationDays);
                    continue;
                }

                var chosen = ChooseModifier(row);
                if (chosen is null)
                {
                    blocked = true;
                    _trace.Add(new TraceLine(row.Id, "engine.modifier_no_candidate", row.ToSourceRef()));
                    continue;
                }

                modifiers.Add(new Modifier(row, chosen));
                var key = row.Action == ReplaceAction ? "engine.modifier_replace" : "engine.modifier_add";
                _trace.Add(new TraceLine(row.Id, key, row.ToSourceRef(), chosen.Id));
            }

            var extraDays = modifiers.Select(item => item.Row.DurationDays).Concat(noteDays).Max();
            var duration = new[] { extraDays, baseRow.DurationDays }.Max();
            var lines = BuildCandidates(baseRow, modifiers, duration, extraDays);
            if (blocked) lines.Clear();
            if (baseRow.Action == ReferAction)
                return Finish(RecommendationStatus.Referral, baseRow.Id, lines.Count == 0 ? [] : Score(lines));
            if (lines.Count == 0)
            {
                _trace.Add(new TraceLine(baseRow.Id, "engine.no_candidate", baseRow.ToSourceRef()));
                return Finish(RecommendationStatus.NoCandidateLeft, baseRow.Id, []);
            }

            _trace.Add(new TraceLine(baseRow.Id, "engine.first_choice", lines[0].Source, lines[0].RegimenId));
            return Finish(RecommendationStatus.Selected, baseRow.Id, Score(lines));
        }

        private Recommendation Finish(RecommendationStatus status, string? ruleId, IReadOnlyList<RegimenLine> lines)
        {
            if (status is RecommendationStatus.NoGuidelineRow or RecommendationStatus.NoCandidateLeft)
                _rationale.Add(new TraceLine(ruleId ?? "table", "engine.consult_specialist", set.ToSourceRef()));
            var first = lines.Count > 0 ? lines[0] : null;
            return new Recommendation(
                syndrome.Id,
                set.Id,
                region?.Id ?? context.Region,
                engine._kb.Version,
                status,
                ruleId,
                first,
                lines.Skip(1).ToArray(),
                _excluded.ToArray(),
                first is null ? [] : Spectrum(first),
                _rationale.ToArray(),
                _risks.ToArray(),
                _states.ToArray(),
                _trace.ToArray());
        }

        private void ResolveQuestions()
        {
            foreach (var id in syndrome.Questions)
            {
                var question = engine._questions[id];
                var visible = question.VisibleWhen is null || Matches(question.VisibleWhen, _values, region);
                var answered = context.Answers.TryGetValue(id, out var given);
                if (!visible)
                {
                    if (answered && given!.Count > 0)
                        _trace.Add(new TraceLine(id, "engine.hidden_ignored", question.ToSourceRef()));
                    _states.Add(new QuestionState(id, false, Nothing, AnswerOrigin.Hidden));
                    continue;
                }

                if (answered)
                {
                    var valid = given!.Where(question.Options.Contains).Distinct(StringComparer.Ordinal).ToArray();
                    if (question.Type != MultiType && valid.Length > 1) valid = valid[..1];
                    if (valid.Length != given!.Count)
                        _trace.Add(new TraceLine(id, "engine.invalid_answer", question.ToSourceRef()));
                    if (valid.Length > 0 || (question.Type == MultiType && given.Count == 0))
                    {
                        Set(id, valid, AnswerOrigin.Answered);
                        continue;
                    }
                }

                if (question.DefaultFrom is { } resistanceId &&
                    region?.Resistance.FirstOrDefault(entry => entry.Id == resistanceId) is { } entry &&
                    question.Options.Contains(entry.Category))
                {
                    _trace.Add(new TraceLine(id, "engine.default_region", entry.ToSourceRef(), entry.Category));
                    Set(id, [entry.Category], AnswerOrigin.RegionDefault);
                    continue;
                }

                var fallback = DefaultOf(question);
                _trace.Add(new TraceLine(id, "engine.default", question.ToSourceRef(), string.Join(",", fallback)));
                Set(id, fallback, AnswerOrigin.Default);
            }
        }

        private void Set(string id, IReadOnlyList<string> value, AnswerOrigin origin)
        {
            _values[id] = value;
            _states.Add(new QuestionState(id, true, value, origin));
        }

        private void DeriveFlags()
        {
            foreach (var flag in syndrome.Derived)
            {
                var on = Matches(flag.When, _values, region);
                _values[flag.Id] = on ? Yes : No;
                if (flag.Risk is null)
                {
                    if (on) _trace.Add(new TraceLine(flag.Id, "engine.flag", syndrome.ToSourceRef()));
                    continue;
                }

                if (on)
                {
                    _risks.Add(flag.Risk);
                    if (flag.Pathogen is { } pathogen) _riskPathogens.Add(pathogen);
                    _trace.Add(new TraceLine(flag.Id, "engine.risk_flag", syndrome.ToSourceRef(), flag.Risk));
                }
                else if (flag.Pathogen is { } pathogen)
                {
                    _quietPathogens.Add(pathogen);
                }
            }

            _quietPathogens.ExceptWith(_riskPathogens);
        }

        private bool RowMatches(GuidelineRow row) =>
            row.Conditions.All(pair => _values.TryGetValue(pair.Key, out var value) && value.Any(pair.Value.Contains));

        private Regimen? ChooseModifier(GuidelineRow row)
        {
            for (var tier = 0; tier < row.Candidates.Count; tier++)
            {
                Regimen? fallback = null;
                foreach (var id in row.Candidates[tier])
                {
                    var regimen = engine._regimens[id];
                    var violation = FirstViolation(regimen.Components, false);
                    if (violation is not null)
                    {
                        Exclude(id, tier + 1, violation);
                        continue;
                    }

                    var parts = regimen.Components.Select(component => new Part(component, row.Id)).ToList();
                    if (!Soft(parts).Demoted) return regimen;
                    if (fallback is null)
                    {
                        fallback = regimen;
                        _trace.Add(new TraceLine(row.Id, "engine.demoted", regimen.ToSourceRef(), id));
                    }
                }

                if (fallback is not null) return fallback;
            }

            return null;
        }

        private (bool Demoted, List<TraceLine> Warnings) Soft(List<Part> parts)
        {
            var demoted = false;
            var warnings = new List<TraceLine>();
            foreach (var rule in _soft)
            foreach (var part in parts)
            {
                var drug = engine._drugs[part.Component.Drug];
                if (!Selects(rule.Exclude, drug, parts.Count == 1)) continue;
                if (warnings.All(item => item.RuleId != rule.Id || item.Subject != drug.Id))
                    warnings.Add(new TraceLine(rule.Id, rule.ReasonKey, rule.ToSourceRef(), drug.Id));
                var spared = rule.Spare is { } spare && part.Component.Role == spare.Role &&
                             (spare.When is null || Matches(spare.When, _values, region));
                if (rule.Mode == ConstraintMode.Demote && !spared) demoted = true;
            }

            return (demoted, warnings);
        }

        private List<RegimenLine> BuildCandidates(GuidelineRow baseRow, List<Modifier> modifiers, int? duration,
            int? modifierDays)
        {
            var lines = new List<RegimenLine>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var demotedIds = new HashSet<string>(StringComparer.Ordinal);
            for (var tier = 0; tier < baseRow.Candidates.Count; tier++)
            foreach (var id in baseRow.Candidates[tier])
            {
                var regimen = engine._regimens[id];
                var parts = Compose(regimen, modifiers);
                var violation = FirstViolation(parts.Select(part => part.Component).ToArray(), regimen.Components.Count == 1);
                if (violation is not null)
                {
                    Exclude(id, tier + 1, violation);
                    continue;
                }

                var key = string.Join("+", parts.Select(part => part.Component.Drug).Order(StringComparer.Ordinal));
                if (!seen.Add(key))
                {
                    _trace.Add(new TraceLine(baseRow.Id, "engine.duplicate", regimen.ToSourceRef(), id));
                    continue;
                }

                var lineId = id;
                var lineSource = regimen.ToSourceRef();
                var core = parts.Where(part => part.AddedBy is null || part.Replacing).Select(part => part.Component).ToArray();
                if (PartKey(core) != PartKey(regimen.Components))
                {
                    var rows = string.Join("+", parts.Where(part => part.Replacing).Select(part => part.AddedBy).Distinct());
                    var known = engine.RegimenFor(core);
                    lineId = known ?? $"{id}@{rows}";
                    if (known is not null) lineSource = engine._regimens[known].ToSourceRef();
                    _trace.Add(new TraceLine(rows, "engine.regimen_rewritten", lineSource, $"{id}>{lineId}"));
                }

                var (demoted, warnings) = Soft(parts);
                if (demoted) demotedIds.Add(lineId);
                foreach (var warning in warnings)
                    _trace.Add(new TraceLine(warning.RuleId, "engine.warning", warning.Source, $"{lineId}/{warning.Subject}"));
                var days = regimen.DurationDays is { } own ? Math.Max(own, modifierDays ?? 0) : duration;
                lines.Add(new RegimenLine(lineId, baseRow.Id, tier + 1, parts.Select(Line).ToArray(), days, lineSource, null)
                {
                    Warnings = warnings
                });
            }

            var ordered = lines
                .OrderBy(line => line.Tier)
                .ThenBy(line => demotedIds.Contains(line.RegimenId) ? 1 : 0)
                .ToList();
            foreach (var line in lines.Where(line => demotedIds.Contains(line.RegimenId)))
                if (ordered.Any(other => other.Tier == line.Tier && !demotedIds.Contains(other.RegimenId)))
                    _trace.Add(new TraceLine(baseRow.Id, "engine.demoted", line.Source, line.RegimenId));
            return ordered;
        }

        private static List<Part> Compose(Regimen regimen, List<Modifier> modifiers)
        {
            var parts = regimen.Components.Select(component => new Part(component, null)).ToList();
            foreach (var (row, chosen) in modifiers)
            {
                var incoming = chosen.Components
                    .Select(component => new Part(component, row.Id, row.Action == ReplaceAction))
                    .ToList();
                if (row.Action == ReplaceAction)
                {
                    var roles = incoming.Select(part => part.Component.Role).Concat(row.Roles).ToHashSet(StringComparer.Ordinal);
                    var at = parts.FindIndex(part => roles.Contains(part.Component.Role));
                    parts.RemoveAll(part => roles.Contains(part.Component.Role));
                    incoming.RemoveAll(part => parts.Any(existing => existing.Component.Drug == part.Component.Drug));
                    parts.InsertRange(at < 0 ? parts.Count : Math.Min(at, parts.Count), incoming);
                }
                else if (row.Action == AddAction)
                {
                    parts.AddRange(incoming.Where(part => parts.All(existing => existing.Component.Drug != part.Component.Drug)));
                }
            }

            return parts;
        }

        private ComponentLine Line(Part part)
        {
            var drug = engine._drugs[part.Component.Drug];
            var dose = drug.Doses.Where(item => item.Id == part.Component.Dose)
                .OrderBy(item => item.RenalBand == BaselineRenalBand ? 0 : 1)
                .First();
            return new ComponentLine(drug.Id, part.Component.Role, dose.Id, dose.Amount, dose.Amount70Kg, dose.Loading,
                dose.IntervalHours, dose.Route, part.AddedBy, engine.DoseSource(drug, dose));
        }

        private Violation? FirstViolation(IReadOnlyList<RegimenComponent> components, bool monotherapy)
        {
            foreach (var rule in _active)
            foreach (var component in components)
            {
                var drug = engine._drugs[component.Drug];
                if (Selects(rule.Exclude, drug, monotherapy)) return new Violation(rule, drug.Id);
            }

            return null;
        }

        private bool Selects(DrugSelector selector, Drug drug, bool monotherapy)
        {
            if (selector.Monotherapy == true && !monotherapy) return false;
            if (selector.Drug is { } ids && !ids.Contains(drug.Id)) return false;
            if (selector.ClassGroup is { } groups && !groups.Contains(drug.ClassGroup)) return false;
            if (selector.BetaLactamCore is { } cores && (drug.BetaLactam is null || !cores.Contains(drug.BetaLactam.Core))) return false;
            if (selector.SharesR1WithCore is { } sharedCores)
            {
                if (drug.BetaLactam is not { } lactam || sharedCores.Contains(lactam.Core)) return false;
                var groupsOfCore = engine.R1Groups(sharedCores);
                if (!groupsOfCore.Contains(lactam.R1Group) && !lactam.R1Similar.Any(groupsOfCore.Contains)) return false;
            }

            if (selector.Pregnancy is { } pregnancy && !pregnancy.Contains(drug.Pregnancy.Category)) return false;
            if (selector.QtRisk is { } qt && !qt.Contains(drug.QtRisk)) return false;
            if (selector.ClassGroupInAnswer is { } question &&
                !(_values.TryGetValue(question, out var used) && used.Contains(drug.ClassGroup))) return false;
            if (selector.License is { } licenses)
            {
                if (region is null) return false;
                var status = region.Licensing.FirstOrDefault(item => item.Drug == drug.Id)?.Status ?? UnknownLicense;
                if (!licenses.Contains(status)) return false;
            }

            return true;
        }

        private void Exclude(string regimenId, int tier, Violation violation)
        {
            if (_excluded.Any(item => item.RegimenId == regimenId)) return;
            _excluded.Add(new ExcludedRegimen(regimenId, violation.Rule.Id, tier, violation.Rule.ReasonKey, violation.DrugId,
                violation.Rule.ToSourceRef()));
            _trace.Add(new TraceLine(violation.Rule.Id, "engine.excluded", violation.Rule.ToSourceRef(), regimenId));
        }

        private IReadOnlyList<RegimenLine> Score(List<RegimenLine> lines)
        {
            if (Scorer is not { } scorer) return lines;
            var tier = lines[0].Tier;
            var peers = lines.Where(line => line.Tier == tier).ToArray();
            if (peers.Length < 2) return lines;

            var relevant = syndrome.Pathogens.Where(pathogen => !_quietPathogens.Contains(pathogen)).ToArray();
            var scores = scorer.Score(peers, engine._drugs, relevant, syndrome.Pathogens, region);
            var result = lines.ToArray();
            for (var i = 0; i < peers.Length; i++) result[i] = result[i] with { Score = scores[i] };
            return result;
        }

        private IReadOnlyList<CoverageBar> Spectrum(RegimenLine line)
        {
            var bars = new List<CoverageBar>();
            foreach (var pathogen in syndrome.Pathogens)
            {
                var best = line.Components
                    .Select(component => Scorer?.Level(component.DrugId, pathogen) ?? ("none", 0.0))
                    .MaxBy(level => level.Value);
                bars.Add(new CoverageBar(pathogen, best.Level, best.Value, _riskPathogens.Contains(pathogen)));
            }

            return bars;
        }
    }
}
