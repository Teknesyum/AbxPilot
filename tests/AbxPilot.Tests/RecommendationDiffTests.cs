using AbxPilot.Core;
using AbxPilot.Core.Engine;
using AbxPilot.Data;

namespace AbxPilot.Tests;

public sealed class RecommendationDiffTests
{
    private static readonly GuidelineEngine Engine = new(KbResources.Knowledge());
    private static readonly GuidelineContext Ward = GuidelineContext.Create("cap", "tr", "idsa-ats-2019", ("setting", ["ward"]));

    [Fact]
    public void SameAnswersGiveAnEmptyDiff()
    {
        var diff = RecommendationDiff.Between(Engine.Evaluate(Ward), Engine.Evaluate(Ward));
        Assert.True(diff.IsEmpty);
        Assert.Empty(diff.ChangedAnswers);
    }

    [Fact]
    public void AddedDrugChangesChoiceDurationAndSpectrum()
    {
        var diff = RecommendationDiff.Between(Engine.Evaluate(Ward), Engine.Evaluate(Ward.WithAnswer("prior_mrsa", "yes")));
        Assert.True(diff.FirstChoiceChanged);
        Assert.True(diff.DurationChanged);
        Assert.Equal(["prior_mrsa"], diff.ChangedAnswers.Where(id => id == "prior_mrsa"));
        Assert.Contains(diff.SpectrumChanges, change => change.PathogenId == "mrsa");
        Assert.Empty(diff.DoseChanges);
    }

    [Fact]
    public void ConstraintShowsNewlyExcludedRegimens()
    {
        var before = Engine.Evaluate(Ward);
        var after = Engine.Evaluate(Ward.WithAnswer("qt_risk", "yes"));
        var diff = RecommendationDiff.Between(before, after);
        Assert.Equal(8, diff.NewlyExcluded.Count);
        Assert.NotEmpty(diff.AlternativesRemoved);
        Assert.Empty(RecommendationDiff.Between(after, before).NewlyExcluded);
        Assert.Equal(8, RecommendationDiff.Between(after, before).NoLongerExcluded.Count);
        Assert.Contains("qt_risk", diff.ChangedAnswers);
    }

    [Fact]
    public void SettingChangeShowsAndHidesQuestions()
    {
        var outpatient = Engine.Evaluate(Ward.WithAnswer("setting", "outpatient"));
        var diff = RecommendationDiff.Between(outpatient, Engine.Evaluate(Ward));
        Assert.Contains("severe_vasopressor", diff.QuestionsShown);
        Assert.Contains("macrolide_resistance", diff.QuestionsHidden);
        Assert.Contains("setting", diff.ChangedAnswers);
    }
}
