using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AbxPilot.UI.ViewModels;
using AbxPilot.UI.Views;

namespace AbxPilot.UI;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var model = new MainViewModel();

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
