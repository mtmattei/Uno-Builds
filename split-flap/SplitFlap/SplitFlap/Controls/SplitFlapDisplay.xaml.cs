using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SplitFlap.Controls;

public sealed partial class SplitFlapDisplay : UserControl
{
    private readonly List<SplitFlapCard> _cards = new();

    public SplitFlapDisplay()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CreateCards();
        UpdateDisplay();
    }

    #region Dependency Properties

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(SplitFlapDisplay),
            new PropertyMetadata("", OnValueChanged));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapDisplay display)
        {
            display.UpdateDisplay();
        }
    }

    public static readonly DependencyProperty LengthProperty =
        DependencyProperty.Register(
            nameof(Length),
            typeof(int),
            typeof(SplitFlapDisplay),
            new PropertyMetadata(6, OnLengthChanged));

    public int Length
    {
        get => (int)GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    private static void OnLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapDisplay display)
        {
            display.CreateCards();
            display.UpdateDisplay();
        }
    }

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(
            nameof(Theme),
            typeof(SplitFlapTheme),
            typeof(SplitFlapDisplay),
            new PropertyMetadata(SplitFlapTheme.Dark, OnThemeChanged));

    public SplitFlapTheme Theme
    {
        get => (SplitFlapTheme)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapDisplay display)
        {
            display.ApplyThemeToCards();
        }
    }

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(
            nameof(Size),
            typeof(SplitFlapSize),
            typeof(SplitFlapDisplay),
            new PropertyMetadata(SplitFlapSize.Medium, OnSizeChanged));

    public SplitFlapSize Size
    {
        get => (SplitFlapSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapDisplay display)
        {
            display.ApplySizeToCards();
        }
    }

    public static readonly DependencyProperty PadCharacterProperty =
        DependencyProperty.Register(
            nameof(PadCharacter),
            typeof(string),
            typeof(SplitFlapDisplay),
            new PropertyMetadata(" ", OnPadCharacterChanged));

    public string PadCharacter
    {
        get => (string)GetValue(PadCharacterProperty);
        set => SetValue(PadCharacterProperty, value);
    }

    private static void OnPadCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapDisplay display)
        {
            display.UpdateDisplay();
        }
    }

    public static readonly DependencyProperty PadSideProperty =
        DependencyProperty.Register(
            nameof(PadSide),
            typeof(PadSide),
            typeof(SplitFlapDisplay),
            new PropertyMetadata(PadSide.Left, OnPadSideChanged));

    public PadSide PadSide
    {
        get => (PadSide)GetValue(PadSideProperty);
        set => SetValue(PadSideProperty, value);
    }

    private static void OnPadSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapDisplay display)
        {
            display.UpdateDisplay();
        }
    }

    public static readonly DependencyProperty StaggerDelayProperty =
        DependencyProperty.Register(
            nameof(StaggerDelay),
            typeof(int),
            typeof(SplitFlapDisplay),
            new PropertyMetadata(50));

    public int StaggerDelay
    {
        get => (int)GetValue(StaggerDelayProperty);
        set => SetValue(StaggerDelayProperty, value);
    }

    public static readonly DependencyProperty FlipDurationProperty =
        DependencyProperty.Register(
            nameof(FlipDuration),
            typeof(TimeSpan),
            typeof(SplitFlapDisplay),
            new PropertyMetadata(TimeSpan.FromMilliseconds(150), OnFlipDurationChanged));

    public TimeSpan FlipDuration
    {
        get => (TimeSpan)GetValue(FlipDurationProperty);
        set => SetValue(FlipDurationProperty, value);
    }

    private static void OnFlipDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapDisplay display)
        {
            foreach (var card in display._cards)
            {
                card.FlipDuration = display.FlipDuration;
            }
        }
    }

    #endregion

    #region Events

    public event EventHandler<DisplayFlipEventArgs>? FlipStart;
    public event EventHandler<DisplayFlipEventArgs>? FlipEnd;
    public event EventHandler<DisplayValueChangedEventArgs>? ValueChanged;

    #endregion

    private void CreateCards()
    {
        CardsPanel.Children.Clear();
        _cards.Clear();

        for (int i = 0; i < Length; i++)
        {
            var card = new SplitFlapCard
            {
                Theme = Theme,
                Size = Size,
                FlipDuration = FlipDuration,
                Character = PadCharacter
            };

            int position = i;
            card.FlipStarted += (s, e) => OnCardFlipStarted(position, e);
            card.FlipCompleted += (s, e) => OnCardFlipCompleted(position, e);

            _cards.Add(card);
            CardsPanel.Children.Add(card);
        }
    }

    private async void UpdateDisplay()
    {
        if (_cards.Count == 0) return;

        var value = Value ?? "";
        var paddedValue = PadValue(value.ToUpperInvariant());

        for (int i = 0; i < _cards.Count; i++)
        {
            var targetChar = i < paddedValue.Length ? paddedValue[i].ToString() : PadCharacter;

            if (StaggerDelay > 0 && i > 0)
            {
                await Task.Delay(StaggerDelay);
            }

            _cards[i].Character = targetChar;
        }

        ValueChanged?.Invoke(this, new DisplayValueChangedEventArgs(value));
    }

    private string PadValue(string value)
    {
        if (value.Length >= Length)
        {
            return value.Substring(0, Length);
        }

        var padCount = Length - value.Length;
        var padding = new string(PadCharacter.Length > 0 ? PadCharacter[0] : ' ', padCount);

        return PadSide == PadSide.Left
            ? padding + value
            : value + padding;
    }

    private void ApplyThemeToCards()
    {
        foreach (var card in _cards)
        {
            card.Theme = Theme;
        }
    }

    private void ApplySizeToCards()
    {
        foreach (var card in _cards)
        {
            card.Size = Size;
        }
    }

    private void OnCardFlipStarted(int position, FlipEventArgs e)
    {
        FlipStart?.Invoke(this, new DisplayFlipEventArgs(position, e.FromCharacter, e.ToCharacter));
    }

    private void OnCardFlipCompleted(int position, FlipEventArgs e)
    {
        FlipEnd?.Invoke(this, new DisplayFlipEventArgs(position, e.FromCharacter, e.ToCharacter));
    }
}

public enum PadSide
{
    Left,
    Right
}

public class DisplayFlipEventArgs : EventArgs
{
    public int Position { get; }
    public string FromCharacter { get; }
    public string ToCharacter { get; }

    public DisplayFlipEventArgs(int position, string fromCharacter, string toCharacter)
    {
        Position = position;
        FromCharacter = fromCharacter;
        ToCharacter = toCharacter;
    }
}

public class DisplayValueChangedEventArgs : EventArgs
{
    public string Value { get; }

    public DisplayValueChangedEventArgs(string value)
    {
        Value = value;
    }
}
