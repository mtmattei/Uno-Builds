using System.ComponentModel;
using Composer.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Composer.Views.Layers;

/// <summary>
/// Layer 6 — Data. Entity cards with field/type pairs. Read-only in brief 02.
/// </summary>
public sealed partial class DataContractGrid : UserControl
{
    private INotifyPropertyChanged? _vm;

    public DataContractGrid()
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
        if (e.PropertyName == "Data")
            DispatcherQueue?.TryEnqueue(Render);
    }

    private void Render()
    {
        var data = Composer.Views.MvuxValueReader.Read<DataContracts>(DataContext, "Data")
                   ?? DataContracts.Default;
        EntityRow.Children.Clear();

        MetaText.Text = $"·  {data.Entities.Length} RECORDS";
        var monoFont = (FontFamily)Application.Current.Resources["MonoFontFamily"];

        foreach (var ent in data.Entities)
        {
            var card = new Border
            {
                Background = AppBrushes.Paper2,
                BorderBrush = AppBrushes.Hairline,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(16, 12, 16, 12),
                MinWidth = 220,
            };

            var stack = new StackPanel { Spacing = 8 };

            stack.Children.Add(new TextBlock
            {
                Text = $"{ent.Name} ({ent.Kind.ToString().ToLowerInvariant()})",
                FontFamily = monoFont,
                FontSize = 11,
                CharacterSpacing = 160,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = AppBrushes.Ink,
            });

            foreach (var f in ent.Fields)
            {
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var n = new TextBlock
                {
                    Text = f.Name,
                    FontFamily = monoFont,
                    FontSize = 12,
                    Foreground = AppBrushes.Ink,
                };
                Grid.SetColumn(n, 0);
                row.Children.Add(n);

                var t = new TextBlock
                {
                    Text = f.TypeText,
                    FontFamily = monoFont,
                    FontSize = 12,
                    Foreground = AppBrushes.Indigo,
                };
                Grid.SetColumn(t, 1);
                row.Children.Add(t);

                stack.Children.Add(row);
            }

            card.Child = stack;
            EntityRow.Children.Add(card);
        }
    }
}
