using Avalonia;
using Avalonia.Controls;

namespace AbxPilot.UI.Controls;

public sealed class QuestionGrid : Panel
{
    public static readonly StyledProperty<double> ColumnMinProperty =
        AvaloniaProperty.Register<QuestionGrid, double>(nameof(ColumnMin), 1);

    public static readonly StyledProperty<int> ColumnsMaxProperty =
        AvaloniaProperty.Register<QuestionGrid, int>(nameof(ColumnsMax), 1);

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<QuestionGrid, double>(nameof(Spacing));

    static QuestionGrid()
    {
        AffectsMeasure<QuestionGrid>(ColumnMinProperty, ColumnsMaxProperty, SpacingProperty);
    }

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

    private List<Control> Shown() => Children.Where(child => child.IsVisible).ToList();

    protected override Size MeasureOverride(Size availableSize)
    {
        var columns = CountFor(availableSize.Width);
        Columns = columns;
        var width = double.IsInfinity(availableSize.Width) ? ColumnMin * columns + Spacing * (columns - 1) : availableSize.Width;
        var cell = CellWidth(width, columns);
        var shown = Shown();
        foreach (var child in Children)
            child.Measure(new Size(cell, double.PositiveInfinity));
        var height = 0d;
        for (var start = 0; start < shown.Count; start += columns)
        {
            var row = shown.Skip(start).Take(columns).Max(child => child.DesiredSize.Height);
            height += row + (start > 0 ? Spacing : 0);
        }
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var columns = CountFor(finalSize.Width);
        Columns = columns;
        var cell = CellWidth(finalSize.Width, columns);
        var shown = Shown();
        var top = 0d;
        for (var start = 0; start < shown.Count; start += columns)
        {
            var row = shown.Skip(start).Take(columns).ToList();
            var height = row.Max(child => child.DesiredSize.Height);
            for (var index = 0; index < row.Count; index++)
                row[index].Arrange(new Rect(index * (cell + Spacing), top, cell, height));
            top += height + Spacing;
        }
        return finalSize;
    }
}
