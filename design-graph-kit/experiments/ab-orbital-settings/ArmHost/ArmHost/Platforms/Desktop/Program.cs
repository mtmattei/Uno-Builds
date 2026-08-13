using Uno.UI.Hosting;

namespace ArmHost;

internal class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        App.InitializeLogging();

        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App())
            .UseX11()
            .UseLinuxFrameBuffer()
            .UseMacOS()
            .UseWin32()
            .Build();

        // Uno.Sdk is pinned to 6.5.31 here to match the Orbital app the arms
        // replicate; that host requires RunAsync, while the newer template this
        // project was scaffolded from emits the synchronous Run().
        await host.RunAsync();
    }
}
