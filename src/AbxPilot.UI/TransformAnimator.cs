using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace AbxPilot.UI;

public sealed class TransformAnimator : InterpolatingAnimator<ITransform>
{
    public override ITransform Interpolate(double progress, ITransform oldValue, ITransform newValue) =>
        TransformOperations.Interpolate(oldValue as TransformOperations ?? TransformOperations.Identity,
            newValue as TransformOperations ?? TransformOperations.Identity, progress);
}
