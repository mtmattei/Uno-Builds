using System.Diagnostics;
using ClaudeDash.Helpers;
using ClaudeDash.Models;

namespace ClaudeDash.Controls;

public sealed partial class ToolchainRow : UserControl
{
    private static readonly Color GreenDot = ColorHelper.FromArgb(255, 74, 222, 128);   // #4ADE80
    private static readonly Color YellowDot = ColorHelper.FromArgb(255, 251, 191, 36);  // #FBBF24
    private static readonly Color RedDot = ColorHelper.FromArgb(255, 239, 68, 68);      // #EF4444
    private static readonly Color GreyDot = ColorHelper.FromArgb(255, 100, 100, 106);   // #64646A

    private string _fixCommand = "";

    public ToolchainRow()
    {
        this.InitializeComponent();
    }

    public void Bind(ToolchainItem item)
    {
        _fixCommand = item.FixCommand;

        var dotColor = item.Status switch
        {
            ToolchainStatus.Pass => GreenDot,
            ToolchainStatus.Warn => YellowDot,
            ToolchainStatus.Fail => RedDot,
            _ => GreyDot
        };

        StatusDot.Fill = new SolidColorBrush(dotColor);
        NameText.Text = item.Name.ToLowerInvariant();
        VersionText.Text = item.Version.ToLowerInvariant();
        DetailText.Text = item.Detail.ToLowerInvariant();

        if (!string.IsNullOrEmpty(item.FixCommand))
        {
            FixButton.Visibility = Visibility.Visible;
            FixButtonText.Text = !string.IsNullOrEmpty(item.FixLabel)
                ? item.FixLabel.ToLowerInvariant()
                : "fix";
        }
        else
        {
            FixButton.Visibility = Visibility.Collapsed;
        }

        UiHelpers.AttachHover(RootGrid);
    }

    private void FixButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_fixCommand)) return;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start cmd /k \"{_fixCommand}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(psi);
        }
        catch { }
    }
}
