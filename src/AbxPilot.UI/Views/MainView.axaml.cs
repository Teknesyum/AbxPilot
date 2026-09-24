using System.ComponentModel;
using AbxPilot.UI.Choreography;
using AbxPilot.UI.ViewModels;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AbxPilot.UI.Views;

public partial class MainView : UserControl
{
    private static readonly TimeSpan HoldDelay = TimeSpan.FromMilliseconds(450);
    private static readonly TimeSpan HoldAutoClose = TimeSpan.FromSeconds(3.5);
    private const double HoldMoveTolerance = 12;

    private MainViewModel? _vm;
    private readonly Dictionary<object, Point> _flip = [];
    private CancellationTokenSource? _sweep;
    private DispatcherTimer? _holdTimer;
    private Border? _holdTarget;
    private Point _holdOrigin;

    public MainView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Attach(DataContext as MainViewModel);
        SizeChanged += (_, _) => Arrange();
        EmptyAction.Click += OnEmptyAction;
        SourceLink.Click += OnSourceLink;
        DrawerScrim.PointerPressed += (_, _) => _vm?.CloseDrawerCommand.Execute(null);
        AddHandler(PointerPressedEvent, OnScorePointerPressed);
        AddHandler(PointerMovedEvent, OnScorePointerMoved);
        AddHandler(PointerReleasedEvent, OnScorePointerReleased);
        AddHandler(PointerCaptureLostEvent, OnScorePointerCaptureLost);
        AttachedToVisualTree += (_, _) =>
        {
            CardGlow.Margin = Negate(Resource<Thickness>("PanelPadding"));
            DurationGlow.Margin = new Thickness(-Resource<double>("Space2"));
            Sweep();
        };
        DetachedFromVisualTree += (_, _) => _sweep?.Cancel();
    }

    private void OnScorePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        CancelHold();
        if (FindScoreCard(e.Source) is not { } card) return;
        if (card.DataContext is not AlternativeRow { HasScore: true }) return;
        _holdTarget = card;
        _holdOrigin = e.GetPosition(this);
        _holdTimer = new DispatcherTimer { Interval = HoldDelay };
        _holdTimer.Tick += OnHoldElapsed;
        _holdTimer.Start();
    }

    private void OnScorePointerMoved(object? sender, PointerEventArgs e)
    {
        if (_holdTarget is null) return;
        var now = e.GetPosition(this);
        if (Math.Abs(now.X - _holdOrigin.X) > HoldMoveTolerance || Math.Abs(now.Y - _holdOrigin.Y) > HoldMoveTolerance)
            CancelHold();
    }

    private void OnScorePointerReleased(object? sender, PointerEventArgs e) => CancelHold();

    private void OnScorePointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) => CancelHold();

    private void OnHoldElapsed(object? sender, EventArgs e)
    {
        CancelHold();
        if (_holdTarget is not { } card) return;
        ToolTip.SetIsOpen(card, true);
        DispatcherTimer.RunOnce(() => ToolTip.SetIsOpen(card, false), HoldAutoClose);
    }

    private void CancelHold()
    {
        if (_holdTimer is { } timer)
        {
            timer.Stop();
            timer.Tick -= OnHoldElapsed;
        }
        _holdTimer = null;
        _holdTarget = null;
    }

    private Border? FindScoreCard(object? source) =>
        (source as Visual)?.GetSelfAndVisualAncestors().OfType<Border>().FirstOrDefault(border => border.Classes.Contains("alt"));

    private void Attach(MainViewModel? vm)
    {
        if (_vm is not null)
        {
            _vm.PropertyChanged -= OnVmChanged;
            _vm.Applying -= OnApplying;
            _vm.Applied -= OnApplied;
        }
        _vm = vm;
        if (vm is null) return;
        vm.PropertyChanged += OnVmChanged;
        vm.Applying += OnApplying;
        vm.Applied += OnApplied;
        Arrange();
        Overlays(false);
        Sweep();
    }

    private void OnVmChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.IsDrawerOpen):
            case nameof(MainViewModel.IsSettingsOpen):
            case nameof(MainViewModel.IsCompact):
                Overlays(true);
                break;
            case nameof(MainViewModel.State):
                Sweep();
                break;
        }
    }

    private void Arrange()
    {
        if (_vm is null || Bounds.Width <= 0) return;
        var compact = Bounds.Width < Resource<double>("CompactBreakpoint");
        _vm.IsCompact = compact;
        Main.Margin = new Thickness(compact ? 0 : Resource<double>("SidebarWidth") + Resource<double>("SectionGap"), 0, 0, 0);
        CardScroll.MaxHeight = Math.Max(Resource<double>("InputHeight"), Bounds.Height * Resource<double>("CardHeightShare"));
        SettingsPanel.MaxHeight = Math.Max(0, Body.Bounds.Height);
        Grid.SetRow(AppTitle, compact ? 1 : 0);
        Grid.SetColumn(AppTitle, 0);
        Grid.SetColumnSpan(AppTitle, compact ? 4 : 2);
        Grid.SetColumn(SpectrumStrip, compact ? 0 : 1);
        Grid.SetRow(SpectrumStrip, compact ? 1 : 0);
        Grid.SetColumnSpan(SpectrumStrip, compact ? 2 : 1);
        Grid.SetColumnSpan(QuestionPanel, compact ? 2 : 1);
    }

    private void Overlays(bool animate)
    {
        if (_vm is null) return;
        var drawer = _vm.IsCompact && _vm.IsDrawerOpen;
        var rail = !_vm.IsCompact || _vm.IsDrawerOpen;
        var railWasVisible = SyndromeRail.IsVisible;
        SyndromeRail.IsVisible = rail;
        DrawerScrim.IsVisible = drawer;
        if (animate && drawer && !railWasVisible) Enter(SyndromeRail, -Resource<double>("EntryOffset"), 0);
        var settingsWasVisible = SettingsPanel.IsVisible;
        SettingsPanel.IsVisible = _vm.IsSettingsOpen;
        if (animate && _vm.IsSettingsOpen && !settingsWasVisible) Enter(SettingsPanel, Resource<double>("EntryOffset"), 0);
        if (_vm.IsSettingsOpen) Dispatcher.UIThread.Post(() => ScoreSwitch.Focus(NavigationMethod.Tab), DispatcherPriority.Loaded);
        if (drawer) Dispatcher.UIThread.Post(() => SearchBox.Focus(NavigationMethod.Tab), DispatcherPriority.Loaded);
    }

    private void OnEmptyAction(object? sender, RoutedEventArgs e)
    {
        if (_vm?.IsCompact == true && !_vm.IsDrawerOpen) _vm.ToggleDrawerCommand.Execute(null);
        else SearchBox.Focus(NavigationMethod.Tab);
    }

    private async void OnSourceLink(object? sender, RoutedEventArgs e)
    {
        if (_vm?.SourceUrl is not { } url || !Uri.TryCreate(url, UriKind.Absolute, out var uri)) return;
        if (TopLevel.GetTopLevel(this) is { } top) await top.Launcher.LaunchUriAsync(uri);
    }

    private void Sweep()
    {
        _sweep?.Cancel();
        _sweep = null;
        SkeletonSweep.IsVisible = false;
        if (_vm?.IsLoading != true || !Tokens.Animate || VisualRoot is null) return;
        SkeletonSweep.IsVisible = true;
        var width = SkeletonSweep.Width;
        _sweep = new CancellationTokenSource();
        var span = Math.Max(Resource<double>("SidebarWidth") * 3, SkeletonHost.Bounds.Width + width);
        var loop = new Animation
        {
            Duration = Tokens.Time("LoadingLoopMin"),
            IterationCount = IterationCount.Infinite,
            Children =
            {
                new KeyFrame { Cue = new Cue(0), Setters = { new Avalonia.Styling.Setter(TranslateTransform.XProperty, -width) } },
                new KeyFrame { Cue = new Cue(1), Setters = { new Avalonia.Styling.Setter(TranslateTransform.XProperty, span) } },
            },
        };
        _ = loop.RunAsync(SkeletonSweep, _sweep.Token);
    }

    private void OnApplying(object? sender, EventArgs e) => Capture();

    private void Capture()
    {
        _flip.Clear();
        foreach (var row in Rows("alt"))
            if (row.DataContext is { } key && row.TranslatePoint(default, this) is { } at)
                _flip[key] = at;
    }

    private void Play()
    {
        if (!Tokens.Animate || _flip.Count == 0) return;
        var moves = new List<(Border Row, double Dy)>();
        foreach (var row in Rows("alt"))
        {
            if (row.DataContext is not { } key || !_flip.TryGetValue(key, out var before)) continue;
            if (row.TranslatePoint(default, this) is not { } now) continue;
            var dy = before.Y - now.Y;
            if (Math.Abs(dy) > 0.5) moves.Add((row, dy));
        }
        _flip.Clear();
        foreach (var (row, dy) in moves)
            Slide(row, 0, dy, Tokens.Time("TBase"));
    }

    private void OnApplied(object? sender, AppliedEventArgs e)
    {
        Dispatcher.UIThread.Post(() => Choreograph(e), DispatcherPriority.Loaded);
    }

    private void Choreograph(AppliedEventArgs e)
    {
        Play();
        var diff = e.Diff;
        var leaving = _vm?.Alternatives.Any(row => row.IsLeaving) == true;
        if (leaving)
        {
            if (Tokens.Animate) DispatcherTimer.RunOnce(DropLeaving, Tokens.Time("TFast"));
            else DropLeaving();
        }
        if (!Tokens.Animate || diff is null || diff.IsEmpty) return;
        foreach (var id in diff.QuestionsShown)
            if (Card(id) is { } shown) Enter(shown, 0, -Resource<double>("EntryOffset"));
        if (diff.DurationChanged) Pulse(DurationGlow);
        if (e.Trigger is { } trigger && Card(trigger) is { } origin) Trace(origin);
        else if (diff.FirstChoiceChanged || diff.StatusChanged) Pulse(CardGlow);
    }

    private void DropLeaving()
    {
        Capture();
        _vm?.DropLeaving();
        Dispatcher.UIThread.Post(Play, DispatcherPriority.Loaded);
    }

    private void Trace(Control origin)
    {
        var size = Resource<double>("Space3");
        if (origin.TranslatePoint(new Point(origin.Bounds.Width / 2, origin.Bounds.Height / 2), TraceLayer) is not { } from) return;
        if (CardScroll.TranslatePoint(new Point(CardScroll.Bounds.Width / 2, CardScroll.Bounds.Height), TraceLayer) is not { } to) return;
        TraceDot.Transitions = null;
        TraceDot.RenderTransform = Translate(from.X - size / 2, from.Y - size / 2);
        TraceDot.Opacity = 1;
        var slow = Tokens.Time("TSlow");
        TraceDot.Transitions =
        [
            new TransformOperationsTransition { Property = RenderTransformProperty, Duration = slow, Easing = Tokens.Ease("EOut") },
            new DoubleTransition { Property = OpacityProperty, Duration = Tokens.Time("TFast"), Easing = Tokens.Ease("EIn") },
        ];
        TraceDot.RenderTransform = Translate(to.X - size / 2, to.Y - size / 2);
        DispatcherTimer.RunOnce(() =>
        {
            TraceDot.Opacity = 0;
            Pulse(CardGlow);
        }, slow);
    }

    private static void Pulse(Border glow)
    {
        glow.Transitions = null;
        glow.Opacity = 0;
        glow.Transitions =
        [
            new DoubleTransition { Property = OpacityProperty, Duration = Tokens.Time("TFast"), Easing = Tokens.Ease("EOut") },
        ];
        glow.Opacity = 1;
        DispatcherTimer.RunOnce(() =>
        {
            glow.Transitions =
            [
                new DoubleTransition { Property = OpacityProperty, Duration = Tokens.Time("TBase"), Easing = Tokens.Ease("EIn") },
            ];
            glow.Opacity = 0;
        }, Tokens.Time("TBase"));
    }

    private static void Enter(Control control, double dx, double dy)
    {
        if (!Tokens.Animate) return;
        control.Transitions = null;
        control.Opacity = 0;
        control.RenderTransform = Translate(dx, dy);
        var time = Tokens.Time("TBase");
        var ease = Tokens.Ease("EOut");
        control.Transitions =
        [
            new TransformOperationsTransition { Property = RenderTransformProperty, Duration = time, Easing = ease },
            new DoubleTransition { Property = OpacityProperty, Duration = time, Easing = ease },
        ];
        control.Opacity = 1;
        control.RenderTransform = TransformOperations.Identity;
        DispatcherTimer.RunOnce(() => control.ClearValue(TransitionsProperty), time);
    }

    private static void Slide(Control control, double dx, double dy, TimeSpan time)
    {
        var kept = control.Transitions;
        control.Transitions = null;
        control.RenderTransform = Translate(dx, dy);
        control.Transitions =
        [
            new TransformOperationsTransition { Property = RenderTransformProperty, Duration = time, Easing = Tokens.Ease("EOut") },
            new DoubleTransition { Property = OpacityProperty, Duration = Tokens.Time("TFast"), Easing = Tokens.Ease("EIn") },
        ];
        control.RenderTransform = TransformOperations.Identity;
        DispatcherTimer.RunOnce(() => control.ClearValue(TransitionsProperty), time);
    }

    private static ITransform Translate(double x, double y)
    {
        var builder = TransformOperations.CreateBuilder(1);
        builder.AppendTranslate(x, y);
        return builder.Build();
    }

    private IEnumerable<Border> Rows(string className) =>
        this.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains(className) && border.IsEffectivelyVisible);

    private Border? Card(string questionId) =>
        this.GetVisualDescendants().OfType<Border>()
            .FirstOrDefault(border => border.Classes.Contains("qcard") && border.DataContext is QuestionCard card && card.Id == questionId && border.IsEffectivelyVisible);

    private T Resource<T>(string key) where T : struct =>
        this.TryFindResource(key, ActualThemeVariant, out var value) && value is T typed ? typed : default;

    private static Thickness Negate(Thickness value) => new(-value.Left, -value.Top, -value.Right, -value.Bottom);
}
