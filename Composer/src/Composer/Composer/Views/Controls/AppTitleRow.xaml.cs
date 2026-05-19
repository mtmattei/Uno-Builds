using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Composer.Views.Controls;

/// <summary>
/// Project name row above the canvas. Per
/// <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §7.1 — slot 2 in the
/// canonical page template, between progress and locked cards.
///
/// Two DPs:
///   <see cref="ProjectName"/> — live string from Intent.AppType.
///   <see cref="ShowReset"/>   — bool gating the Reset link's visibility
///                                (true once HasLockedLayers flips true).
///
/// Both typed object so MVUX bindable wrappers around the feeds can be
/// unwrapped via MvuxValueReader.
/// </summary>
public sealed partial class AppTitleRow : UserControl
{
    public static readonly DependencyProperty ProjectNameProperty =
        DependencyProperty.Register(
            nameof(ProjectName), typeof(object), typeof(AppTitleRow),
            new PropertyMetadata(null, OnProjectNameChanged));

    public object? ProjectName
    {
        get => GetValue(ProjectNameProperty);
        set => SetValue(ProjectNameProperty, value);
    }

    public static readonly DependencyProperty ShowResetProperty =
        DependencyProperty.Register(
            nameof(ShowReset), typeof(object), typeof(AppTitleRow),
            new PropertyMetadata(null, OnShowResetChanged));

    public object? ShowReset
    {
        get => GetValue(ShowResetProperty);
        set => SetValue(ShowResetProperty, value);
    }

    public AppTitleRow() => this.InitializeComponent();

    private static void OnProjectNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AppTitleRow row) return;
        var name = e.NewValue switch
        {
            string s => s,
            null     => null,
            _        => Composer.Views.MvuxValueReader.Unwrap<string>(e.NewValue),
        };
        row.ProjectNameText.Text = string.IsNullOrWhiteSpace(name) ? "Untitled" : name!;
    }

    private static void OnShowResetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AppTitleRow row) return;
        var show = AsBool(e.NewValue);
        row.ResetButton.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Unwrap a bool from an MVUX bindable feed wrapper. The
    /// generic <see cref="Composer.Views.MvuxValueReader"/> is class-typed
    /// so it can't dispatch on <c>bool</c> directly; reflect for the
    /// canonical accessors here.</summary>
    private static bool AsBool(object? value)
    {
        if (value is bool b) return b;
        if (value is null) return false;
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var t = value.GetType();
        foreach (var name in new[] { "Value", "Current", "Model", "_value" })
        {
            if (t.GetProperty(name, Flags)?.GetValue(value) is bool pv) return pv;
            if (t.GetField(name, Flags)?.GetValue(value) is bool fv) return fv;
        }
        foreach (var prop in t.GetProperties(Flags))
            if (prop.PropertyType == typeof(bool) && prop.GetValue(value) is bool pv)
                return pv;
        return false;
    }

    private void OnResetClick(object sender, RoutedEventArgs e)
    {
        // Walk up to the host Shell/Page and invoke Reset on the bound
        // ComposerModel. Same reflection pattern as IntentCard / Stack.
        var host = FindParent<Page>(this);
        Composer.Views.MvuxCommandInvoker.Invoke(host?.DataContext, "Reset");
    }

    private static T? FindParent<T>(DependencyObject child) where T : class
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null && parent is not T)
            parent = VisualTreeHelper.GetParent(parent);
        return parent as T;
    }
}
