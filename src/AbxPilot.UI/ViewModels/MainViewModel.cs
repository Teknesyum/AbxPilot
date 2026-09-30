using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using AbxPilot.Core;
using AbxPilot.Core.Engine;
using AbxPilot.Core.Knowledge;
using AbxPilot.Data;
using AbxPilot.UI.Localization;
using AbxPilot.UI.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbxPilot.UI.ViewModels;

public enum ScreenState
{
    Loading,
    Empty,
    Ready,
    Consult,
    Referral,
    NoAntibiotic,
    Error
}

public sealed class AppliedEventArgs : EventArgs
{
    public AppliedEventArgs(RecommendationDiff? diff, string? trigger)
    {
        Diff = diff;
        Trigger = trigger;
    }

    public RecommendationDiff? Diff { get; }

    public string? Trigger { get; }
}

public sealed record AlternativesPage(MainViewModel Owner);

public sealed record RationalePage(MainViewModel Owner);

public sealed partial class MainViewModel : ObservableObject
{
    public const string DefaultRegion = "tr";

    public const string OtherRegion = "other";

    private readonly Func<KnowledgeBase> _load;
    private readonly ISettingsStore _store;
    private readonly Dictionary<string, IReadOnlyList<string>> _answers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<string, IReadOnlyList<string>>> _memory = new(StringComparer.Ordinal);
    private Dictionary<string, string[]> _drugsBySyndrome = new(StringComparer.Ordinal);
    private AppSettings _settings;
    private KnowledgeBase? _knowledge;
    private GuidelineEngine? _engine;
    private Recommendation? _current;
    private int _generation;
    private string? _errorKey;
    private string _region = DefaultRegion;
    private string _set = "";
    private Dictionary<string, string[]> _setsBySyndrome = new(StringComparer.Ordinal);

    public MainViewModel() : this(KbResources.Knowledge, FileSettingsStore.ForUser())
    {
    }

    public MainViewModel(Func<KnowledgeBase> load, ISettingsStore store)
    {
        _load = load;
        _store = store;
        _settings = store.Load();

        if (_settings.Language is { } saved && Localizer.Languages.Contains(saved))
            Localizer.SetLanguage(saved);

        foreach (var code in Localizer.Languages)
            Languages.Add(new LanguageOption(code, code == Localizer.Language));

        showScore = _settings.ShowScore;
        confirmUpdate = _settings.ConfirmUpdate;
        showTip = !_settings.TipSeen;
        AlternativesTab = new AlternativesPage(this);
        RationaleTab = new RationalePage(this);
        currentTab = AlternativesTab;

        Localizer.Changed += (_, _) => Relabel();
        Idle = LoadAsync();
    }

    public event EventHandler? Applying;

    public event EventHandler<AppliedEventArgs>? Applied;

    public Task Idle { get; private set; }

    public TimeSpan LastEvaluation { get; private set; }

    public bool EvaluatedOffUiThread { get; private set; }

    public Recommendation? Current => _current;

    public ObservableCollection<LanguageOption> Languages { get; } = [];

    public ObservableCollection<SyndromeItem> Syndromes { get; } = [];

    public ObservableCollection<ChoiceOption> GuidelineSets { get; } = [];

    public ObservableCollection<ChoiceOption> Regions { get; } = [];

    public ObservableCollection<ComponentSlot> Slots { get; } = [];

    public ObservableCollection<TextRow> Warnings { get; } = [];

    public ObservableCollection<TextRow> Licenses { get; } = [];

    public ObservableCollection<TextRow> Rationale { get; } = [];

    public ObservableCollection<TextRow> TraceRows { get; } = [];

    public ObservableCollection<AlternativeRow> Alternatives { get; } = [];

    public ObservableCollection<ExcludedRow> Excluded { get; } = [];

    public ObservableCollection<SpectrumRow> Spectrum { get; } = [];

    public ObservableCollection<QuestionCard> Questions { get; } = [];

    public ObservableCollection<TextRow> History { get; } = [];

    public bool HasHistory => History.Count > 0;

    public ObservableCollection<QuestionCard> OpenQuestions { get; } = [];

    public ObservableCollection<QuestionCard> ChosenQuestions { get; } = [];

    public bool HasChosen => ChosenQuestions.Count > 0;

    public bool CanReset => _answers.Count > 0;

    public AlternativesPage AlternativesTab { get; }

