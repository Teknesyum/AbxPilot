using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
        TitleBar.PointerPressed += OnTitleBarPointerPressed;
        TitleNotice.SizeChanged += (_, _) => FitNotice();
        Localizer.Changed += (_, _) => Dispatcher.UIThread.Post(FitNotice, DispatcherPriority.Loaded);
        PropertyChanged += (_, e) =>
        {
            if (e.Property != WindowStateProperty) return;
            ApplyWindowFrame();
            UpdateMaximizeGlyph();
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

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        if (e.ClickCount == 2)
        {
            ToggleMaximizeRestore();
            e.Handled = true;
            return;
        }
        BeginMoveDrag(e);
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

    private void UpdateMaximizeGlyph()
    {
        var key = WindowState == WindowState.Maximized ? "IconRestore" : "IconMaximize";
        if (this.TryFindResource(key, out var geometry) && geometry is Geometry shape) MaximizeGlyph.Data = shape;
    }

    private void OnMinimize(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void OnMaximizeRestore(object? sender, RoutedEventArgs e) => ToggleMaximizeRestore();
    private void OnClose(object? sender, RoutedEventArgs e) => Close();
    private void ToggleMaximizeRestore() => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
}
