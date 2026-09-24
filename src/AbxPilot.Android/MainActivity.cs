using AndroidX.Activity;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia.Android;
using AbxPilot.UI;

namespace AbxPilot.Android;

[Activity(
    Label = "AbxPilot",
    Theme = "@style/MainTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        OnBackPressedDispatcher.AddCallback(this, new DrawerBackCallback(this));
    }

    private sealed class DrawerBackCallback(MainActivity activity) : OnBackPressedCallback(true)
    {
        public override void HandleOnBackPressed()
        {
            if (App.CurrentModel is not { } model || (!model.IsSettingsOpen && !model.IsDrawerOpen))
            {
                Enabled = false;
                activity.OnBackPressedDispatcher.OnBackPressed();
                Enabled = true;
                return;
            }
            if (model.IsSettingsOpen) model.CloseSettingsCommand.Execute(null);
            else model.CloseDrawerCommand.Execute(null);
        }
    }
}
