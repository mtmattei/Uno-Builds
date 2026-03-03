using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI;

namespace SplitFlap.Controls;

public enum SplitFlapTheme
{
    Dark,
    Light,
    Vintage
}

public enum SplitFlapSize
{
    Small,
    Medium,
    Large
}

public sealed partial class SplitFlapCard : UserControl
{
    private string _displayChar = "0";
    private string _prevChar = "0";
    private bool _isFlipping = false;
    private bool _pendingFlip = false;
    private string _pendingChar = string.Empty;

    public SplitFlapCard()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        ApplyTheme();
        ApplySize();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateCharacterDisplay(_displayChar);
    }

    #region Dependency Properties

    public static readonly DependencyProperty CharacterProperty =
        DependencyProperty.Register(
            nameof(Character),
            typeof(string),
            typeof(SplitFlapCard),
            new PropertyMetadata("0", OnCharacterChanged));

    public string Character
    {
        get => (string)GetValue(CharacterProperty);
        set => SetValue(CharacterProperty, value);
    }

    private static void OnCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapCard card)
        {
            card.OnCharacterChanged((string)e.OldValue, (string)e.NewValue);
        }
    }

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(
            nameof(Theme),
            typeof(SplitFlapTheme),
            typeof(SplitFlapCard),
            new PropertyMetadata(SplitFlapTheme.Dark, OnThemeChanged));

    public SplitFlapTheme Theme
    {
        get => (SplitFlapTheme)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapCard card)
        {
            card.ApplyTheme();
        }
    }

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(
            nameof(Size),
            typeof(SplitFlapSize),
            typeof(SplitFlapCard),
            new PropertyMetadata(SplitFlapSize.Medium, OnSizeChanged));

    public SplitFlapSize Size
    {
        get => (SplitFlapSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapCard card)
        {
            card.ApplySize();
        }
    }

    public static readonly DependencyProperty FlipDurationProperty =
        DependencyProperty.Register(
            nameof(FlipDuration),
            typeof(TimeSpan),
            typeof(SplitFlapCard),
            new PropertyMetadata(TimeSpan.FromMilliseconds(150)));

    public TimeSpan FlipDuration
    {
        get => (TimeSpan)GetValue(FlipDurationProperty);
        set => SetValue(FlipDurationProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<FlipEventArgs>? FlipStarted;
    public event EventHandler<FlipEventArgs>? FlipCompleted;

    #endregion

    private void OnCharacterChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(newValue))
        {
            newValue = " ";
        }

        var newChar = newValue.Length > 0 ? newValue[0].ToString().ToUpperInvariant() : " ";

        if (newChar == _displayChar)
        {
            return;
        }

        if (_isFlipping)
        {
            _pendingFlip = true;
            _pendingChar = newChar;
            return;
        }

        StartFlipAnimation(_displayChar, newChar);
    }

    private async void StartFlipAnimation(string fromChar, string toChar)
    {
        _isFlipping = true;
        _prevChar = fromChar;
        _displayChar = toChar;

        FlipStarted?.Invoke(this, new FlipEventArgs(fromChar, toChar));

        // Set up the static base to show the NEW character
        StaticTopChar.Text = toChar;
        StaticBottomChar.Text = toChar;

        // Set up animated flaps
        AnimatedTopChar.Text = fromChar;
        AnimatedBottomChar.Text = toChar;

        // Reset transforms (using ScaleY instead of RotationX)
        TopFlapScale.ScaleY = 1;
        BottomFlapScale.ScaleY = 0;

        // Show animated flaps
        AnimatedTopFlap.Visibility = Visibility.Visible;
        AnimatedBottomFlap.Visibility = Visibility.Visible;

        // Update animation durations
        FlipTopAnimation.Duration = new Duration(FlipDuration);
        FlipBottomAnimation.Duration = new Duration(FlipDuration);

        // Start top flap animation (falling down)
        FlipTopStoryboard.Begin();

        // Wait for top flap to complete
        await Task.Delay(FlipDuration);

        // Start bottom flap animation (falling into place)
        FlipBottomStoryboard.Begin();

        // Wait for bottom flap to complete
        await Task.Delay(FlipDuration);

        // Hide animated flaps
        AnimatedTopFlap.Visibility = Visibility.Collapsed;
        AnimatedBottomFlap.Visibility = Visibility.Collapsed;

        _isFlipping = false;

        FlipCompleted?.Invoke(this, new FlipEventArgs(fromChar, toChar));

        // Handle any pending flip
        if (_pendingFlip)
        {
            _pendingFlip = false;
            var pendingChar = _pendingChar;
            _pendingChar = string.Empty;

            if (pendingChar != _displayChar)
            {
                StartFlipAnimation(_displayChar, pendingChar);
            }
        }
    }

    private void UpdateCharacterDisplay(string character)
    {
        StaticTopChar.Text = character;
        StaticBottomChar.Text = character;
    }

    private void ApplyTheme()
    {
        var colors = GetThemeColors(Theme);

        // Frame gradient
        FrameGradientTop.Color = colors.FrameBackground;
        FrameGradientBottom.Color = colors.FrameBackgroundDark;

        // Card gradients
        CardGradientTop.Color = colors.CardTop;
        CardGradientMid.Color = BlendColors(colors.CardTop, colors.CardBottom, 0.5);
        CardGradientMid2.Color = BlendColors(colors.CardTop, colors.CardBottom, 0.5);
        CardGradientBottom.Color = colors.CardBottom;

        // Text
        TextBrush.Color = colors.TextColor;

        // Divider
        DividerBrush.Color = colors.DividerColor;

        // Rivets
        RivetBrush.Color = colors.RivetColor;
    }

    private void ApplySize()
    {
        var (width, height, fontSize, gap, rivetSize, padding) = GetSizeDimensions(Size);

        OuterFrame.Width = width + (padding * 2) + 16;
        OuterFrame.Height = height + (padding * 2) + 16;
        OuterFrame.Padding = new Thickness(padding);

        CardContainer.Margin = new Thickness(8);
        CenterDivider.Height = gap;

        StaticTopChar.FontSize = fontSize;
        StaticBottomChar.FontSize = fontSize;
        AnimatedTopChar.FontSize = fontSize;
        AnimatedBottomChar.FontSize = fontSize;

        // Adjust character positioning based on font size
        var offset = fontSize * 0.5;
        StaticTopChar.Margin = new Thickness(0, 0, 0, -offset);
        StaticBottomChar.Margin = new Thickness(0, -offset, 0, 0);
        AnimatedTopChar.Margin = new Thickness(0, 0, 0, -offset);
        AnimatedBottomChar.Margin = new Thickness(0, -offset, 0, 0);

        // Rivet sizes
        RivetTopLeft.Width = rivetSize;
        RivetTopLeft.Height = rivetSize;
    }

    private static ThemeColors GetThemeColors(SplitFlapTheme theme)
    {
        return theme switch
        {
            SplitFlapTheme.Light => new ThemeColors
            {
                FrameBackground = ColorFromHex("#d6d3d1"),
                FrameBackgroundDark = ColorFromHex("#a8a29e"),
                CardTop = ColorFromHex("#f5f5f4"),
                CardBottom = ColorFromHex("#e7e5e4"),
                CardBorder = ColorFromHex("#d6d3d1"),
                TextColor = ColorFromHex("#1c1917"),
                DividerColor = ColorFromHex("#a8a29e"),
                RivetColor = ColorFromHex("#a8a29e"),
            },
            SplitFlapTheme.Vintage => new ThemeColors
            {
                FrameBackground = ColorFromHex("#92400e"),
                FrameBackgroundDark = ColorFromHex("#78350f"),
                CardTop = ColorFromHex("#fef3c7"),
                CardBottom = ColorFromHex("#fde68a"),
                CardBorder = ColorFromHex("#fcd34d"),
                TextColor = ColorFromHex("#451a03"),
                DividerColor = ColorFromHex("#fbbf24"),
                RivetColor = ColorFromHex("#b45309"),
            },
            _ => new ThemeColors // Dark theme
            {
                FrameBackground = ColorFromHex("#09090b"),
                FrameBackgroundDark = ColorFromHex("#000000"),
                CardTop = ColorFromHex("#27272a"),
                CardBottom = ColorFromHex("#18181b"),
                CardBorder = ColorFromHex("#3f3f46"),
                TextColor = ColorFromHex("#fef3c7"),
                DividerColor = ColorFromHex("#09090b"),
                RivetColor = ColorFromHex("#52525b"),
            }
        };
    }

    private static (double width, double height, double fontSize, double gap, double rivetSize, double padding)
        GetSizeDimensions(SplitFlapSize size)
    {
        return size switch
        {
            SplitFlapSize.Small => (40, 56, 30, 1, 4, 3),
            SplitFlapSize.Large => (96, 128, 72, 3, 8, 6),
            _ => (64, 88, 48, 2, 6, 4) // Medium
        };
    }

    private static Color ColorFromHex(string hex)
    {
        hex = hex.TrimStart('#');

        byte a = 255;
        byte r, g, b;

        if (hex.Length == 8)
        {
            a = Convert.ToByte(hex.Substring(0, 2), 16);
            r = Convert.ToByte(hex.Substring(2, 2), 16);
            g = Convert.ToByte(hex.Substring(4, 2), 16);
            b = Convert.ToByte(hex.Substring(6, 2), 16);
        }
        else
        {
            r = Convert.ToByte(hex.Substring(0, 2), 16);
            g = Convert.ToByte(hex.Substring(2, 2), 16);
            b = Convert.ToByte(hex.Substring(4, 2), 16);
        }

        return Color.FromArgb(a, r, g, b);
    }

    private static Color BlendColors(Color color1, Color color2, double ratio)
    {
        return Color.FromArgb(
            (byte)(color1.A + (color2.A - color1.A) * ratio),
            (byte)(color1.R + (color2.R - color1.R) * ratio),
            (byte)(color1.G + (color2.G - color1.G) * ratio),
            (byte)(color1.B + (color2.B - color1.B) * ratio)
        );
    }

    private class ThemeColors
    {
        public Color FrameBackground { get; set; }
        public Color FrameBackgroundDark { get; set; }
        public Color CardTop { get; set; }
        public Color CardBottom { get; set; }
        public Color CardBorder { get; set; }
        public Color TextColor { get; set; }
        public Color DividerColor { get; set; }
        public Color RivetColor { get; set; }
    }
}

public class FlipEventArgs : EventArgs
{
    public string FromCharacter { get; }
    public string ToCharacter { get; }

    public FlipEventArgs(string fromCharacter, string toCharacter)
    {
        FromCharacter = fromCharacter;
        ToCharacter = toCharacter;
    }
}
