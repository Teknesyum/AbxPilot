using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AbxPilot.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (!Motion.Reduced) Classes.Add("anim");
        FitToWorkArea();
        TitleBar.PointerPressed += OnTitleBarPointerPressed;
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
