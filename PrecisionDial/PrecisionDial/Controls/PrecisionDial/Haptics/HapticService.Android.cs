using Android.Content;
using Android.OS;

namespace PrecisionDial.Controls;

public partial class HapticService
{
    private Vibrator? _vibrator;

    public partial void Prepare()
    {
        _vibrator = (Vibrator?)Android.App.Application.Context
            .GetSystemService(Context.VibratorService);
    }

    public partial void FireDetentClick()
    {
        if (_vibrator is null) return;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            _vibrator.Vibrate(VibrationEffect.CreatePredefined(VibrationEffect.EffectTick));
        else
        {
#pragma warning disable CS0618
            _vibrator.Vibrate(5);
#pragma warning restore CS0618
        }
    }

    public partial void FireMajorDetentClick()
    {
        if (_vibrator is null) return;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            _vibrator.Vibrate(VibrationEffect.CreatePredefined(VibrationEffect.EffectClick));
        else _vibrator.Vibrate(10);
    }

    public partial void FireBoundaryStop()
    {
        if (_vibrator is null) return;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            _vibrator.Vibrate(VibrationEffect.CreatePredefined(VibrationEffect.EffectHeavyClick));
        else _vibrator.Vibrate(30);
    }

    public partial void FireSelectionTick() => FireDetentClick();

    public partial void Release() { _vibrator = null; }
}
