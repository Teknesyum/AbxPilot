using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;

namespace AbxPilot.UI.Choreography;

public sealed class NudgeTransition : IPageTransition
{
    public string Axis { get; set; } = "X";

    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;
        if (!Tokens.Animate)
        {
            if (from is not null) from.IsVisible = false;
            if (to is not null) to.IsVisible = true;
            return;
        }

        var offset = Tokens.Number("EntryOffset") * (forward ? 1 : -1);
        var property = Axis == "Y" ? TranslateTransform.YProperty : TranslateTransform.XProperty;
        var tasks = new List<Task>();
        if (from is not null)
            tasks.Add(Make(property, 0, -offset, 1, 0, Tokens.Time("TFast"), Tokens.Ease("EIn")).RunAsync(from, cancellationToken));
        if (to is not null)
        {
            to.IsVisible = true;
            tasks.Add(Make(property, offset, 0, 0, 1, Tokens.Time("TBase"), Tokens.Ease("EOut")).RunAsync(to, cancellationToken));
        }

        await Task.WhenAll(tasks);
        if (from is not null && !cancellationToken.IsCancellationRequested) from.IsVisible = false;
    }

    private static Animation Make(AvaloniaProperty property, double fromOffset, double toOffset, double fromOpacity,
        double toOpacity, TimeSpan duration, Avalonia.Animation.Easings.Easing easing) =>
        new()
        {
            Duration = duration,
            Easing = easing,
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters = { new Setter(property, fromOffset), new Setter(Visual.OpacityProperty, fromOpacity) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(property, toOffset), new Setter(Visual.OpacityProperty, toOpacity) }
                }
            }
        };
}
