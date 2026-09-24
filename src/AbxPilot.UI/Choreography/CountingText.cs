using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;

namespace AbxPilot.UI.Choreography;

public sealed partial class CountingText : TextBlock
{
    public static readonly StyledProperty<string?> AmountProperty =
        AvaloniaProperty.Register<CountingText, string?>(nameof(Amount));

    public static readonly StyledProperty<double> NumberProperty =
        AvaloniaProperty.Register<CountingText, double>(nameof(Number));

    private string _suffix = "";
    private int _decimals;
    private bool _counting;

    protected override Type StyleKeyOverride => typeof(TextBlock);

    public string? Amount
    {
        get => GetValue(AmountProperty);
        set => SetValue(AmountProperty, value);
    }

    public double Number
    {
        get => GetValue(NumberProperty);
        set => SetValue(NumberProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == AmountProperty)
            Retarget(change.GetOldValue<string?>(), change.GetNewValue<string?>());
        else if (change.Property == NumberProperty && _counting)
            Text = Compose(Number);
    }

    private void Retarget(string? before, string? after)
    {
        var next = Parse(after);
        var previous = Parse(before);
        if (next is null || previous is null || previous.Value.Suffix != next.Value.Suffix || !Tokens.Animate)
        {
            _counting = false;
            Transitions = null;
            if (next is not null) Number = next.Value.Value;
            Text = after;
            return;
        }

        _suffix = next.Value.Suffix;
        _decimals = next.Value.Decimals;
        _counting = true;
        Transitions ??=
        [
            new DoubleTransition
            {
                Property = NumberProperty,
                Duration = Tokens.Time("TBase"),
                Easing = Tokens.Ease("EOut")
            }
        ];
        Number = next.Value.Value;
    }

    private string Compose(double value)
    {
        var text = value.ToString("F" + _decimals, CultureInfo.InvariantCulture) + _suffix;
        var target = Parse(Amount);
        if (target is not null && Math.Abs(value - target.Value.Value) < 1e-9) return Amount!;
        return text;
    }

    private static (double Value, string Suffix, int Decimals)? Parse(string? text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        var match = Leading().Match(text);
        if (!match.Success) return null;
        var number = match.Groups[1].Value;
        var dot = number.IndexOf('.');
        return (double.Parse(number, CultureInfo.InvariantCulture), match.Groups[2].Value, dot < 0 ? 0 : number.Length - dot - 1);
    }

    [GeneratedRegex(@"^(\d+(?:\.\d+)?)(.*)$", RegexOptions.Singleline)]
    private static partial Regex Leading();
}
