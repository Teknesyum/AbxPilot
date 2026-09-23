using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace AbxPilot.Android;

[Activity(
    Label = "AbxPilot",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity;
