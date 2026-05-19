using System.Collections.Generic;
using Composer.Models;
using Composer.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Composer.Views.Controls;

/// <summary>
/// One locked layer's compressed summary card. Stacks above the active
/// canvas — by layer 8 there are seven of these visible. Header + italic
/// summary sentence + four-fact detail grid + Revisit link.
/// </summary>
public sealed partial class LockedContextCard : UserControl
{
    public static readonly DependencyProperty LayerKindProperty =
        DependencyProperty.Register(
            nameof(LayerKind), typeof(LayerKind), typeof(LockedContextCard),
            new PropertyMetadata(Composer.Models.LayerKind.Architecture, OnLayerKindChanged));

    public static readonly DependencyProperty SummaryProperty =
        DependencyProperty.Register(
            nameof(Summary), typeof(string), typeof(LockedContextCard),
            new PropertyMetadata("", OnSummaryChanged));

    public LayerKind LayerKind
    {
        get => (LayerKind)GetValue(LayerKindProperty);
        set => SetValue(LayerKindProperty, value);
    }

    public string Summary
    {
        get => (string)GetValue(SummaryProperty);
        set => SetValue(SummaryProperty, value);
    }

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsExpanded), typeof(bool), typeof(LockedContextCard),
            new PropertyMetadata(true, OnIsExpandedChanged));

    /// <summary>Whether the summary + facts grid are visible. Two-way:
    /// the parent stack can bind to DefaultExpandedKinds to auto-collapse
    /// older locked layers; the chevron in the header toggles it locally
    /// as a manual override.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public IList<KeyValuePair<string, string>>? Facts { get; set; }

    public LockedContextCard()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => { Render(); ApplyExpansion(); };
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LockedContextCard c) c.ApplyExpansion();
    }

    private void ApplyExpansion()
    {
        var v = IsExpanded ? Visibility.Visible : Visibility.Collapsed;
        SummaryText.Visibility = v;
        DividerLine.Visibility = v;
        FactsGrid.Visibility   = v;
        ExpandToggle.Content   = IsExpanded ? "−" : "+";
    }

    private void OnExpandToggleClick(object sender, RoutedEventArgs e)
    {
        IsExpanded = !IsExpanded;
    }

    private static void OnLayerKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LockedContextCard c) c.Render();
    }

    private static void OnSummaryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LockedContextCard c) c.SummaryText.Text = (string?)e.NewValue ?? "";
    }

    private void Render()
    {
        var def = Composer.Models.Layers.Get(LayerKind);
        HeaderLabelRun.Text = def.Label.ToUpperInvariant();
        SummaryText.Text = Summary ?? string.Empty;

        FactsGrid.Children.Clear();
        if (Facts is null) return;

        var row = 0;
        foreach (var kv in Facts)
        {
            if (row > 3) break;

            var label = new TextBlock
            {
                Text = kv.Key.ToUpperInvariant(),
                FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
                FontSize = 10,
                CharacterSpacing = 160,
                Foreground = (Brush)Application.Current.Resources["Ink3Brush"],
                Margin = new Thickness(0, 4, 8, 4),
            };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, 0);
            FactsGrid.Children.Add(label);

            var value = new TextBlock
            {
                Text = kv.Value,
                FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
                FontSize = 12,
                Foreground = (Brush)Application.Current.Resources["InkBrush"],
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 4),
            };
            Grid.SetRow(value, row);
            Grid.SetColumn(value, 1);
            FactsGrid.Children.Add(value);

            row++;
        }
    }

    private void OnRevisitClick(object sender, RoutedEventArgs e)
    {
        var page = FindParent<Page>(this);
        MvuxCommandInvoker.Invoke(page?.DataContext, "Revisit", LayerKind);
    }

    private static T? FindParent<T>(DependencyObject child) where T : class
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null && parent is not T)
            parent = VisualTreeHelper.GetParent(parent);
        return parent as T;
    }
}
