using Avalonia;
using Avalonia.Controls;

namespace AbxPilot.UI.Controls;

public sealed class QuestionRow : Panel
{
    public static readonly StyledProperty<double> LabelMinProperty =
        AvaloniaProperty.Register<QuestionRow, double>(nameof(LabelMin));

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<QuestionRow, double>(nameof(Spacing));

    public static readonly StyledProperty<bool> IsFlexibleProperty =
        AvaloniaProperty.Register<QuestionRow, bool>(nameof(IsFlexible));

    static QuestionRow()
    {
        AffectsMeasure<QuestionRow>(LabelMinProperty, SpacingProperty, IsFlexibleProperty);
    }

    public double LabelMin
    {
        get => GetValue(LabelMinProperty);
        set => SetValue(LabelMinProperty, value);
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public bool IsFlexible
    {
        get => GetValue(IsFlexibleProperty);
        set => SetValue(IsFlexibleProperty, value);
    }

    public bool IsStacked { get; private set; }

    private Control? Label => Children.Count > 0 ? Children[0] : null;

    private Control? Control => Children.Count > 1 ? Children[1] : null;

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Label is not { } label || Control is not { } control) return base.MeasureOverride(availableSize);

        var width = availableSize.Width;
        var reserve = LabelMin;
        if (IsFlexible && !double.IsInfinity(width))
        {
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            reserve = Math.Clamp(label.DesiredSize.Width, LabelMin, Math.Max(LabelMin, width / 2));
        }
        var room = Math.Max(0, width - reserve - Spacing);
        control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var natural = control.DesiredSize.Width;
        IsStacked = !double.IsInfinity(width) && natural > room && !IsFlexible;

        if (IsStacked)
        {
            label.Measure(new Size(width, double.PositiveInfinity));
            control.Measure(new Size(width, double.PositiveInfinity));
            return new Size(width, label.DesiredSize.Height + Spacing + control.DesiredSize.Height);
        }

        if (natural > room) control.Measure(new Size(room, double.PositiveInfinity));
        var used = Math.Min(natural, room);
        label.Measure(new Size(Math.Max(0, width - used - Spacing), double.PositiveInfinity));
        var total = double.IsInfinity(width) ? label.DesiredSize.Width + Spacing + used : width;
        return new Size(total, Math.Max(label.DesiredSize.Height, control.DesiredSize.Height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Label is not { } label || Control is not { } control) return base.ArrangeOverride(finalSize);

        if (IsStacked)
        {
            label.Arrange(new Rect(0, 0, finalSize.Width, label.DesiredSize.Height));
            control.Arrange(new Rect(0, label.DesiredSize.Height + Spacing, control.DesiredSize.Width, control.DesiredSize.Height));
            return finalSize;
        }

        var used = control.DesiredSize.Width;
        var labelWidth = Math.Max(0, finalSize.Width - used - Spacing);
        label.Arrange(new Rect(0, (finalSize.Height - label.DesiredSize.Height) / 2, labelWidth, label.DesiredSize.Height));
        control.Arrange(new Rect(finalSize.Width - used, (finalSize.Height - control.DesiredSize.Height) / 2, used, control.DesiredSize.Height));
        return finalSize;
    }
}
