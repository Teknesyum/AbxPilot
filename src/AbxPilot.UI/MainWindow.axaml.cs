using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Styling;
using Avalonia.Threading;
using AbxPilot.UI.Kabuk;
using AbxPilot.UI.Localization;
using AbxPilot.UI.Update;
using AbxPilot.UI.ViewModels;

namespace AbxPilot.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (!Motion.Reduced) Classes.Add("anim");
        FitToWorkArea();
        ApplyLabels();
        BindUpdates();
        UygulamaOlcegi.Bagla(this);
        TitleNotice.SizeChanged += (_, _) => FitNotice();
        Localizer.Changed += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            ApplyLabels();
            FitNotice();
        }, DispatcherPriority.Loaded);
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

    private void ApplyLabels()
    {
        Title = Localizer.Get("app.name.first") + Localizer.Get("app.name.second");
    }

    private void BindUpdates()
    {
        var updater = new GitHubUpdater();
        SurumDugmesi.Surum = GitHubUpdater.Label;
        SurumDugmesi.OnayIste = true;
        SurumDugmesi.Denetle = updater.CheckAsync;
        SurumDugmesi.Sor = result => DataContext is MainViewModel { ConfirmUpdate: true } ? AskInstall(result) : Task.FromResult(true);
        SurumDugmesi.Kur = updater.InstallAsync;
    }

    private async Task<bool> AskInstall(SurumSonucu result)
    {
        var accepted = false;
        var dialog = new Window
        {
            Title = Title,
            Icon = Icon,
            SizeToContent = SizeToContent.WidthAndHeight,
            CanResize = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            FontFamily = FontFamily,
            FontSize = FontSize,
            Foreground = Foreground,
            Background = this.TryFindResource("Surface", out var surface) && surface is IBrush brush ? brush : Brushes.Transparent
        };
        var install = new Button { Content = Localizer.Get("update.installNow"), IsDefault = true };
        var later = new Button { Content = Localizer.Get("update.later"), IsCancel = true };
        if (this.TryFindResource("PrimaryButton", out var primary) && primary is ControlTheme primaryTheme) install.Theme = primaryTheme;
        if (this.TryFindResource("HeaderButton", out var plain) && plain is ControlTheme plainTheme) later.Theme = plainTheme;
        install.Click += (_, _) => { accepted = true; dialog.Close(); };
        later.Click += (_, _) => dialog.Close();
        var gap = this.TryFindResource("Space3", out var space) && space is double value ? value : 0;
        dialog.Content = new StackPanel
        {
            Margin = new Thickness(gap * 2),
            Spacing = gap,
            MaxWidth = this.TryFindResource("ToastWidth", out var width) && width is double max ? max : double.PositiveInfinity,
            Children =
            {
                new TextBlock { Text = Localizer.Get("update.confirm").Replace("{version}", result.Surum), TextWrapping = TextWrapping.Wrap },
                new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = gap, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Children = { later, install } }
            }
        };
        await dialog.ShowDialog(this);
        return accepted;
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
