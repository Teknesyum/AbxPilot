using System.Collections.ObjectModel;
using AbxPilot.Data;
using AbxPilot.UI.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbxPilot.UI.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly KbManifest _manifest = KbResources.Manifest();

    public MainViewModel()
    {
        foreach (var code in Localizer.Languages)
            Languages.Add(new LanguageOption(code, code == Localizer.Language));

        Syndromes = _manifest.Syndromes;
        Localizer.Changed += (_, _) => OnPropertyChanged(nameof(DataVersion));
    }

    public ObservableCollection<LanguageOption> Languages { get; } = [];

    public IReadOnlyList<string> Syndromes { get; }

    public bool HasSyndromes => Syndromes.Count > 0;

    public bool NoSyndromes => !HasSyndromes;

    public string DataVersion => Localizer.Format("titlebar.dataVersion", ("version", _manifest.Version));

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