    public RationalePage RationaleTab { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading), nameof(IsEmpty), nameof(IsReady), nameof(IsConsult), nameof(IsError),
        nameof(IsReferral), nameof(IsNoAntibiotic), nameof(IsCaution), nameof(HasCard), nameof(HasResult),
        nameof(ShowQuestionHint))]
    private ScreenState state = ScreenState.Loading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoMatches))]
    private string searchText = "";

    [ObservableProperty]
    private SyndromeItem? selectedSyndrome;

    [ObservableProperty]
    private string syndromeTitle = "";

    [ObservableProperty]
    private string guidelineName = "";

    [ObservableProperty]
    private string durationText = "";

    [ObservableProperty]
    private bool hasDuration;

    [ObservableProperty]
    private string sourceText = "";

    [ObservableProperty]
    private string reviewText = "";

    [ObservableProperty]
    private bool isUnreviewed;

    [ObservableProperty]
    private string consultText = "";

    [ObservableProperty]
    private string errorText = "";

    [ObservableProperty]
    private string outcomeText = "";

    [ObservableProperty]
    private string referralNote = "";

    [ObservableProperty]
    private bool hasFirstChoice;

    [ObservableProperty]
    private string guidelineSettingLabel = "";

    [ObservableProperty]
    private string licensesTitle = "";

    [ObservableProperty]
    private bool noRegionData;

    [ObservableProperty]
    private string? sourceUrl;

    [ObservableProperty]
    private string alternativesHeader = "";

    [ObservableProperty]
    private string rationaleHeader = "";

    [ObservableProperty]
    private string excludedHeader = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAlternativesTab), nameof(IsRationaleTab))]
    private object currentTab;

    [ObservableProperty]
    private bool isTraceOpen;

    [ObservableProperty]
    private bool isSettingsOpen;

    [ObservableProperty]
    private bool isDrawerOpen;

    [ObservableProperty]
    private bool isCompact;

    [ObservableProperty]
    private bool showScore;

    [ObservableProperty]
    private bool confirmUpdate;

    [ObservableProperty]
    private bool showTip;

    [ObservableProperty]
    private string settingsPath = "";

    [ObservableProperty]
    private string headline = "";

    [ObservableProperty]
    private string changeText = "";

    [ObservableProperty]
    private string noEffectText = "";

    [ObservableProperty]
    private bool isComposite;

    public bool IsLoading => State == ScreenState.Loading;

    public bool IsEmpty => State == ScreenState.Empty;

    public bool IsReady => State == ScreenState.Ready;

    public bool IsConsult => State == ScreenState.Consult;

    public bool IsError => State == ScreenState.Error;

    public bool IsReferral => State == ScreenState.Referral;

    public bool IsNoAntibiotic => State == ScreenState.NoAntibiotic;

    public bool IsCaution => IsConsult || IsReferral;

    public bool HasCard => IsReady || IsConsult || IsReferral || IsNoAntibiotic;

    public bool HasSetChoices => GuidelineSets.Count > 0;

    public bool NoSetChoices => GuidelineSets.Count == 0;

    public bool HasResult => HasCard;

    public bool ShowQuestionHint => !HasCard;

    public bool IsAlternativesTab => CurrentTab is AlternativesPage;

    public bool IsRationaleTab => CurrentTab is RationalePage;

    public bool NoMatches => Syndromes.Count > 0 && Syndromes.All(item => !item.IsMatch);

    public bool HasSyndromes => Syndromes.Count > 0;

    public bool NoSyndromes => _knowledge is not null && Syndromes.Count == 0;

    public bool HasWarnings => Warnings.Count > 0;

    public bool HasExcluded => Excluded.Count > 0;

    public bool NoAlternatives => Alternatives.Count == 0;

    public bool HasSpectrum => Spectrum.Count > 0;

    public bool NoSpectrum => HasCard && Spectrum.Count == 0;

    public string DataVersion =>
        Localizer.Format("titlebar.dataVersion", ("version", _knowledge?.Version ?? "…"));

    public bool HasDataVersion => _knowledge is not null;

    public string GuidelineSet => _set;

    public string RegionId => _region;

    private async Task LoadAsync()
    {
        State = ScreenState.Loading;
        KnowledgeBase knowledge;
        try
        {
            knowledge = await Task.Run(_load);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            Fail("error.load");
            return;
        }

        _knowledge = knowledge;
        _engine = new GuidelineEngine(knowledge);

        Syndromes.Clear();
        foreach (var syndrome in knowledge.Syndromes)
            Syndromes.Add(new SyndromeItem(syndrome.Id));

        var regimens = knowledge.Regimens.ToDictionary(regimen => regimen.Id, StringComparer.Ordinal);
        _drugsBySyndrome = knowledge.Syndromes.ToDictionary(
            syndrome => syndrome.Id,
            syndrome => knowledge.GuidelineRows
                .Where(row => row.Syndrome == syndrome.Id)
                .SelectMany(row => row.Candidates.SelectMany(tier => tier))
                .SelectMany(id => regimens.TryGetValue(id, out var regimen) ? regimen.Components.Select(item => item.Drug) : [])
                .Concat(knowledge.Spectrum.Where(entry => entry.Syndrome == syndrome.Id).Select(entry => entry.Drug))
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            StringComparer.Ordinal);
        _setsBySyndrome = knowledge.Syndromes.ToDictionary(
            syndrome => syndrome.Id,
            syndrome => knowledge.GuidelineSets
                .Where(set => knowledge.GuidelineRows.Any(row => row.Set == set.Id && row.Syndrome == syndrome.Id))
                .Select(set => set.Id)
                .ToArray(),
            StringComparer.Ordinal);
        _region = knowledge.Regions.Any(region => region.Id == _settings.Region) ? _settings.Region! : DefaultRegion;
        Regions.Clear();
        foreach (var region in knowledge.Regions
                     .OrderBy(region => region.Id == DefaultRegion ? 0 : region.Id == OtherRegion ? 2 : 1)
                     .ThenBy(region => region.Id, StringComparer.Ordinal))
            Regions.Add(new ChoiceOption("region", region.Id));
        NoRegionData = knowledge.Regions.FirstOrDefault(region => region.Id == _region)?.Resistance.Count is null or 0;
        _set = "";
        GuidelineSets.Clear();
        OnSetsChanged();

        Relabel();
        State = ScreenState.Empty;
        OnPropertyChanged(nameof(DataVersion));
        OnPropertyChanged(nameof(HasDataVersion));
        OnPropertyChanged(nameof(HasSyndromes));
        OnPropertyChanged(nameof(NoSyndromes));

        var start = Syndromes.FirstOrDefault(item => item.Id == _settings.LastSyndrome) ?? Syndromes.FirstOrDefault();
        if (start is not null) await Open(start);
    }

    public Task SelectSyndromeAsync(string id)
    {
        var item = Syndromes.FirstOrDefault(syndrome => syndrome.Id == id);
        if (item is null) return Task.CompletedTask;
        SelectSyndrome(item);
        return Idle;
    }

    public Task AnswerAsync(string question, params string[] values)
    {
        _answers[question] = values;
        OnAnswersChanged();
        return Evaluate(question);
    }

    public Task ClearAsync(string question)
    {
        _answers.Remove(question);
        OnAnswersChanged();
        return Evaluate(question);
    }

    [RelayCommand]
    private void SelectSyndrome(SyndromeItem item) => Open(item);

    private Task Open(SyndromeItem item)
    {
        IsDrawerOpen = false;
        if (_knowledge is null || ReferenceEquals(item, SelectedSyndrome) && _current is not null) return Task.CompletedTask;

        foreach (var syndrome in Syndromes)
            syndrome.IsSelected = ReferenceEquals(syndrome, item);
        if (SelectedSyndrome is { } previous)
            _memory[previous.Id] = new Dictionary<string, IReadOnlyList<string>>(_answers, StringComparer.Ordinal);
        SelectedSyndrome = item;
        ChooseSet(item.Id);
        _answers.Clear();
        if (_memory.TryGetValue(item.Id, out var remembered))
            foreach (var pair in remembered)
                _answers[pair.Key] = pair.Value;
        History.Clear();
        OnPropertyChanged(nameof(HasHistory));
        OnAnswersChanged();
        _current = null;
        BuildQuestions(item.Id);
        Relabel();
        if (_settings.LastSyndrome != item.Id) Persist(_settings with { LastSyndrome = item.Id });
        return Evaluate(null);
    }

    [RelayCommand]
    private void Choose(ChoiceOption option)
    {
        var card = Questions.FirstOrDefault(item => item.Id == option.Owner);
        if (card is null) return;

        var effective = Effective(card.Id);
        string[] next;
        if (card.IsMulti)
        {
            next = effective.Contains(option.Code)
                ? effective.Where(code => code != option.Code).ToArray()
                : card.Options.Select(item => item.Code).Where(code => code == option.Code || effective.Contains(code)).ToArray();
        }
        else
        {
            if (effective.Count == 1 && effective[0] == option.Code) return;
            next = [option.Code];
        }

        Store(card.Id, next);
    }

    [RelayCommand]
    private void ToggleQuestion(QuestionCard card)
    {
        if (!card.IsBoolean) return;
        Store(card.Id, [card.IsOn ? "no" : "yes"]);
    }

    [RelayCommand(CanExecute = nameof(CanReset))]
    private void ResetAnswers()
    {
        if (_answers.Count == 0) return;
        _answers.Clear();
        OnAnswersChanged();
        Evaluate(null);
    }

    private IReadOnlyList<string> Effective(string id) =>
        _answers.TryGetValue(id, out var answered)
            ? answered
            : _current?.Questions.FirstOrDefault(state => state.QuestionId == id)?.Value ?? DefaultFor(id);

    private void Store(string id, IReadOnlyList<string> value)
    {
        if (Same(value, DefaultFor(id))) _answers.Remove(id);
        else _answers[id] = value;
        OnAnswersChanged();
        Evaluate(id);
    }

    private void OnAnswersChanged()
    {
        OnPropertyChanged(nameof(CanReset));
        ResetAnswersCommand.NotifyCanExecuteChanged();
    }

    private IReadOnlyList<string> DefaultFor(string id)
    {
        var question = _knowledge?.Questions.FirstOrDefault(item => item.Id == id);
        if (question is null) return [];
        if (question.DefaultFrom is { } resistanceId &&
            _knowledge!.Regions.FirstOrDefault(region => region.Id == _region)?.Resistance
                .FirstOrDefault(entry => entry.Id == resistanceId) is { } entry &&
            question.Options.Contains(entry.Category))
            return [entry.Category];
        return question.Default.ValueKind switch
        {
            JsonValueKind.String => [question.Default.GetString()!],
            JsonValueKind.Array => question.Default.EnumerateArray().Select(item => item.GetString() ?? "")
                .Where(item => item.Length > 0).ToArray(),
            _ => []
        };
    }

    private static bool Same(IReadOnlyList<string> left, IReadOnlyList<string> right) =>
        left.Count == right.Count && left.All(right.Contains);

    [RelayCommand]
    private void SelectGuidelineSet(ChoiceOption option)
    {
        if (SelectedSyndrome is not { } syndrome || option.Code == _set) return;
        _set = option.Code;
        var chosen = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in _settings.GuidelineSets ?? new Dictionary<string, string>())
            chosen[pair.Key] = pair.Value;
        chosen[syndrome.Id] = option.Code;
        Persist(_settings with { GuidelineSets = chosen });
        Relabel();
        Evaluate(null);
    }

    [RelayCommand]
    private void SelectRegion(ChoiceOption option)
    {
        if (_knowledge is null || option.Code == _region) return;
        _region = option.Code;
        NoRegionData = _knowledge.Regions.FirstOrDefault(region => region.Id == _region)?.Resistance.Count is null or 0;
        Persist(_settings with { Region = option.Code });
        if (SelectedSyndrome is { } syndrome)
            ChooseSet(syndrome.Id);
        Relabel();
        if (SelectedSyndrome is not null)
            Evaluate(null);
    }

    [RelayCommand]
    private void SelectLanguage(string code)
    {
        Localizer.SetLanguage(code);
        foreach (var option in Languages)
            option.IsSelected = option.Code == code;
        Persist(_settings with { Language = code });
    }

    [RelayCommand]
    private void DismissTip()
    {
        ShowTip = false;
        if (!_settings.TipSeen) Persist(_settings with { TipSeen = true });
    }

    [RelayCommand]
    private void FocusSyndrome(string index)
    {
        if (!int.TryParse(index, out var at)) return;
        var item = Syndromes.Where(syndrome => syndrome.IsMatch).Skip(at - 1).FirstOrDefault();
        if (item is not null) Open(item);
    }

    [RelayCommand]
    private void CloseOverlays()
    {
        IsSettingsOpen = false;
        IsDrawerOpen = false;
        IsTraceOpen = false;
    }

    [RelayCommand]
    private void ShowTab(string id)
    {
        CurrentTab = id == "rationale" ? RationaleTab : AlternativesTab;
    }

    [RelayCommand]
    private void ToggleTrace() => IsTraceOpen = !IsTraceOpen;

    [RelayCommand]
    private void ToggleSettings()
    {
        IsSettingsOpen = !IsSettingsOpen;
        if (IsSettingsOpen) IsDrawerOpen = false;
    }

    [RelayCommand]
    private void CloseSettings() => IsSettingsOpen = false;

    [RelayCommand]
    private void ToggleDrawer()
    {
        IsDrawerOpen = !IsDrawerOpen;
        if (IsDrawerOpen) IsSettingsOpen = false;
    }

    [RelayCommand]
    private void CloseDrawer() => IsDrawerOpen = false;

    [RelayCommand]
    private void Retry()
    {
        if (_knowledge is null)
        {
            Idle = LoadAsync();
            return;
        }

        if (SelectedSyndrome is null)
        {
            State = ScreenState.Empty;
            return;
        }

        Evaluate(null);
    }

    public void DropLeaving()
    {
        foreach (var row in Alternatives.Where(row => row.IsLeaving).ToArray())
            Alternatives.Remove(row);
        OnPropertyChanged(nameof(NoAlternatives));
    }

    partial void OnConfirmUpdateChanged(bool value)
    {
        if (_settings.ConfirmUpdate != value) Persist(_settings with { ConfirmUpdate = value });
    }

    partial void OnShowScoreChanged(bool value)
    {
        foreach (var row in Alternatives)
            row.ShowScore = value;
        if (_settings.ShowScore != value) Persist(_settings with { ShowScore = value });
    }

    partial void OnSearchTextChanged(string value) => Filter();

    partial void OnIsCompactChanged(bool value)
    {
        if (!value) IsDrawerOpen = false;
    }

    private void Persist(AppSettings settings)
    {
        _settings = settings;
        _store.Save(settings);
        OnPropertyChanged(nameof(GuidelineSet));
    }

    private void ChooseSet(string syndromeId)
    {
        var available = _setsBySyndrome.GetValueOrDefault(syndromeId) ?? [];
        var saved = _settings.GuidelineSets?.GetValueOrDefault(syndromeId);
        _set = saved is not null && available.Contains(saved) ? saved : DefaultSet(available);
        GuidelineSets.Clear();
        foreach (var id in available)
            GuidelineSets.Add(new ChoiceOption("set", id));
        OnSetsChanged();
    }

    private string DefaultSet(IReadOnlyList<string> available)
    {
        var sets = available
            .Select(id => _knowledge!.GuidelineSets.First(set => set.Id == id))
            .ToArray();
        var chosen = sets.FirstOrDefault(set => string.Equals(set.Region, _region, StringComparison.OrdinalIgnoreCase))
                     ?? sets.FirstOrDefault(set => set.Region is null)
                     ?? sets.FirstOrDefault();
        return chosen?.Id ?? "";
    }

    private void OnSetsChanged()
    {
        OnPropertyChanged(nameof(GuidelineSet));
        OnPropertyChanged(nameof(HasSetChoices));
        OnPropertyChanged(nameof(NoSetChoices));
    }

    private void Filter()
    {
        var query = SearchText.Trim();
        var compare = Localizer.Culture.CompareInfo;
        var options = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
        foreach (var item in Syndromes)
            item.IsMatch = query.Length == 0 ||
                           compare.IndexOf(item.Name, query, options) >= 0 ||
                           compare.IndexOf(item.Terms, query, options) >= 0 ||
                           item.Id.Contains(query, StringComparison.OrdinalIgnoreCase);
        OnPropertyChanged(nameof(NoMatches));
    }

    private Task Evaluate(string? trigger)
    {
        if (_engine is null || SelectedSyndrome is null) return Idle;

        var context = new GuidelineContext(
            SelectedSyndrome.Id,
            new Dictionary<string, IReadOnlyList<string>>(_answers, StringComparer.Ordinal),
            _region,
            _set);
        Idle = EvaluateAsync(_engine, context, trigger, ++_generation);
        return Idle;
    }

    private async Task EvaluateAsync(GuidelineEngine engine, GuidelineContext context, string? trigger, int generation)
    {
        var caller = Environment.CurrentManagedThreadId;
        var worker = caller;
        var watch = Stopwatch.StartNew();
        Recommendation result;
        try
        {
            result = await Task.Run(() =>
            {
                worker = Environment.CurrentManagedThreadId;
                return engine.Evaluate(context);
            });
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            if (generation == _generation) Fail("error.evaluate");
            return;
        }

        if (generation != _generation) return;
        LastEvaluation = watch.Elapsed;
        EvaluatedOffUiThread = worker != caller;
        Apply(result, trigger);
    }

    private void Fail(string key)
    {
        _errorKey = key;
        ErrorText = Localizer.Get(key);
        State = ScreenState.Error;
    }

    private void Apply(Recommendation result, string? trigger)
    {
        var diff = _current is not null && _current.SyndromeId == result.SyndromeId
            ? RecommendationDiff.Between(_current, result)
            : null;
        Applying?.Invoke(this, EventArgs.Empty);
        _errorKey = null;
        _current = result;
        Render(result, diff);
        Describe(diff, trigger);
        Applied?.Invoke(this, new AppliedEventArgs(diff, trigger));
    }

    private void Describe(RecommendationDiff? diff, string? trigger)
    {
        var card = trigger is null ? null : Questions.FirstOrDefault(item => item.Id == trigger);
        if (diff is null || card is null)
        {
            ChangeText = "";
            NoEffectText = "";
            return;
        }

        var key = diff.StatusChanged ? "change.status"
            : diff.FirstChoiceChanged ? "change.firstChoice"
            : diff.DoseChanges.Count > 0 ? "change.dose"
            : diff.DurationChanged ? "change.duration"
            : diff.AlternativesAdded.Count + diff.AlternativesRemoved.Count + diff.NewlyExcluded.Count +
              diff.NoLongerExcluded.Count > 0 ? "change.alternatives"
            : diff.SpectrumChanges.Count > 0 ? "change.spectrum"
            : null;
        ChangeText = key is null ? "" : Localizer.Format(key, ("question", card.Label));
        NoEffectText = key is null ? Localizer.Get("change.noEffect") : "";
        History.Insert(0, new TextRow(
            Localizer.Format("history.answer", ("question", card.Label), ("answer", AnswerText(card))),
            key is null ? Localizer.Get("history.noEffect") : ChangeText));
        while (History.Count > 5) History.RemoveAt(History.Count - 1);
        OnPropertyChanged(nameof(HasHistory));
    }

    private void BuildQuestions(string syndromeId)
    {
        OpenQuestions.Clear();
        ChosenQuestions.Clear();
        OnPropertyChanged(nameof(HasChosen));
        Questions.Clear();
        var syndrome = _knowledge!.Syndromes.First(item => item.Id == syndromeId);
        foreach (var id in syndrome.Questions)
        {
            var question = _knowledge.Questions.FirstOrDefault(item => item.Id == id);
            if (question is null) continue;
            var card = new QuestionCard(question.Id, question.Type);
            foreach (var option in question.Options)
                card.Options.Add(new ChoiceOption(question.Id, option) { IsMulti = card.IsMulti });
            Questions.Add(card);
        }
    }

    private void Relabel()
    {
        foreach (var item in Syndromes)
        {
            item.Name = Localizer.Get($"syndrome.{item.Id}.name");
            var aliasKey = $"syndrome.{item.Id}.aliases";
            var aliases = Localizer.Get(aliasKey);
            var drugs = (_drugsBySyndrome.GetValueOrDefault(item.Id) ?? []).Select(drug => Localizer.Get($"drug.{drug}.name"));
            item.Terms = string.Join(" · ", drugs.Prepend(aliases == aliasKey ? "" : aliases));
        }
        foreach (var option in GuidelineSets)
        {
            option.Label = Localizer.Get($"guideline.{option.Code}.name");
            option.IsSelected = option.Code == _set;
        }

        foreach (var option in Regions)
        {
            option.Label = Localizer.Get($"region.{option.Code}.name");
            option.IsSelected = option.Code == _region;
        }

        foreach (var card in Questions)
        {
            card.Label = Localizer.Get($"question.{card.Id}.label");
            foreach (var option in card.Options)
                option.Label = Localizer.Get($"question.{card.Id}.option.{option.Code}");
        }

        GuidelineName = _set.Length > 0
            ? Localizer.Get($"guideline.{_set}.name")
            : Localizer.Format("appbar.region", ("name", Localizer.Get($"region.{_region}.name")));
        SyndromeTitle = SelectedSyndrome?.Name ?? Localizer.Get("appbar.noSyndrome");
        GuidelineSettingLabel = SelectedSyndrome is { } selected
            ? Localizer.Format("settings.guidelineFor", ("syndrome", selected.Name))
            : Localizer.Get("settings.guideline");
        LicensesTitle = Localizer.Format("card.licenses", ("region", Localizer.Get($"region.{_region}.name")));
        AlternativesHeader = Localizer.Get("card.tab.alternatives");
        RationaleHeader = Localizer.Get("card.tab.rationale");
        SettingsPath = _store is FileSettingsStore file
            ? Localizer.Format("settings.stored", ("path", file.Path))
            : Localizer.Get("settings.storedMemory");
        UpdateSource();
        Filter();
        OnPropertyChanged(nameof(DataVersion));
        OnPropertyChanged(nameof(HasDataVersion));

        if (_errorKey is not null) ErrorText = Localizer.Get(_errorKey);
        if (_current is not null) Render(_current, null);
    }

    private void UpdateSource()
    {
        var set = _knowledge?.GuidelineSets.FirstOrDefault(item => item.Id == _set);
        var source = set is null ? null : _knowledge!.Sources.FirstOrDefault(item => item.Id == set.Source);
        SourceUrl = source?.Url;
    }

    private void Render(Recommendation result, RecommendationDiff? diff)
    {
        var culture = Localizer.Culture;
        GuidelineName = Localizer.Get($"guideline.{result.GuidelineSet}.name");
        SyndromeTitle = SelectedSyndrome?.Name ?? Localizer.Get("appbar.noSyndrome");

        RenderQuestions(result);
        RenderFirstChoice(result.FirstChoice, culture);
        RenderAlternatives(result, diff, culture);
        RenderSpectrum(result);

        Rationale.Clear();
        foreach (var line in result.Rationale)
            Rationale.Add(new TextRow(Localizer.Get(line.MessageKey), SourceLine(line.Source)));

        TraceRows.Clear();
        foreach (var line in result.Trace)
            TraceRows.Add(new TextRow(Localizer.Get(line.MessageKey), TraceDetail(line)));

        ConsultText = result.Status switch
        {
            RecommendationStatus.NoGuidelineRow => Localizer.Get("engine.no_row"),
            RecommendationStatus.NoCandidateLeft => Localizer.Get("engine.no_candidate"),
            _ => ""
        };

        HasFirstChoice = result.FirstChoice is not null &&
                         result.Status is RecommendationStatus.Selected or RecommendationStatus.Referral;
        OutcomeText = result.Status is RecommendationStatus.Referral or RecommendationStatus.NoAntibiotic &&
                      result.Rationale.Count > 0
            ? Localizer.Get(result.Rationale[0].MessageKey)
            : "";
        ReferralNote = Localizer.Get(HasFirstChoice ? "card.referral.withRegimen" : "card.referral.noRegimen");
        State = result.Status switch
        {
            RecommendationStatus.Selected => ScreenState.Ready,
            RecommendationStatus.Referral => ScreenState.Referral,
            RecommendationStatus.NoAntibiotic => ScreenState.NoAntibiotic,
            _ => ScreenState.Consult
        };
        Headline = State switch
        {
            ScreenState.Ready => string.Join(" + ", Slots.Select(slot => slot.DrugName)),
            ScreenState.Referral => Localizer.Get("card.referral.title"),
            ScreenState.NoAntibiotic => Localizer.Get("card.noAntibiotic.title"),
            _ => Localizer.Get("card.consult.title")
        };
        IsComposite = Slots.Count > 1;
        foreach (var slot in Slots)
            slot.ShowName = IsComposite || State != ScreenState.Ready;
        OnPropertyChanged(nameof(HasWarnings));
        OnPropertyChanged(nameof(HasExcluded));
        OnPropertyChanged(nameof(NoAlternatives));
        OnPropertyChanged(nameof(HasSpectrum));
        OnPropertyChanged(nameof(NoSpectrum));
    }

    private void RenderQuestions(Recommendation result)
    {
        foreach (var card in Questions)
        {
            var stateOf = result.Questions.FirstOrDefault(item => item.QuestionId == card.Id);
            card.IsVisible = stateOf?.Visible ?? true;
            var value = stateOf?.Value ?? [];
            foreach (var option in card.Options)
                option.IsSelected = value.Contains(option.Code);
            card.IsOn = value.Contains("yes");
            card.IsChanged = _answers.ContainsKey(card.Id);
        }

        Sync(OpenQuestions, Questions.Where(card => card.IsVisible && !card.IsChanged).ToList());
        Sync(ChosenQuestions, Questions.Where(card => card.IsVisible && card.IsChanged).ToList());
        OnPropertyChanged(nameof(HasChosen));
    }

    private void RenderFirstChoice(RegimenLine? line, CultureInfo culture)
    {
        var components = line?.Components ?? [];
        while (Slots.Count > components.Count) Slots.RemoveAt(Slots.Count - 1);
        while (Slots.Count < components.Count) Slots.Add(new ComponentSlot());
        for (var index = 0; index < components.Count; index++)
        {
            var component = components[index];
            var slot = Slots[index];
            slot.DrugId = component.DrugId;
            slot.DrugName = Localizer.Get($"drug.{component.DrugId}.name");            slot.Role = Localizer.Get($"role.{component.Role}");
            slot.Amount = component.Amount;
            slot.Schedule = Localizer.Format("card.interval",
                ("hours", component.IntervalHours.ToString(culture)));
            slot.Route = Localizer.Get($"route.{component.Route}");
            var extras = new List<string>();
            if (component.Loading is { Length: > 0 } loading)
                extras.Add(Localizer.Format("card.loading", ("dose", loading)));
            if (component.Amount70Kg is { Length: > 0 } adult)
                extras.Add(Localizer.Format("card.amount70", ("dose", adult)));
            slot.Extra = string.Join(" · ", extras);
            slot.HasExtra = extras.Count > 0;
        }

        HasDuration = line?.DurationDays is not null;
        DurationText = line?.DurationDays is { } days
            ? Localizer.Format("card.days", ("days", days.ToString(culture)))
            : Localizer.Get("card.noDuration");

        SourceText = line is null ? "" : SourceLine(line.Source);
        IsUnreviewed = line is not null && line.Source.ReviewStatus != "reviewed";
        ReviewText = line is null
            ? ""
            : IsUnreviewed
                ? Localizer.Get("card.review.unreviewed")
                : Localizer.Format("card.review.reviewed",
                    ("date", line.Source.ReviewedAt?.ToString("d", culture) ?? ""));

        Warnings.Clear();
        foreach (var warning in line?.Warnings ?? [])
        {
            var subject = warning.Subject is { Length: > 0 } drug ? Localizer.Get($"drug.{drug}.name") : "";
            Warnings.Add(new TextRow(Localizer.Get(warning.MessageKey), subject));
        }

        Licenses.Clear();
        var region = _knowledge?.Regions.FirstOrDefault(item => item.Id == _region);
        foreach (var component in components)
        {
            var status = region?.Licensing.FirstOrDefault(item => item.Drug == component.DrugId)?.Status ?? "unknown";
            Licenses.Add(new TextRow(
                Localizer.Format("card.license", ("drug", Localizer.Get($"drug.{component.DrugId}.name")),
                    ("status", Localizer.Get($"license.{status}"))),
                ""));
        }
    }

    private void RenderAlternatives(Recommendation result, RecommendationDiff? diff, CultureInfo culture)
    {
        DropLeaving();
        var newlyExcluded = diff?.NewlyExcluded.ToDictionary(item => item.RegimenId, StringComparer.Ordinal)
                            ?? new Dictionary<string, ExcludedRegimen>(StringComparer.Ordinal);
        var existing = Alternatives.ToDictionary(row => row.Key, StringComparer.Ordinal);
        var desired = new List<AlternativeRow>();
        foreach (var line in result.Alternatives)
        {
            var row = existing.GetValueOrDefault(line.Key) ?? new AlternativeRow(line.Key, line.RegimenId);
            row.Title = string.Join(" + ", line.Components.Select(item => Localizer.Get($"drug.{item.DrugId}.name")));
            var dose = string.Join(" · ", line.Components.Select(item =>
                $"{item.Amount} {Localizer.Get($"route.{item.Route}")} {Localizer.Format("card.interval", ("hours", item.IntervalHours.ToString(culture)))}"));
            var tier = Localizer.Format("card.tier", ("tier", line.Tier.ToString(culture)));
            row.Detail = $"{tier} · {dose}";
            row.HasScore = line.Score is not null;
            row.ShowScore = ShowScore;
            row.ScoreTotal = line.Score is null
                ? ""
                : Localizer.Format("score.total", ("value", line.Score.Total.ToString("0.00", culture)));
            row.ScoreParts = line.Score?.Components
                .Select(part => new ScorePart(
                    Localizer.Get($"scoring.{part.Id}"),
                    Localizer.Format("score.formula",
                        ("value", part.Value.ToString("0.00", culture)),
                        ("weight", part.Weight.ToString("0.00", culture)),
                        ("contribution", part.Contribution.ToString("0.00", culture)))))
                .ToArray() ?? [];
            desired.Add(row);
        }

        foreach (var row in Alternatives)
        {
            if (desired.Contains(row) || !newlyExcluded.TryGetValue(row.RegimenId, out var excluded)) continue;
            row.IsLeaving = true;
            row.LeavingReason = Localizer.Get(excluded.ReasonKey);
            desired.Add(row);
        }

        Sync(Alternatives, desired);

        Excluded.Clear();
        foreach (var item in result.Excluded)
        {
            var row = new ExcludedRow(item.RegimenId)
            {
                Title = RegimenName(item.RegimenId, item.DrugId),
                Reason = Localizer.Get(item.ReasonKey)
            };
            Excluded.Add(row);
        }

        AlternativesHeader = Localizer.Format("card.tab.alternativesCount",
            ("count", result.Alternatives.Count.ToString(culture)));
        ExcludedHeader = Localizer.Format("card.excluded",
            ("count", result.Excluded.Count.ToString(culture)));
    }

    private void RenderSpectrum(Recommendation result)
    {
        var existing = Spectrum.ToDictionary(row => row.PathogenId, StringComparer.Ordinal);
        var desired = new List<SpectrumRow>();
        foreach (var bar in result.Spectrum)
        {
            var row = existing.GetValueOrDefault(bar.PathogenId) ?? new SpectrumRow(bar.PathogenId);
            row.Name = Localizer.Get($"pathogen.{bar.PathogenId}.name");
            row.ShortName = Localizer.Get($"pathogen.{bar.PathogenId}.short");
            row.Level = bar.Level;
            row.LevelText = Localizer.Get($"spectrum.{bar.Level}");
            row.Coverage = Math.Clamp(bar.Coverage, 0, 1);
            row.AtRisk = bar.AtRisk;
            row.RiskText = Localizer.Get("spectrum.atRisk");
            desired.Add(row);
        }

        Sync(Spectrum, desired);
    }

    private static string AnswerText(QuestionCard card) =>
        card.IsBoolean
            ? Localizer.Get(card.IsOn ? "history.yes" : "history.no")
            : string.Join(", ", card.Options.Where(option => option.IsSelected).Select(option => option.Label));

    public string BuildSummary()
    {
        var lines = new List<string>
        {
            $"{SyndromeTitle} — {GuidelineName}",
            Headline
        };
        foreach (var slot in Slots)
            lines.Add($"• {slot.DrugName}: {slot.Amount} {slot.Route} {slot.Schedule}{(slot.HasExtra ? " (" + slot.Extra + ")" : "")}");
        if (HasFirstChoice) lines.Add(DurationText);
        lines.Add(OutcomeText);
        lines.Add(ConsultText);
        foreach (var warning in Warnings)
            lines.Add("! " + (warning.Detail.Length > 0 ? warning.Detail + ": " : "") + warning.Text);
        var chosen = Questions.Where(card => card.IsVisible && card.IsChanged).ToArray();
        if (chosen.Length > 0)
        {
            lines.Add(Localizer.Get("summary.answers"));
            foreach (var card in chosen)
                lines.Add($"  {card.Label}: {AnswerText(card)}");
        }
        lines.Add(SourceText);
        lines.Add(ReviewText);
        lines.Add(DataVersion + " · AbxPilot " + Update.GitHubUpdater.Label);
        lines.Add(Localizer.Get("footer.disclaimer"));
        return string.Join(Environment.NewLine, lines.Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    private string RegimenName(string regimenId, string? drugId)
    {
        var regimen = _knowledge?.Regimens.FirstOrDefault(item => item.Id == regimenId);
        if (regimen is not null)
            return string.Join(" + ", regimen.Components.Select(item => Localizer.Get($"drug.{item.Drug}.name")));
        return drugId is null ? regimenId : Localizer.Get($"drug.{drugId}.name");
    }

    private static string SourceLine(SourceRef source)
    {
        var name = Localizer.Get($"guideline.{source.Source}.name");
        if (name == $"guideline.{source.Source}.name") name = source.Source;
        return Localizer.Format("card.source", ("name", name), ("section", source.Section));
    }

    private static string TraceDetail(TraceLine line)
    {
        var subject = line.Subject is { Length: > 0 } text ? $" · {text}" : "";
        return $"{line.RuleId}{subject}";
    }

    private static void Sync<T>(ObservableCollection<T> target, IList<T> desired) where T : class
    {
        for (var index = 0; index < desired.Count; index++)
        {
            var at = target.IndexOf(desired[index]);
            if (at == index) continue;
            if (at >= 0) target.Move(at, index);
            else target.Insert(index, desired[index]);
        }

        while (target.Count > desired.Count) target.RemoveAt(target.Count - 1);
    }
}

public sealed partial class LanguageOption : ObservableObject
{
    public LanguageOption(string code, bool selected)
    {
        Code = code;
        isSelected = selected;
        Localizer.Changed += (_, _) => OnPropertyChanged(nameof(Label));
    }

    public string Code { get; }

    public string Label => Localizer.Get("lang." + Code);

    [ObservableProperty]
    private bool isSelected;
}
