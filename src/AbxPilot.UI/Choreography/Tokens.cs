using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace AbxPilot.UI.Choreography;

public static class Tokens
{
    public static TimeSpan Time(string key) => Find<TimeSpan>(key, TimeSpan.Zero);

    public static Easing Ease(string key) => Find<Easing?>(key, null) ?? new LinearEasing();

    public static double Number(string key) => Find(key, 0d);

    public static bool Animate => !Motion.Reduced;

    private static T Find<T>(string key, T fallback)
    {
        if (Application.Current is { } app && app.TryGetResource(key, app.ActualThemeVariant, out var value) && value is T typed)
            return typed;
        if (Application.Current?.Styles is { } styles)
            foreach (var style in styles)
                if (style is IResourceProvider provider && provider.TryGetResource(key, null, out var found) && found is T match)
                    return match;
        return fallback;
    }
}
