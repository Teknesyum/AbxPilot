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

    public static readonly IValueConverter Inset = new FuncValueConverter<double, Thickness>(value => new Thickness(value));
}
