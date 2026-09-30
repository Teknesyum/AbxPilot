using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AbxPilot.UI.ViewModels;

public sealed partial class SyndromeItem : ObservableObject
{
    public SyndromeItem(string id) => Id = id;

    public string Id { get; }

    public string Terms { get; set; } = "";

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isMatch = true;
}

public sealed partial class ChoiceOption : ObservableObject
{
    public ChoiceOption(string owner, string code)
    {
        Owner = owner;
        Code = code;
    }

    public string Owner { get; }

    public string Code { get; }

    public bool IsMulti { get; init; }

    [ObservableProperty]
    private string label = "";

    [ObservableProperty]
    private bool isSelected;
}

public sealed partial class ComponentSlot : ObservableObject
{
    [ObservableProperty]
    private string drugId = "";

    [ObservableProperty]
    private string drugName = "";

    [ObservableProperty]
    private string role = "";

    [ObservableProperty]
    private string amount = "";

    [ObservableProperty]
    private string schedule = "";

    [ObservableProperty]
    private string route = "";

    [ObservableProperty]
    private string extra = "";

    [ObservableProperty]
    private bool hasExtra;

    [ObservableProperty]
    private bool showName;
}

public sealed record TextRow(string Text, string Detail)
{
    public bool HasDetail => Detail.Length > 0;
}

public sealed record ScorePart(string Name, string Formula);

public sealed partial class AlternativeRow : ObservableObject
{
    public AlternativeRow(string key, string regimenId)
    {
        Key = key;
        RegimenId = regimenId;
    }

    public string Key { get; }

    public string RegimenId { get; }

    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private string detail = "";

    [ObservableProperty]
    private string scoreTotal = "";

    [ObservableProperty]
    private bool hasScore;

    [ObservableProperty]
    private bool showScore;

    [ObservableProperty]
    private bool isLeaving;

    [ObservableProperty]
    private string leavingReason = "";

    [ObservableProperty]
    private IReadOnlyList<ScorePart> scoreParts = [];

    public bool ScoreInline => HasScore && ShowScore;

    partial void OnHasScoreChanged(bool value) => OnPropertyChanged(nameof(ScoreInline));

    partial void OnShowScoreChanged(bool value) => OnPropertyChanged(nameof(ScoreInline));
}

public sealed partial class ExcludedRow : ObservableObject
{
    public ExcludedRow(string regimenId) => RegimenId = regimenId;

    public string RegimenId { get; }

    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private string reason = "";
}

public sealed partial class SpectrumRow : ObservableObject
{
    public SpectrumRow(string pathogenId) => PathogenId = pathogenId;

    public string PathogenId { get; }

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string shortName = "";

    [ObservableProperty]
    private string levelText = "";

    [ObservableProperty]
    private string level = "none";

    [ObservableProperty]
    private double coverage;

    [ObservableProperty]
    private bool atRisk;

    [ObservableProperty]
    private string riskText = "";

    public bool IsReliable => Level == "reliable";

    public bool IsVariable => Level == "variable";

    public bool IsNone => Level != "reliable" && Level != "variable";

    partial void OnLevelChanged(string value)
    {
        OnPropertyChanged(nameof(IsReliable));
        OnPropertyChanged(nameof(IsVariable));
        OnPropertyChanged(nameof(IsNone));
    }
}

public sealed partial class QuestionCard : ObservableObject
{
    public QuestionCard(string id, string type)
    {
        Id = id;
        Type = type;
    }

    public string Id { get; }

    public string Type { get; }

    public bool IsMulti => Type == "multi";

    public bool IsBoolean => Type == "boolean";

    public bool IsSingle => !IsMulti && !IsBoolean;

    public bool IsWide => IsMulti || IsSingle && Options.Count > 3;

    public ObservableCollection<ChoiceOption> Options { get; } = [];

    [ObservableProperty]
    private string label = "";

    [ObservableProperty]
    private bool isVisible = true;

    [ObservableProperty]
    private bool isChanged;

    [ObservableProperty]
    private bool isOn;
}
