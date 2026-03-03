using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.System;
using Worn.Models;

namespace Worn.Controls;

public sealed partial class DayCard : UserControl
{
    private bool _isFlipped;
    private Storyboard? _hoverSb;
    private Storyboard? _flipSb;

    public static readonly DependencyProperty ForecastProperty =
        DependencyProperty.Register(
            nameof(Forecast),
            typeof(DayForecast),
            typeof(DayCard),
            new PropertyMetadata(null, OnForecastChanged));

    public DayForecast? Forecast
    {
        get => (DayForecast?)GetValue(ForecastProperty);
        set => SetValue(ForecastProperty, value);
    }

    public DayCard()
    {
        this.InitializeComponent();
        IsTabStop = true;
        KeyDown += OnCardKeyDown;
    }

    private static void OnForecastChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DayCard card && e.NewValue is DayForecast forecast)
        {
            card.ApplyForecast(forecast);
        }
    }

    private static Brush GetBrush(string key) =>
        (Brush)Application.Current.Resources[key];

    private void ApplyForecast(DayForecast f)
    {
        DayAbbrevText.Text = f.DayAbbrev;
        EmojiText.Text = f.Emoji;
        HeadlineText.Text = f.Headline;
        FabricsText.Text = string.Join(" / ", f.Fabrics);
        BackLabelText.Text = f.BackLabel;
        TipText.Text = f.Tip;
        BackFabricText.Text = f.BackFabricDetail;

        AutomationProperties.SetName(CardRoot,
            $"{f.DayAbbrev}: {f.Headline}. {string.Join(", ", f.Fabrics)}");

        SwatchBar.Background = GetBrush(
            f.IsToday ? "WornTerracottaBrush" : "WornSlateBrush");

        if (f.IsToday)
        {
            FrontFace.Background = GetBrush("WornWarmBlackBrush");
            DayAbbrevText.Foreground = GetBrush("WornGoldBrush");
            EmojiText.Foreground = GetBrush("WornCreamBrush");
            HeadlineText.Foreground = GetBrush("WornCreamBrush");
            FabricsText.Foreground = GetBrush("WornSlateBrush");

            BackFace.Background = GetBrush("WornTerracottaBrush");
            BackLabelText.Foreground = GetBrush("WornCreamBrush");
            TipText.Foreground = GetBrush("WornCreamBrush");
            BackFabricText.Foreground = GetBrush("WornBlushBrush");
        }
        else
        {
            FrontFace.Background = GetBrush("WornLinenBrush");
            DayAbbrevText.Foreground = GetBrush("WornSlateBrush");
            HeadlineText.Foreground = GetBrush("WornWarmBlackBrush");
            FabricsText.Foreground = GetBrush("WornSlateBrush");

            BackFace.Background = GetBrush("WornCharcoalBrush");
            BackLabelText.Foreground = GetBrush("WornGoldBrush");
            TipText.Foreground = GetBrush("WornCreamBrush");
            BackFabricText.Foreground = GetBrush("WornBlushBrush");
        }
    }

    private void OnCardTapped(object sender, TappedRoutedEventArgs e)
    {
        _isFlipped = !_isFlipped;
        AnimateFlip(_isFlipped);
    }

    private void OnCardKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is VirtualKey.Enter or VirtualKey.Space)
        {
            _isFlipped = !_isFlipped;
            AnimateFlip(_isFlipped);
            e.Handled = true;
        }
    }

    private void OnCardPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        AnimateHoverLift(-4);
    }

    private void OnCardPointerExited(object sender, PointerRoutedEventArgs e)
    {
        AnimateHoverLift(0);
    }

    private void AnimateHoverLift(double toY)
    {
        _hoverSb?.Stop();

        var anim = new DoubleAnimation
        {
            To = toY,
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        _hoverSb = new Storyboard();
        Storyboard.SetTarget(anim, CardLift);
        Storyboard.SetTargetProperty(anim, "Y");
        _hoverSb.Children.Add(anim);

        _hoverSb.Completed += (_, _) => _hoverSb?.Stop();
        _hoverSb.Begin();
    }

    private void AnimateFlip(bool toBack)
    {
        _flipSb?.Stop();

        var halfDuration = TimeSpan.FromMilliseconds(120);
        var fullDuration = TimeSpan.FromMilliseconds(240);

        var storyboard = new Storyboard();
        _flipSb = storyboard;

        var shrinkAnim = new DoubleAnimationUsingKeyFrames();
        shrinkAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 1 });
        shrinkAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(halfDuration), Value = 0 });
        shrinkAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(fullDuration), Value = 0 });
        Storyboard.SetTarget(shrinkAnim, toBack ? FrontScale : BackScale);
        Storyboard.SetTargetProperty(shrinkAnim, "ScaleX");
        storyboard.Children.Add(shrinkAnim);

        var growAnim = new DoubleAnimationUsingKeyFrames();
        growAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 0 });
        growAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(halfDuration), Value = 0 });
        growAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(fullDuration), Value = 1 });
        Storyboard.SetTarget(growAnim, toBack ? BackScale : FrontScale);
        Storyboard.SetTargetProperty(growAnim, "ScaleX");
        storyboard.Children.Add(growAnim);

        var hideAnim = new DoubleAnimationUsingKeyFrames();
        hideAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 1 });
        hideAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(halfDuration), Value = 0 });
        Storyboard.SetTarget(hideAnim, toBack ? FrontFace : BackFace);
        Storyboard.SetTargetProperty(hideAnim, "Opacity");
        storyboard.Children.Add(hideAnim);

        var showAnim = new DoubleAnimationUsingKeyFrames();
        showAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 0 });
        showAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(halfDuration), Value = 1 });
        Storyboard.SetTarget(showAnim, toBack ? BackFace : FrontFace);
        Storyboard.SetTargetProperty(showAnim, "Opacity");
        storyboard.Children.Add(showAnim);

        FrontFace.IsHitTestVisible = !toBack;
        BackFace.IsHitTestVisible = toBack;

        storyboard.Begin();
    }
}
