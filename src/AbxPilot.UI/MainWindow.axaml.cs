using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using AbxPilot.UI.Localization;

namespace AbxPilot.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (!Motion.Reduced) Classes.Add("anim");
        FitToWorkArea();
        TitleNotice.SizeChanged += (_, _) => FitNotice();
        Localizer.Changed += (_, _) => Dispatcher.UIThread.Post(FitNotice, DispatcherPriority.Loaded);
        PropertyChanged += (_, e) =>
        {
            if (e.Property != WindowStateProperty) return;
            ApplyWindowFrame();
        };
    }

    private void FitToWorkArea()
    {
        var screen = Screens.Primary;
        if (screen is null || !this.TryFindResource("WindowWorkAreaShare", out var value) || value is not double share) return;
        var area = screen.WorkingArea.Size.ToSize(screen.Scaling);
        Width = Math.Max(MinWidth, area.Width * share);
        Height = Math.Max(MinHeight, area.Height * share);
    }

    private void FitNotice()
    {
        var text = NoticeFull.Text ?? string.Empty;
        using var layout = new TextLayout(text, new Typeface(NoticeFull.FontFamily, NoticeFull.FontStyle, NoticeFull.FontWeight), NoticeFull.FontSize, null);
        var fits = layout.Width <= TitleNotice.Bounds.Width;
        NoticeFull.IsVisible = fits;
        NoticeShort.IsVisible = !fits;
    }

    private void ApplyWindowFrame()
    {
        var maximized = WindowState == WindowState.Maximized;
        WindowShell.BorderThickness = maximized
            ? new Thickness(0)
            : this.TryFindResource("BorderWidth", out var border) && border is Thickness thickness ? thickness : default;
        WindowShell.CornerRadius = maximized
            ? new CornerRadius(0)
            : this.TryFindResource("WindowRadius", out var radius) && radius is CornerRadius corner ? corner : default;
    }
}
