using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace FieldCheck.Droid;

[Activity(
    MainLauncher = true,
    ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
    WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.StateHidden,
    Exported = true
)]
public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        global::AndroidX.Core.SplashScreen.SplashScreen.InstallSplashScreen(this);

        // Deterministic repository mode for state verification: adb shell am start ... --es data_mode error
        App.PlatformDataMode = Intent?.GetStringExtra("data_mode");

        base.OnCreate(savedInstanceState);
    }

}
