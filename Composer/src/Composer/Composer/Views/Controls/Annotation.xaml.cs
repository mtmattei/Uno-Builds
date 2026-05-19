using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Composer.Views.Controls;

/// <summary>
/// Editorial marginalia primitive — italic body behind a 1px left rule.
/// Spec: <c>docs/DESIGN-BRIEF.md</c> §4.3.
///
/// Two voices controlled by <see cref="Voice"/>:
///   <c>rationale</c> (default): plain italic, no quote marks.
///   <c>quote</c>:                italic wrapped in "..." — for content the
///                                agent will receive verbatim.
///
/// Set <see cref="Eyebrow"/> + <see cref="Body"/> + (optionally)
/// <see cref="Voice"/>. The control is read-only; updating any property
/// re-renders.
/// </summary>
public sealed partial class Annotation : UserControl
{
    public static readonly DependencyProperty EyebrowProperty =
        DependencyProperty.Register(nameof(Eyebrow), typeof(string), typeof(Annotation),
            new PropertyMetadata(string.Empty, OnContentChanged));

    // Body DP is object — MVUX wraps IFeed<string> in a BindableXxxViewModel
    // proxy; a string-typed DP fails the TypeConverter check. We unwrap to
    // string in Apply() via MvuxValueReader, matching the ActiveLayerHeader
    // pattern. XAML literals ("Some text") still bind fine — string is an object.
    public static readonly DependencyProperty BodyProperty =
        DependencyProperty.Register(nameof(Body), typeof(object), typeof(Annotation),
            new PropertyMetadata(null, OnContentChanged));

    /// <summary>"rationale" (default) or "quote". Quote wraps the body in
    /// curly double-quote marks.</summary>
    public static readonly DependencyProperty VoiceProperty =
        DependencyProperty.Register(nameof(Voice), typeof(string), typeof(Annotation),
            new PropertyMetadata("rationale", OnContentChanged));

    public string Eyebrow
    {
        get => (string)GetValue(EyebrowProperty);
        set => SetValue(EyebrowProperty, value);
    }

    public object? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public string Voice
    {
        get => (string)GetValue(VoiceProperty);
        set => SetValue(VoiceProperty, value);
    }

    public Annotation()
    {
        this.InitializeComponent();
        Apply();
    }

    private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Annotation a) a.Apply();
    }

    private void Apply()
    {
        EyebrowText.Text = (Eyebrow ?? string.Empty).ToUpperInvariant();
        var body = Body switch
        {
            string s => s,
            null     => string.Empty,
            _        => Composer.Views.MvuxValueReader.Unwrap<string>(Body) ?? string.Empty,
        };
        BodyText.Text = string.Equals(Voice, "quote", System.StringComparison.OrdinalIgnoreCase)
            ? $"“{body}”"  // “ … ”
            : body;
    }
}
