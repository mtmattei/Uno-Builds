using System.ComponentModel;
using Composer.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Composer.Views.Layers;

/// <summary>
/// Layer 7 — Implementation. Linear phase rows with files + dependency.
/// Read-only in brief 02.
/// </summary>
public sealed partial class ImplementationPhaseGrid : UserControl
{
    private INotifyPropertyChanged? _vm;

    public ImplementationPhaseGrid()
    {
        this.InitializeComponent();
        this.DataContextChanged += (_, args) => Attach(args.NewValue);
        this.Unloaded            += (_, _)    => Detach();
    }

    private void Attach(object? dc)
    {
        Detach();
        if (dc is INotifyPropertyChanged inpc)
        {
            _vm = inpc;
            _vm.PropertyChanged += OnVmPropertyChanged;
        }
        Render();
    }

    private void Detach()
    {
        if (_vm is null) return;
        _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm = null;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Implementation")
            DispatcherQueue?.TryEnqueue(Render);
    }

    private void Render()
    {
        var plan = Composer.Views.MvuxValueReader.Read<BuildPlan>(DataContext, "Implementation")
                   ?? BuildPlan.Default;
        PhaseStack.Children.Clear();

        MetaText.Text = $"·  {plan.Phases.Length} PHASES";
        var monoFont = (FontFamily)Application.Current.Resources["MonoFontFamily"];
        var serifItalic = (FontFamily)Application.Current.Resources["SerifLightItalicFontFamily"];

        for (var i = 0; i < plan.Phases.Length; i++)
        {
            var p = plan.Phases[i];
            if (i > 0)
            {
                PhaseStack.Children.Add(new Border
                {
                    Height = 1,
                    Background = AppBrushes.Hairline,
                    Margin = new Thickness(0, 6, 0, 6),
                });
            }

            var row = new Grid();
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Padding = new Thickness(0, 8, 0, 8);

            row.Children.Add(MakeMono($"P{p.Number}", AppBrushes.Ink3, 11, col: 0));
            row.Children.Add(MakeMono(p.Label.ToUpperInvariant(), AppBrushes.Ink, 11, col: 1, semiBold: true));
            row.Children.Add(MakeSerifItalic(string.Join(" · ", p.Files), AppBrushes.Ink2, 13, col: 2));
            row.Children.Add(MakeMono(p.After == "—" ? "after —" : $"after {p.After}", AppBrushes.Ink3, 10, col: 3));

            PhaseStack.Children.Add(row);
        }

        TextBlock MakeMono(string text, Brush fg, double fontSize, int col, bool semiBold = false)
        {
            var t = new TextBlock
            {
                Text = text,
                FontFamily = monoFont,
                FontSize = fontSize,
                CharacterSpacing = 160,
                Foreground = fg,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = semiBold ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal,
            };
            Grid.SetColumn(t, col);
            return t;
        }

        TextBlock MakeSerifItalic(string text, Brush fg, double fontSize, int col)
        {
            var t = new TextBlock
            {
                Text = text,
                FontFamily = serifItalic,
                FontSize = fontSize,
                Foreground = fg,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(8, 0, 8, 0),
            };
            Grid.SetColumn(t, col);
            return t;
        }
    }
}
