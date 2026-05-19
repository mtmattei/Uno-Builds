namespace YouTubeMs.Services.Motion;

public static class Motion
{
    public static bool ReducedMotion { get; private set; }

    public static void Initialize()
    {
        try
        {
            var settings = new Windows.UI.ViewManagement.UISettings();
            ReducedMotion = !settings.AnimationsEnabled;
        }
        catch
        {
            ReducedMotion = false;
        }
    }
}
