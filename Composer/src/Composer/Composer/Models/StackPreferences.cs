using System.Collections.Immutable;

namespace Composer.Models;

public enum StackPattern   { Mvux, Mvvm, MvuxMessaging }
public enum MarkupKind     { Xaml, CSharpMarkup }
public enum RendererKind   { Skia, Native }
public enum HttpClientKind { Kiota, Refit, None }
public enum NavigationKind { Region, Frame, None }
public enum ThemeKind      { Material, Fluent, Cupertino, Custom }
public enum PlatformTarget { Wasm, iOS, Android, MacOS, Windows, Linux }

/// <summary>
/// Layer 0 — Stack Preferences. The user's stack defaults captured before
/// composing. Every downstream MARKDOWN_GEN entry references these so the
/// produced briefs are stack-correct (MVUX vs MVVM, Skia vs Native,
/// Material vs Cupertino, etc.) even without AI augmentation.
///
/// Spec: <c>docs/ENGINEERING-BRIEF-01-stack-preferences.md</c>.
///
/// Defaults match canonical Uno conventions per Matt's user preferences:
/// MVUX, Region nav, Material theme, Skia renderer, Kiota HTTP, XAML markup.
/// Platforms default to the four most-shipped Uno targets.
/// </summary>
public record StackPreferences(
    StackPattern Pattern,
    MarkupKind Markup,
    RendererKind Renderer,
    HttpClientKind Http,
    NavigationKind Nav,
    ThemeKind Theme,
    ImmutableHashSet<PlatformTarget> Platforms)
{
    public static StackPreferences Default { get; } = new(
        Pattern:   StackPattern.Mvux,
        Markup:    MarkupKind.Xaml,
        Renderer:  RendererKind.Skia,
        Http:      HttpClientKind.Kiota,
        Nav:       NavigationKind.Region,
        Theme:     ThemeKind.Material,
        Platforms: ImmutableHashSet.Create(
            PlatformTarget.Wasm,
            PlatformTarget.iOS,
            PlatformTarget.Android,
            PlatformTarget.Windows));

    /// <summary>True when the chosen pattern is MVUX or MVUX + Messaging —
    /// both branches produce identical reactive-feeds output downstream.</summary>
    public bool IsMvux => Pattern is StackPattern.Mvux or StackPattern.MvuxMessaging;

    /// <summary>Display labels used by the canvas + markdown generation.
    /// Single source of truth so the segmented buttons, locked card, and
    /// markdown all spell the values the same way.</summary>
    public static string Label(StackPattern v) => v switch
    {
        StackPattern.Mvux           => "MVUX",
        StackPattern.Mvvm           => "MVVM",
        StackPattern.MvuxMessaging  => "MVUX + Messaging",
        _ => v.ToString(),
    };

    public static string Label(MarkupKind v) => v switch
    {
        MarkupKind.Xaml         => "XAML",
        MarkupKind.CSharpMarkup => "C# Markup",
        _ => v.ToString(),
    };

    public static string Label(RendererKind v) => v.ToString();
    public static string Label(HttpClientKind v) => v.ToString();
    public static string Label(NavigationKind v) => v.ToString();
    public static string Label(ThemeKind v) => v.ToString();
    public static string Label(PlatformTarget v) => v switch
    {
        PlatformTarget.MacOS => "macOS",
        _                    => v.ToString(),
    };
}
