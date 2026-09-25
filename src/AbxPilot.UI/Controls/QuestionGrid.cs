using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;

namespace AbxPilot.UI.Controls;

public sealed class QuestionGrid : Panel
{
    public static readonly StyledProperty<double> ColumnMinProperty =
        AvaloniaProperty.Register<QuestionGrid, double>(nameof(ColumnMin), 1);

    public static readonly StyledProperty<int> ColumnsMaxProperty =
        AvaloniaProperty.Register<QuestionGrid, int>(nameof(ColumnsMax), 1);

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<QuestionGrid, double>(nameof(Spacing));

    public static readonly AttachedProperty<bool> IsWideProperty =
        AvaloniaProperty.RegisterAttached<QuestionGrid, Control, bool>("IsWide");

    static QuestionGrid()
    {
        AffectsMeasure<QuestionGrid>(ColumnMinProperty, ColumnsMaxProperty, SpacingProperty);
        IsWideProperty.Changed.AddClassHandler<Control>((control, _) =>
            control.FindAncestorOfType<QuestionGrid>()?.InvalidateMeasure());
    }

    public static bool GetIsWide(Control control) => control.GetValue(IsWideProperty);

    public static void SetIsWide(Control control, bool value) => control.SetValue(IsWideProperty, value);

    public double ColumnMin
    {
        get => GetValue(ColumnMinProperty);
        set => SetValue(ColumnMinProperty, value);
    }

    public int ColumnsMax
    {
        get => GetValue(ColumnsMaxProperty);
        set => SetValue(ColumnsMaxProperty, value);
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public int Columns { get; private set; } = 1;

    private int CountFor(double width)
    {
        if (double.IsInfinity(width) || width <= 0) return Math.Max(1, ColumnsMax);
        var fit = (int)Math.Floor((width + Spacing) / (Math.Max(1, ColumnMin) + Spacing));
        return Math.Clamp(fit, 1, Math.Max(1, ColumnsMax));
    }

    private double CellWidth(double width, int columns) =>
        Math.Max(0, (width - Spacing * (columns - 1)) / columns);

    private static bool Wide(Control child) =>
        GetIsWide(child) || child is ContentPresenter { Child: { } inner } && GetIsWide(inner);

    private List<List<Control>> Lines(int columns)
    {
        var lines = new List<List<Control>>();
        var waiting = new List<Control>();
        List<Control>? line = null;
        foreach (var child in Children.Where(child => child.IsVisible))
        {
            if (Wide(child))
            {
                if (line is null || line.Count == columns) lines.Add([child]);
                else waiting.Add(child);
                continue;
            }

            if (line is null || line.Count == columns)
            {
                line = [];
                lines.Add(line);
            }

            line.Add(child);
            if (line.Count == columns) Flush();
        }

        Flush();
        return lines;

        void Flush()
        {
            foreach (var wide in waiting) lines.Add([wide]);
            waiting.Clear();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var columns = CountFor(availableSize.Width);
        Columns = columns;
        var width = double.IsInfinity(availableSize.Width) ? ColumnMin * columns + Spacing * (columns - 1) : availableSize.Width;
        var cell = CellWidth(width, columns);
        foreach (var child in Children)
            child.Measure(new Size(Wide(child) ? width : cell, double.PositiveInfinity));
        foreach (var child in Children.Where(Wide))
            child.Measure(new Size(width, double.PositiveInfinity));
        var lines = Lines(columns);
        var height = lines.Sum(line => line.Max(child => child.DesiredSize.Height)) + Spacing * Math.Max(0, lines.Count - 1);
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var columns = CountFor(finalSize.Width);
        Columns = columns;
        var cell = CellWidth(finalSize.Width, columns);
        var top = 0d;
        foreach (var line in Lines(columns))
        {
            var height = line.Max(child => child.DesiredSize.Height);
            if (line.Count == 1 && Wide(line[0]))
                line[0].Arrange(new Rect(0, top, finalSize.Width, height));
            else
                for (var index = 0; index < line.Count; index++)
                    line[index].Arrange(new Rect(index * (cell + Spacing), top, cell, height));
            top += height + Spacing;
        }

        return finalSize;
    }
}
