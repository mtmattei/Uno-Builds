namespace QuoteCraft.Presentation;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
    }

    private async void UploadLogo_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".webp");

#if !__BROWSERWASM__
        WinRT.Interop.InitializeWithWindow.Initialize(picker,
            WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow));
#endif

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;

        // Copy to local data directory
        var logoDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuoteCraft", "logo");
        Directory.CreateDirectory(logoDir);

        var ext = Path.GetExtension(file.Name);
        var destPath = Path.Combine(logoDir, $"logo{ext}");
        var sourceBytes = await File.ReadAllBytesAsync(file.Path);
        await File.WriteAllBytesAsync(destPath, sourceBytes);

        // Update model
        if (DataContext is SettingsModel model)
            await model.SetLogoPath(destPath, CancellationToken.None);
    }
}
