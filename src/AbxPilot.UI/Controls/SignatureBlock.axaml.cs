using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AbxPilot.UI.Controls;

public partial class SignatureBlock : UserControl
{
    public SignatureBlock() => InitializeComponent();

    private async void OnSignatureNavigate(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string url }) return;
        var launcher = TopLevel.GetTopLevel(this)?.Launcher;
        if (launcher is null) return;
        await launcher.LaunchUriAsync(new Uri(url));
    }
}
