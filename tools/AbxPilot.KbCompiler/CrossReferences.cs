using System.Text.Json;
using AbxPilot.Core.Knowledge;

namespace AbxPilot.KbCompiler;

internal sealed class CrossReferences(
    List<Mapped<KbSource>> sources,
    List<Mapped<Drug>> drugs,
    List<Mapped<Pathogen>> pathogens,
    List<Mapped<Regimen>> regimens,
    List<Mapped<Question>> questions,
    List<Mapped<Syndrome>> syndromes,
    List<Mapped<GuidelineSet>> sets,
    List<Mapped<GuidelineRow>> rows,
    List<Mapped<SpectrumEntry>> spectrum,
    List<Mapped<Region>> regions,
    Mapped<Scoring>? scoring,
    List<Mapped<Constraint>> constraints,
    Dictionary<string, Dictionary<string, StringEntry>> strings,
    DiagnosticBag bag)
{
    private static readonly string[] YesNo = ["yes", "no"];
    private readonly Dictionary<string, (string File, int Line)> _requiredKeys = new(StringComparer.Ordinal);

    public void Run()
    {
        var sourceIds = Unique(sources, item => item.Id, "source");
        var drugIds = Unique(drugs, item => item.Id, "drug");
        var pathogenIds = Unique(pathogens, item => item.Id, "pathogen");
        var regimenIds = Unique(regimens, item => item.Id, "regimen");
        var questionIds = Unique(questions, item => item.Id, "question");
        Unique(syndromes, item => item.Id, "syndrome");
        Unique(sets, item => item.Id, "guideline set");
        var regionIds = Unique(regions, item => item.Id, "region");
        Unique(spectrum, item => item.Syndrome + "/" + item.Drug, "spectrum row");
        Unique(constraints, item => item.Id, "constraint");
        UniqueRows();

        var drugById = drugs.GroupBy(item => item.Model.Id).ToDictionary(group => group.Key, group => group.First().Model);
        var questionById = questions.GroupBy(item => item.Model.Id).ToDictionary(group => group.Key, group => group.First().Model);
        var resistanceIds = regions.SelectMany(item => item.Model.Resistance.Select(entry => entry.Id)).ToHashSet(StringComparer.Ordinal);

        CheckSources(sourceIds);
        CheckDrugs(drugIds);
        CheckPathogens();
        CheckRegimens(drugById);
        CheckQuestions(questionById, resistanceIds);
        var flagsBySyndrome = CheckSyndromes(pathogenIds, questionById, resistanceIds);
        CheckRows(regimenIds, questionById, flagsBySyndrome);
        CheckSpectrum(drugIds);
        CheckRegions(drugIds, pathogenIds);
        foreach (var (set, at) in sets)
            if (set.Region is { } region && !regionIds.Contains(region))
                Unknown(Codes.UnknownReference, at, "/region", $"unknown region '{region}'");
        CheckScoring();
        CheckConstraints(questionById, resistanceIds, flagsBySyndrome.Values.SelectMany(item => item).ToHashSet(StringComparer.Ordinal));
        CheckStrings();
    }

    private HashSet<string> Unique<T>(List<Mapped<T>> items, Func<T, string> key, string kind)
    {
        var seen = new Dictionary<string, SourceRecord>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            var id = key(item.Model);
            if (seen.TryGetValue(id, out var first))
            {
                bag.Error(Codes.Duplicate, item.Source.File, item.Source.Line,
                    $"{kind} '{id}' is already defined in {Path.GetFileName(first.File)}:{first.Line}");
                continue;
            }

            seen[id] = item.Source;
        }

        return seen.Keys.ToHashSet(StringComparer.Ordinal);
    }

    private void UniqueRows()
    {
        var seen = new Dictionary<string, SourceRecord>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            var key = row.Model.Set + "/" + row.Model.Id;
            if (seen.TryGetValue(key, out var first))
                bag.Error(Codes.Duplicate, row.Source.File, row.Source.Line,
                    $"row '{row.Model.Id}' is already defined on line {first.Line}");
            else
                seen[key] = row.Source;
        }
    }

    private void Require(string key, SourceRecord at, string pointer = "")
    {
        _requiredKeys.TryAdd(key, (at.File, at.LineOf(pointer)));
    }

    private void Unknown(string code, SourceRecord at, string pointer, string message) =>
        bag.Error(code, at.File, at.LineOf(pointer), $"{(pointer.Length == 0 ? "/" : pointer)}: {message}");

    private void SourceExists(HashSet<string> sourceIds, string source, SourceRecord at, string pointer)
    {
        if (!sourceIds.Contains(source))
            Unknown(Codes.UnknownReference, at, pointer, $"unknown source '{source}'");
    }

    private void CheckSources(HashSet<string> sourceIds)
    {
        void Record(KbRecord record, SourceRecord at, string pointer)
        {
            SourceExists(sourceIds, record.Source, at, pointer + "/source");
            Require("review." + record.ReviewStatus, at, pointer + "/review_status");
        }

        foreach (var item in drugs)
        {
            Record(item.Model, item.Source, "");
            for (var i = 0; i < item.Model.Doses.Count; i++)
                SourceExists(sourceIds, item.Model.Doses[i].Source, item.Source, $"/doses/{i}/source");
            if (item.Model.Renal is { } renal)
                SourceExists(sourceIds, renal.Source, item.Source, "/renal/source");
        }

        foreach (var item in pathogens) Record(item.Model, item.Source, "");
        foreach (var item in regimens) Record(item.Model, item.Source, "");
        foreach (var item in questions) Record(item.Model, item.Source, "");
        foreach (var item in syndromes) Record(item.Model, item.Source, "");
        foreach (var item in sets) Record(item.Model, item.Source, "");
        foreach (var item in rows) Record(item.Model, item.Source, "");
        foreach (var item in spectrum) Record(item.Model, item.Source, "");
        foreach (var item in regions)
        {
            Record(item.Model, item.Source, "");
            for (var i = 0; i < item.Model.Resistance.Count; i++)
                Record(item.Model.Resistance[i], item.Source, $"/resistance/{i}");
            for (var i = 0; i < item.Model.Thresholds.Count; i++)
                Record(item.Model.Thresholds[i], item.Source, $"/thresholds/{i}");
        }

        if (scoring is not null) Record(scoring.Model, scoring.Source, "");
        foreach (var item in constraints) Record(item.Model, item.Source, "");
    }

    private void CheckDrugs(HashSet<string> drugIds)
    {
        var groups = drugs.Where(item => item.Model.BetaLactam is not null)
            .Select(item => item.Model.BetaLactam!.R1Group)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var (drug, at) in drugs)
        {
            Require($"drug.{drug.Id}.name", at);
            Require("class." + drug.Class, at, "/class");
            Require("aware." + drug.Aware, at, "/aware");
            Require("bioavailability." + drug.OralBioavailability, at, "/oral_bioavailability");
            Require("pregnancy." + drug.Pregnancy.Category, at, "/pregnancy/category");
            if (drug.Pregnancy.NoteKey is { } note) Require(note, at, "/pregnancy/note_key");
            Require("qt." + drug.QtRisk, at, "/qt_risk");
            for (var i = 0; i < drug.AdverseEffects.Count; i++) Require("adverse." + drug.AdverseEffects[i], at, $"/adverse_effects/{i}");
            for (var i = 0; i < drug.Interactions.Count; i++) Require("interaction." + drug.Interactions[i], at, $"/interactions/{i}");
            foreach (var route in drug.Routes) Require("route." + route, at, "/routes");

            var isBetaLactam = drug.ClassGroup == "beta_lactam";
            if (isBetaLactam != drug.BetaLactam is not null)
                Unknown(Codes.Model, at, "/beta_lactam",
                    isBetaLactam ? "a beta-lactam needs its side-chain group" : "only beta-lactams carry a side-chain group");
            if (drug.BetaLactam is { } lactam)
            {
                Require("r1." + lactam.R1Group, at, "/beta_lactam/r1_group");
                for (var i = 0; i < lactam.R1Similar.Count; i++)
                    if (!groups.Contains(lactam.R1Similar[i]))
                        Unknown(Codes.UnknownReference, at, $"/beta_lactam/r1_similar/{i}", $"unknown side-chain group '{lactam.R1Similar[i]}'");
            }

            var doseIds = new HashSet<string>(StringComparer.Ordinal);
            var baseline = drug.Doses.Where(dose => dose.RenalBand == "normal").Select(dose => dose.Id).ToHashSet(StringComparer.Ordinal);
            for (var i = 0; i < drug.Doses.Count; i++)
            {
                var dose = drug.Doses[i];
                if (!doseIds.Add(dose.Id + "/" + dose.RenalBand))
                    Unknown(Codes.Duplicate, at, $"/doses/{i}/id", $"dose '{dose.Id}' is defined twice for renal band '{dose.RenalBand}'");
                if (!baseline.Contains(dose.Id))
                    Unknown(Codes.Model, at, $"/doses/{i}/renal_band", $"dose '{dose.Id}' has no normal renal band entry");
                if (!drug.Routes.Contains(dose.Route))
                    Unknown(Codes.Model, at, $"/doses/{i}/route", $"route '{dose.Route}' is not in the drug's routes");
                Require("indication." + dose.Indication, at, $"/doses/{i}/indication");
            }
        }

        _ = drugIds;
    }

    private void CheckPathogens()
    {
        foreach (var (pathogen, at) in pathogens)
        {
            Require($"pathogen.{pathogen.Id}.name", at);
            Require("pathogen_group." + pathogen.Group, at, "/group");
            for (var i = 0; i < pathogen.ResistanceMechanisms.Count; i++)
                Require("resistance." + pathogen.ResistanceMechanisms[i], at, $"/resistance_mechanisms/{i}");
        }
    }

    private void CheckRegimens(Dictionary<string, Drug> drugById)
    {
        foreach (var (regimen, at) in regimens)
        {
            var roles = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < regimen.Components.Count; i++)
            {
                var component = regimen.Components[i];
                Require("role." + component.Role, at, $"/components/{i}/role");
                if (!roles.Add(component.Role))
                    Unknown(Codes.Model, at, $"/components/{i}/role", $"role '{component.Role}' appears twice in one regimen");
                if (!drugById.TryGetValue(component.Drug, out var drug))
                {
                    Unknown(Codes.UnknownDrug, at, $"/components/{i}/drug", $"unknown drug '{component.Drug}'");
                    continue;
                }

                if (drug.Doses.All(dose => dose.Id != component.Dose))
                    Unknown(Codes.UnknownDose, at, $"/components/{i}/dose", $"drug '{drug.Id}' has no dose '{component.Dose}'");
            }
        }
    }

    private void CheckQuestions(Dictionary<string, Question> questionById, HashSet<string> resistanceIds)
    {
        foreach (var (question, at) in questions)
        {
            Require($"question.{question.Id}.label", at);
            foreach (var option in question.Options) Require($"question.{question.Id}.option.{option}", at, "/options");

            var defaults = question.Default.ValueKind == JsonValueKind.Array
                ? question.Default.EnumerateArray().Select(item => item.GetString() ?? "").ToArray()
                : [question.Default.GetString() ?? ""];
            foreach (var value in defaults)
                if (!question.Options.Contains(value))
                    Unknown(Codes.InvalidOption, at, "/default", $"default '{value}' is not an option of '{question.Id}'");

            if (question.DefaultFrom is { } from && !resistanceIds.Contains(from))
                Unknown(Codes.UnknownReference, at, "/default_from", $"no region resistance entry '{from}'");

            if (question.VisibleWhen is { } condition)
                CheckCondition(condition, at, "/visible_when", questionById, new HashSet<string>(StringComparer.Ordinal), resistanceIds);
        }
    }

    private void CheckCondition(Condition condition, SourceRecord at, string pointer,
        Dictionary<string, Question> questionById, HashSet<string> flags, HashSet<string> resistanceIds)
    {
        if (condition.All is { } all)
            for (var i = 0; i < all.Count; i++) CheckCondition(all[i], at, $"{pointer}/all/{i}", questionById, flags, resistanceIds);
        if (condition.Any is { } any)
            for (var i = 0; i < any.Count; i++) CheckCondition(any[i], at, $"{pointer}/any/{i}", questionById, flags, resistanceIds);
        if (condition.Not is { } not) CheckCondition(not, at, pointer + "/not", questionById, flags, resistanceIds);
        if (condition.Region is { } resistance && !resistanceIds.Contains(resistance))
            Unknown(Codes.UnknownReference, at, pointer + "/region", $"no region lists resistance '{resistance}'");
        if (condition.Question is not { } id) return;

        IReadOnlyList<string> options;
        if (questionById.TryGetValue(id, out var question)) options = question.Options;
        else if (flags.Contains(id)) options = YesNo;
        else
        {
            Unknown(Codes.UnknownQuestion, at, pointer + "/question", $"unknown question '{id}'");
            return;
        }

        var values = new List<string>();
        if (condition.Eq is { } eq) values.Add(eq);
        if (condition.In is { } list) values.AddRange(list);
        if (condition.ContainsAny is { } contains) values.AddRange(contains);
        foreach (var value in values)
            if (!options.Contains(value))
                Unknown(Codes.InvalidOption, at, pointer, $"'{value}' is not an option of '{id}'");
    }

    private Dictionary<string, HashSet<string>> CheckSyndromes(HashSet<string> pathogenIds, Dictionary<string, Question> questionById, HashSet<string> resistanceIds)
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var (syndrome, at) in syndromes)
        {
            Require($"syndrome.{syndrome.Id}.name", at);
            for (var i = 0; i < syndrome.Pathogens.Count; i++)
                if (!pathogenIds.Contains(syndrome.Pathogens[i]))
                    Unknown(Codes.UnknownReference, at, $"/pathogens/{i}", $"unknown pathogen '{syndrome.Pathogens[i]}'");
            for (var i = 0; i < syndrome.Questions.Count; i++)
                if (!questionById.ContainsKey(syndrome.Questions[i]))
                    Unknown(Codes.UnknownQuestion, at, $"/questions/{i}", $"unknown question '{syndrome.Questions[i]}'");

            var flags = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < syndrome.Derived.Count; i++)
            {
                var flag = syndrome.Derived[i];
                if (questionById.ContainsKey(flag.Id) || !flags.Add(flag.Id))
                    Unknown(Codes.Duplicate, at, $"/derived/{i}/id", $"flag '{flag.Id}' clashes with another name");
                CheckCondition(flag.When, at, $"/derived/{i}/when", questionById, flags, resistanceIds);
                if (flag.Pathogen is { } pathogen && !syndrome.Pathogens.Contains(pathogen))
                    Unknown(Codes.UnknownReference, at, $"/derived/{i}/pathogen", $"pathogen '{pathogen}' is not a pathogen of '{syndrome.Id}'");
            }

            result[syndrome.Id] = flags;
        }

        return result;
    }

    private void CheckRows(HashSet<string> regimenIds, Dictionary<string, Question> questionById,
        Dictionary<string, HashSet<string>> flagsBySyndrome)
    {
        var setIds = sets.Select(item => item.Model.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var set in sets)
        {
            Require($"guideline.{set.Model.Id}.name", set.Source);
            if (set.Model.Extends is not { } parent) continue;
            if (!setIds.Contains(parent))
                Unknown(Codes.UnknownReference, set.Source, "/extends", $"unknown parent set '{parent}'");
            var seen = new HashSet<string>(StringComparer.Ordinal) { set.Model.Id };
            for (var current = parent; current is not null; current = sets.FirstOrDefault(item => item.Model.Id == current)?.Model.Extends)
                if (!seen.Add(current))
                {
                    Unknown(Codes.Model, set.Source, "/extends", $"set '{set.Model.Id}' inherits from itself");
                    break;
                }
        }

        foreach (var (row, at) in rows)
        {
            Require(row.RationaleKey, at);
            foreach (var role in row.Roles) Require("role." + role, at);
            var syndrome = syndromes.FirstOrDefault(item => item.Model.Id == row.Syndrome)?.Model;
            if (syndrome is null)
            {
                Unknown(Codes.UnknownReference, at, "/syndrome", $"no syndrome '{row.Syndrome}' for this table");
                continue;
            }

            var flags = flagsBySyndrome.GetValueOrDefault(row.Syndrome) ?? [];
            foreach (var (column, values) in row.Conditions)
            {
                IReadOnlyList<string> options;
                if (syndrome.Questions.Contains(column) && questionById.TryGetValue(column, out var question)) options = question.Options;
                else if (flags.Contains(column)) options = YesNo;
                else
                {
                    Unknown(Codes.UnknownQuestion, at, "", $"column '{column}' is neither a question of '{row.Syndrome}' nor a derived flag");
                    continue;
                }

                foreach (var value in values)
                    if (!options.Contains(value))
                        Unknown(Codes.InvalidOption, at, "", $"column '{column}': '{value}' is not a valid option");
            }

            foreach (var tier in row.Candidates)
            foreach (var id in tier)
            {
                if (!regimenIds.Contains(id))
                {
                    Unknown(Codes.UnknownReference, at, "", $"candidate regimen '{id}' does not exist");
                    continue;
                }
            }
        }
    }

    private void CheckSpectrum(HashSet<string> drugIds)
    {
        foreach (var (entry, at) in spectrum)
        {
            var syndrome = syndromes.FirstOrDefault(item => item.Model.Id == entry.Syndrome)?.Model;
            if (syndrome is null)
                Unknown(Codes.UnknownReference, at, "", $"no syndrome '{entry.Syndrome}' for this spectrum file");
            if (!drugIds.Contains(entry.Drug))
                Unknown(Codes.UnknownDrug, at, "", $"unknown drug '{entry.Drug}'");
            foreach (var (pathogen, level) in entry.Coverage)
            {
                if (syndrome is not null && !syndrome.Pathogens.Contains(pathogen))
                    Unknown(Codes.UnknownReference, at, "", $"pathogen column '{pathogen}' is not a pathogen of '{entry.Syndrome}'");
                Require("spectrum." + level, at);
            }

            if (syndrome is not null)
                foreach (var pathogen in syndrome.Pathogens.Where(id => !entry.Coverage.ContainsKey(id)))
                    Unknown(Codes.Model, at, "", $"spectrum row '{entry.Drug}' has no column for '{pathogen}'");
        }

        var covered = spectrum.Select(item => item.Model.Drug).ToHashSet(StringComparer.Ordinal);
        foreach (var (drug, at) in drugs)
            if (!covered.Contains(drug.Id))
                bag.Warning(Codes.Coverage, at.File, at.Line, $"drug '{drug.Id}' has no spectrum row");

        var regimenById = regimens.GroupBy(item => item.Model.Id).ToDictionary(group => group.Key, group => group.First().Model);
        var bySyndrome = spectrum.Select(item => (item.Model.Syndrome, item.Model.Drug)).ToHashSet();
        foreach (var (row, at) in rows)
        foreach (var id in row.Candidates.SelectMany(tier => tier))
        {
            if (!regimenById.TryGetValue(id, out var regimen)) continue;
            foreach (var component in regimen.Components)
                if (bySyndrome.Add((row.Syndrome, component.Drug)))
                    bag.Warning(Codes.Coverage, at.File, at.Line,
                        $"drug '{component.Drug}' of regimen '{id}' has no spectrum row for '{row.Syndrome}'");
        }
    }

    private void CheckRegions(HashSet<string> drugIds, HashSet<string> pathogenIds)
    {
        foreach (var (region, at) in regions)
        {
            Require($"region.{region.Id}.name", at);
            for (var i = 0; i < region.Resistance.Count; i++)
            {
                var entry = region.Resistance[i];
                if (!pathogenIds.Contains(entry.Pathogen))
                    Unknown(Codes.UnknownReference, at, $"/resistance/{i}/pathogen", $"unknown pathogen '{entry.Pathogen}'");
            }

            for (var i = 0; i < region.Thresholds.Count; i++)
                Require($"threshold.{region.Thresholds[i].Id}", at, $"/thresholds/{i}");

            var listed = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < region.Licensing.Count; i++)
            {
                var license = region.Licensing[i];
                if (!drugIds.Contains(license.Drug))
                    Unknown(Codes.UnknownDrug, at, $"/licensing/{i}/drug", $"unknown drug '{license.Drug}'");
                if (!listed.Add(license.Drug))
                    Unknown(Codes.Duplicate, at, $"/licensing/{i}/drug", $"drug '{license.Drug}' is listed twice");
                Require("license." + license.Status, at, $"/licensing/{i}/status");
            }

            foreach (var drug in drugIds.Where(id => !listed.Contains(id)))
                bag.Warning(Codes.Coverage, at.File, at.Line, $"region '{region.Id}' does not state the licensing of '{drug}'");
        }
    }

    private void CheckScoring()
    {
        if (scoring is null) return;
        var (model, at) = scoring;
        var sum = model.Components.Sum(component => component.Weight);
        if (Math.Abs(sum - 1.0) > 1e-9)
            Unknown(Codes.Scoring, at, "/components", $"weights sum to {sum:0.###}, expected 1");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < model.Components.Count; i++)
        {
            if (!seen.Add(model.Components[i].Id))
                Unknown(Codes.Duplicate, at, $"/components/{i}/id", $"component '{model.Components[i].Id}' is listed twice");
            Require("scoring." + model.Components[i].Id, at, $"/components/{i}/id");
        }
    }

    private void CheckConstraints(Dictionary<string, Question> questionById, HashSet<string> resistanceIds, HashSet<string> flags)
    {
        foreach (var (constraint, at) in constraints)
        {
            Require(constraint.ReasonKey, at, "/reason_key");
            if (constraint.When is { } when)
                CheckCondition(when, at, "/when", questionById, flags, resistanceIds);
            if (constraint.Spare?.When is { } spare)
                CheckCondition(spare, at, "/spare/when", questionById, flags, resistanceIds);
            foreach (var drug in constraint.Exclude.Drug ?? [])
                if (!drugs.Any(item => item.Model.Id == drug))
                    Unknown(Codes.UnknownDrug, at, "/exclude/drug", $"unknown drug '{drug}'");
            if (constraint.Exclude.ClassGroupInAnswer is { } answer && !questionById.ContainsKey(answer))
                Unknown(Codes.UnknownQuestion, at, "/exclude/class_group_in_answer", $"unknown question '{answer}'");
        }
    }

    private void CheckStrings()
    {
        if (!strings.TryGetValue(KbCompilation.ReferenceLanguage, out var reference)) return;

        var all = new Dictionary<string, (string File, int Line)>(_requiredKeys, StringComparer.Ordinal);
        foreach (var (_, table) in strings)
        foreach (var (key, entry) in table)
            all.TryAdd(key, (entry.File, entry.Line));

        foreach (var (key, (file, line)) in all.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            var required = _requiredKeys.ContainsKey(key);
            if (!reference.ContainsKey(key))
                bag.Error(Codes.MissingTr, file, line,
                    $"i18n key '{key}' is missing in '{KbCompilation.ReferenceLanguage}'{(required ? "" : " but exists in another language")}");

            foreach (var (language, table) in strings)
            {
                if (language == KbCompilation.ReferenceLanguage || table.ContainsKey(key)) continue;
                bag.Warning(Codes.MissingEn, file, line, $"i18n key '{key}' is missing in '{language}'");
            }
        }
    }
}
