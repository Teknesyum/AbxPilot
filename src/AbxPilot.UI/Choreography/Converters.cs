using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace AbxPilot.UI.Choreography;

public static class Converters
{
    public static readonly IValueConverter ScaleX = new FuncValueConverter<double, ITransform>(value =>
        TransformOperations.Parse("scaleX(" + Math.Clamp(value, 0, 1).ToString("0.###", CultureInfo.InvariantCulture) + ")"));

    public static readonly IValueConverter Gutter = new FuncValueConverter<double, Thickness>(value => new Thickness(0, 0, value, 0));
    public static readonly IValueConverter Overhang = new FuncValueConverter<double, Thickness>(value => new Thickness(0, 0, -value, 0));
    public static readonly IValueConverter Inset = new FuncValueConverter<double, Thickness>(value => new Thickness(value));

    public static readonly IValueConverter Sides = new FuncValueConverter<double, Thickness>(value => new Thickness(value, 0));

    public static readonly IValueConverter ShiftX = new FuncValueConverter<double, ITransform>(value =>
        TransformOperations.Parse("translateX(" + value.ToString("0.###", CultureInfo.InvariantCulture) + "px)"));

    public static readonly IValueConverter Round = new FuncValueConverter<double, CornerRadius>(value => new CornerRadius(value / 2));
}
