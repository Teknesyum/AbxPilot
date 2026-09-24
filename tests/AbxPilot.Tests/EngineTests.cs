using AbxPilot.Core;
using AbxPilot.Core.Engine;
using AbxPilot.Core.Knowledge;
using AbxPilot.Data;
using AbxPilot.Tests.Golden;

namespace AbxPilot.Tests;

public sealed class EngineTests
{
    private const string Idsa = "idsa-ats-2019";
    private static readonly KnowledgeBase Kb = KbResources.Knowledge();
    private static readonly GuidelineEngine Engine = new(Kb);

    private static GuidelineContext Cap(params (string Question, string[] Values)[] answers) =>
        GuidelineContext.Create("cap", "tr", Idsa, answers);

    [Fact]
    public void HiddenAnswerIsIgnoredAndVisibilityIsReturned()
    {
        var result = Engine.Evaluate(Cap(("setting", ["outpatient"]), ("severe_vasopressor", ["yes"])));

        var hidden = result.Questions.Single(state => state.QuestionId == "severe_vasopressor");
        Assert.False(hidden.Visible);
        Assert.Empty(hidden.Value);
        Assert.Equal(AnswerOrigin.Hidden, hidden.Origin);
        Assert.Contains(result.Trace, line => line.MessageKey == "engine.hidden_ignored" && line.RuleId == "severe_vasopressor");
        Assert.Equal("out_healthy_high", result.MatchedRuleId);
        Assert.Contains("macrolide_resistance", result.VisibleQuestions);
        Assert.DoesNotContain("recent_hosp_iv", result.VisibleQuestions);

        var ward = Engine.Evaluate(Cap(("setting", ["ward"])));
        Assert.Contains("severe_vasopressor", ward.VisibleQuestions);
        Assert.DoesNotContain("macrolide_resistance", ward.VisibleQuestions);
    }

    [Fact]
    public void RegionSuppliesTheDefault()
    {
        var state = Engine.Evaluate(Cap()).Questions.Single(item => item.QuestionId == "macrolide_resistance");
        Assert.Equal(AnswerOrigin.RegionDefault, state.Origin);
        Assert.Equal(["ge25"], state.Value);

        var unknown = Engine.Evaluate(GuidelineContext.Create("cap", "zz", Idsa));
        Assert.Equal(AnswerOrigin.Default, unknown.Questions.Single(item => item.QuestionId == "macrolide_resistance").Origin);
        Assert.Contains(unknown.Trace, line => line.MessageKey == "engine.region_missing");
    }

    [Fact]
    public void InvalidAnswerFallsBackToDefault()
    {
        var result = Engine.Evaluate(Cap(("setting", ["moon"])));
        Assert.Equal(AnswerOrigin.Default, result.Questions.Single(item => item.QuestionId == "setting").Origin);
        Assert.Contains(result.Trace, line => line.MessageKey == "engine.invalid_answer");
    }

    [Fact]
    public void UnknownSyndromeOrSetThrows()
    {
        Assert.Throws<ArgumentException>(() => Engine.Evaluate(GuidelineContext.Create("meningitis", "tr", Idsa)));
        Assert.Throws<ArgumentException>(() => Engine.Evaluate(GuidelineContext.Create("cap", "tr", "nope")));
    }

    [Fact]
    public void ScoreNeverChangesTheSelection()
    {
        var reversed = Kb with
        {
            Scoring = Kb.Scoring! with
            {
                Components = Kb.Scoring.Components.Select((item, index) => item with { Weight = index % 2 == 0 ? 0 : 1 }).ToArray()
            }
        };
        var unscored = new GuidelineEngine(Kb with { Scoring = null });
        var other = new GuidelineEngine(reversed);

        foreach (var item in GoldenCase.All())
        foreach (var set in GoldenCase.SetsFor(item.Syndrome))
        {
            var context = item.Context(set);
            var baseline = Order(Engine.Evaluate(context));
            Assert.Equal(baseline, Order(other.Evaluate(context)));
            Assert.Equal(baseline, Order(unscored.Evaluate(context)));
        }

        static string[] Order(Recommendation result) =>
            new[] { result.FirstChoice }.Concat(result.Alternatives).Select(line => line?.Key ?? "-").ToArray();
    }

