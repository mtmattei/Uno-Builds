using Microsoft.UI.Xaml.Controls;
using Unoblueprint.Models;

namespace Unoblueprint.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        InitializeDemo();
    }

    private void InitializeDemo()
    {
        var plugin = new PluginInfo
        {
            Name = "Wordsometric",
            Description = "Wordsometric allows you to create isometric layers without manually having to set them up.",
            Category = "Third-party payment",
            ActiveInstallations = 300000,
            IsInstalled = false
        };

        PluginCardControl.Plugin = plugin;
    }
}
