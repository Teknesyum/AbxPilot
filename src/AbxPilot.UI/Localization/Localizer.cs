using System.Globalization;
using AbxPilot.Data;

namespace AbxPilot.UI.Localization;

public static class Localizer
{
    public const string DefaultLanguage = "tr";

    private static readonly object Gate = new();
    private static IReadOnlyDictionary<string, string> strings = KbResources.Strings(DefaultLanguage);
    private static IReadOnlyDictionary<string, string> labels = Etiketler.Oku(DefaultLanguage);
    private static string language = DefaultLanguage;

    public static event EventHandler? Changed;

    public static IReadOnlyList<string> Languages => KbResources.Languages;

    public static string Language
    {
        get { lock (Gate) return language; }
    }

    public static CultureInfo Culture => CultureInfo.GetCultureInfo(Language);

    public static void SetLanguage(string code)
    {
        lock (Gate)
        {
            if (string.Equals(language, code, StringComparison.Ordinal)) return;
            strings = KbResources.Strings(code);
            labels = Etiketler.Oku(code);
            language = code;
        }

        Changed?.Invoke(null, EventArgs.Empty);
    }

    public static string Get(string key)
    {
        lock (Gate)
            return strings.TryGetValue(key, out var value) ? value
                : labels.TryGetValue(key, out var label) ? label
                : key;
    }

    public static string Format(string key, params (string Name, string Value)[] values)
    {
        var text = Get(key);
        foreach (var (name, value) in values)
            text = text.Replace("{" + name + "}", value, StringComparison.Ordinal);
        return text;
    }
}
