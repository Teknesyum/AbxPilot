using System.Collections.ObjectModel;
using AbxPilot.Core.Knowledge;
using AbxPilot.Data;
using AbxPilot.UI.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbxPilot.UI.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly KnowledgeBase _knowledge = KbResources.Knowledge();

    public MainViewModel()
    {
        foreach (var code in Localizer.Languages)
            Languages.Add(new LanguageOption(code, code == Localizer.Language));

        Localizer.Changed += (_, _) =>
        {
            OnPropertyChanged(nameof(DataVersion));
            OnPropertyChanged(nameof(Syndromes));
        };
    }

    public ObservableCollection<LanguageOption> Languages { get; } = [];

    public IReadOnlyList<string> Syndromes =>
        _knowledge.Syndromes.Select(syndrome => Localizer.Get($"syndrome.{syndrome.Id}.name")).ToArray();

    public bool HasSyndromes => _knowledge.Syndromes.Count > 0;

    public bool NoSyndromes => !HasSyndromes;

    public string DataVersion => Localizer.Format("titlebar.dataVersion", ("version", _knowledge.Version));

    [RelayCommand]
    private void SelectLanguage(string code)
    {
        Localizer.SetLanguage(code);
        foreach (var option in Languages)
            option.IsSelected = option.Code == code;
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
