using System.ComponentModel;
using Avalonia.Data;

namespace AbxPilot.UI.Localization;

public sealed class LocalizedText : INotifyPropertyChanged
{
    private static readonly Dictionary<string, LocalizedText> Known = new(StringComparer.Ordinal);

    private static readonly CompiledBindingPath ValuePath =
        CompiledBinding.Create<LocalizedText, string>(text => text.Value).Path!;

    private readonly string _key;

    private LocalizedText(string key)
    {
        _key = key;
        Localizer.Changed += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        Binding = new CompiledBinding(ValuePath) { Source = this, Mode = BindingMode.OneWay };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Value => Localizer.Get(_key);

    internal CompiledBinding Binding { get; }

    public static LocalizedText For(string key)
    {
        lock (Known)
        {
            if (Known.TryGetValue(key, out var known)) return known;
            var made = new LocalizedText(key);
            Known[key] = made;
            return made;
        }
    }
}

public sealed class TextExtension
{
    public TextExtension()
    {
    }

    public TextExtension(string key) => Key = key;

    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider provider) => LocalizedText.For(Key).Binding;
}
