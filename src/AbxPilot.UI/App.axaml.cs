using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AbxPilot.UI.ViewModels;
using AbxPilot.UI.Views;

namespace AbxPilot.UI;

public partial class App : Application
{
    public static MainViewModel? CurrentModel { get; private set; }

    public override void Initialize()
    {
        Animation.RegisterCustomAnimator<ITransform, TransformAnimator>();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var model = new MainViewModel();
        CurrentModel = model;

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new MainWindow { DataContext = model };
                break;
            case ISingleViewApplicationLifetime single:
                single.MainView = new MainView { DataContext = model };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