    [Fact]
    public void ScoreComparesOnlyTheFirstTier()
    {
        var result = Engine.Evaluate(Cap(("setting", ["ward"])));
        var first = result.FirstChoice!;
        Assert.NotNull(first.Score);
        Assert.Equal(first.Score!.Components.Sum(item => item.Contribution), first.Score.Total, 6);
        Assert.Equal(Kb.Scoring!.Components.Select(item => item.Id), first.Score.Components.Select(item => item.Id));
        Assert.All(result.Alternatives, line => Assert.Equal(line.Tier == first.Tier, line.Score is not null));
        Assert.Contains(result.Alternatives, line => line.Score is null);
    }

    [Fact]
    public void RegionThresholdLeafDrivesAConstraint()
    {
        var rule = Kb.Constraints[0] with
        {
            Id = "region_probe",
            When = new Condition { Region = "spn_macrolide", In = ["ge25"] },
            Exclude = new DrugSelector { BetaLactamCore = ["penicillin"] }
        };
        var probe = Kb with { Constraints = [rule] };
        var high = new GuidelineEngine(probe).Evaluate(Cap(("setting", ["outpatient"])));
        Assert.Equal("dox_po", high.FirstChoice!.RegimenId);
        Assert.Contains(high.Excluded, item => item.RegimenId == "amx_po" && item.RuleId == "region_probe");

        var region = Kb.Regions.Single(item => item.Id == "tr");
        var low = region with
        {
            Resistance = region.Resistance.Select(entry => entry.Id == "spn_macrolide" ? entry with { Category = "lt25" } : entry).ToArray()
        };
        var calm = new GuidelineEngine(probe with { Regions = [low] }).Evaluate(Cap(("setting", ["outpatient"])));
        Assert.Equal("amx_po", calm.FirstChoice!.RegimenId);
    }

    [Fact]
    public void NoCandidateAsksForASpecialist()
    {
        var result = Engine.Evaluate(Cap(("setting", ["ward"]), ("qt_risk", ["yes"]), ("pregnancy", ["yes"])));
        Assert.Equal(RecommendationStatus.NoCandidateLeft, result.Status);
        Assert.True(result.ConsultSpecialist);
        Assert.Null(result.FirstChoice);
        Assert.Contains(result.Rationale, line => line.MessageKey == "engine.consult_specialist");
    }

    [Fact]
    public void NoRowAsksForASpecialist()
    {
        var empty = new GuidelineEngine(Kb with { GuidelineRows = [] });
        var result = empty.Evaluate(Cap());
        Assert.Equal(RecommendationStatus.NoGuidelineRow, result.Status);
        Assert.Contains(result.Rationale, line => line.MessageKey == "engine.consult_specialist");
    }

    [Fact]
    public void RiskFlagsMarkTheSpectrum()
    {
        var result = Engine.Evaluate(Cap(("setting", ["ward"]), ("prior_mrsa", ["yes"])));
        Assert.Equal(["mrsa"], result.RiskFlags);
        var bar = result.Spectrum.Single(item => item.PathogenId == "mrsa");
        Assert.True(bar.AtRisk);
        Assert.True(bar.Coverage > 0);
        Assert.Equal(7, result.FirstChoice!.DurationDays);
        Assert.Contains(result.FirstChoice.Components, item => item.AddedByRule == "mrsa_prior");
    }

    [Fact]
    public void EveryMessageKeyIsTranslated()
    {
        foreach (var language in KbResources.Languages)
        {
            var strings = KbResources.Strings(language);
            foreach (var item in GoldenCase.All())
            foreach (var set in GoldenCase.SetsFor(item.Syndrome))
            {
                var result = Engine.Evaluate(item.Context(set));
                var keys = result.Trace.Concat(result.Rationale).Select(line => line.MessageKey)
                    .Concat(result.Excluded.Select(line => line.ReasonKey))
                    .Concat(result.Alternatives.Prepend(result.FirstChoice).OfType<RegimenLine>()
                        .SelectMany(line => line.Warnings).Select(line => line.MessageKey));
                Assert.All(keys, key => Assert.True(strings.ContainsKey(key), $"{language}: {key}"));
            }
        }
    }

