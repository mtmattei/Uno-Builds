using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Unoblueprint.Models;

namespace Unoblueprint.Controls;

public sealed partial class PluginCard : UserControl
{
    public static readonly DependencyProperty PluginProperty =
        DependencyProperty.Register(
            nameof(Plugin),
            typeof(PluginInfo),
            typeof(PluginCard),
            new PropertyMetadata(null, OnPluginChanged));

    public PluginInfo? Plugin
    {
        get => (PluginInfo?)GetValue(PluginProperty);
        set => SetValue(PluginProperty, value);
    }

    public PluginCard()
    {
        this.InitializeComponent();
    }

    private static void OnPluginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PluginCard card && e.NewValue is PluginInfo plugin)
        {
            card.UpdateUI(plugin);
        }
    }

    private void UpdateUI(PluginInfo plugin)
    {
        CategoryText.Text = plugin.Category;
        PluginNameText.Text = plugin.Name;
        DescriptionText.Text = plugin.Description;
        InstallBtn.IsInstalled = plugin.IsInstalled;

        // Update logo text to first letter of plugin name
        if (!string.IsNullOrEmpty(plugin.Name))
        {
            LogoText.Text = plugin.Name[0].ToString();
        }

        // Format installations count with comma formatting like "300,00"
        if (plugin.ActiveInstallations >= 1000000)
        {
            var millions = plugin.ActiveInstallations / 1000000.0;
            InstallationsText.Text = $"{millions:0.00} active installations".Replace(".", ",");
        }
        else if (plugin.ActiveInstallations >= 1000)
        {
            var thousands = plugin.ActiveInstallations / 1000.0;
            InstallationsText.Text = $"{thousands:0.00} active installations".Replace(".", ",");
        }
        else
        {
            InstallationsText.Text = $"{plugin.ActiveInstallations} active installations";
        }
    }
}