    [Fact]
    public void UnknownLicenseKeepsTheDrugWithAWarning()
    {
        var region = Kb.Regions.Single(item => item.Id == "tr");
        var unlisted = region with { Licensing = region.Licensing.Where(item => item.Drug != "amoxicillin").ToArray() };
        var result = new GuidelineEngine(Kb with { Regions = [unlisted] }).Evaluate(Cap(("setting", ["outpatient"])));
        Assert.Equal("amx_po", result.FirstChoice!.RegimenId);
        Assert.Contains(result.FirstChoice.Warnings, line => line.RuleId == "license_unknown" && line.Subject == "amoxicillin");
        Assert.DoesNotContain(result.Excluded, item => item.RegimenId == "amx_po");

        var licensed = Engine.Evaluate(Cap(("setting", ["outpatient"])));
        Assert.Empty(licensed.FirstChoice!.Warnings);
    }

    [Fact]
    public void IgeAllergyWarnsOnOtherBetaLactamsAndScarExcludesThem()
    {
        var ige = Engine.Evaluate(Cap(("setting", ["ward"]), ("pen_allergy", ["ige"])));
        Assert.Equal("cro_azm_iv", ige.FirstChoice!.RegimenId);
        var warning = Assert.Single(ige.FirstChoice.Warnings, line => line.RuleId == "allergy_ige_other_beta_lactam");
        Assert.Equal("aaaai-acaai-2022", warning.Source.Source);
        Assert.Contains(ige.Trace, line => line.MessageKey == "engine.warning" && line.RuleId == "allergy_ige_other_beta_lactam");

        var scar = Engine.Evaluate(Cap(("setting", ["ward"]), ("pen_allergy", ["scar"])));
        Assert.All(scar.Excluded, item => Assert.Equal("aaaai-acaai-2022", item.Source.Source));
        Assert.DoesNotContain(scar.FirstChoice!.Components, line => line.DrugId == "ceftriaxone");
    }

    [Fact]
    public void RecentClassDemotesButKeepsTheInpatientBackbone()
    {
        var outpatient = Engine.Evaluate(Cap(("setting", ["outpatient"]), ("comorbidity", ["lung"]), ("recent_abx_class", ["beta_lactam"])));
        Assert.Equal("lvx_po", outpatient.FirstChoice!.RegimenId);
        Assert.Contains(outpatient.Alternatives, line => line.RegimenId == "amc_azm_po" && line.Warnings.Any(item => item.RuleId == "recent_same_class"));
        Assert.Contains(outpatient.Trace, line => line.MessageKey == "engine.demoted" && line.Subject == "amc_azm_po");

        var ward = Engine.Evaluate(Cap(("setting", ["ward"]), ("recent_abx_class", ["beta_lactam"])));
        Assert.Equal("sam_azm_iv", ward.FirstChoice!.RegimenId);
        Assert.Contains(ward.FirstChoice.Warnings, line => line.RuleId == "recent_same_class");
    }

    [Fact]
    public void ReplacedRegimenIsRenamedAndTraced()
    {
        var result = Engine.Evaluate(Cap(("setting", ["ward"]), ("pen_allergy", ["ige"]), ("prior_pseudomonas", ["yes"])));
        Assert.Equal("fep_azm_iv", result.FirstChoice!.RegimenId);
        Assert.Equal(["cefepime", "azithromycin"], result.FirstChoice.DrugIds);
        Assert.Contains(result.Trace, line => line.MessageKey == "engine.regimen_rewritten" && line.Subject == "sam_azm_iv>fep_azm_iv");
    }

    [Fact]
    public void EveryLineCarriesItsSource()
    {
        var result = Engine.Evaluate(Cap(("setting", ["ward"]), ("pen_allergy", ["ige"])));
        Assert.All(result.Trace, line => Assert.False(string.IsNullOrEmpty(line.Source.Source)));
        Assert.All(result.Excluded, line => Assert.Equal(line.RuleId, line.Source.Id));
        Assert.All(result.FirstChoice!.Components, line => Assert.StartsWith(line.DrugId + ".", line.Source.Id));
    }
}
