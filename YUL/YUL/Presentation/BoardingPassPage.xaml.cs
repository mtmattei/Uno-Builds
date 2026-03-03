using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Storage.Streams;
using ZXing.SkiaSharp;

namespace YUL.Presentation;

public sealed partial class BoardingPassPage : Page
{
    private const double CollapsedHeight = 80;
    private const double ExpandThreshold = 100;
    private const double DismissThreshold = 150;
    private double _initialY;
    private double _currentY;
    private bool _isExpanded = false;
    private bool _isAnimating = false;

    public BoardingPassPage()
    {
        this.InitializeComponent();
        this.DataContextChanged += OnDataContextChanged;
    }

    private BoardingPass? _boardingPass;

    private async void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (DataContext is BoardingPassViewModel viewModel)
        {
            var pass = await viewModel.BoardingPass;
            
            if (pass != null)
            {
                _boardingPass = pass;
                
                // Update collapsed content manually for custom formatting
                CollapsedTitle.Text = $"{pass.FlightNumber} - {pass.Status}";
                CollapsedSubtitle.Text = $"Gate {pass.Gate} • {pass.DepartureTime}";
                
                // Generate Barcode
                GenerateBarcode(pass.QRCodeData);
            }
        }
    }

    private async void GenerateBarcode(string data)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Generating barcode with data: {data}");
            
            var writer = new ZXing.SkiaSharp.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Height = 100,
                    Width = 600,
                    Margin = 0,
                    PureBarcode = true
                }
            };

            using var skBitmap = writer.Write(data);
            using var image = SkiaSharp.SKImage.FromBitmap(skBitmap);
            using var encoded = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            using var stream = new System.IO.MemoryStream();
            encoded.SaveTo(stream);
            stream.Position = 0;

            var bitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
            await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
            
            DispatcherQueue.TryEnqueue(() =>
            {
                if (QRCodeImage != null)
                {
                    QRCodeImage.Source = bitmap;
                    System.Diagnostics.Debug.WriteLine("Barcode image set successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("QRCodeImage is null!");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Barcode generation failed: {ex.Message}\n{ex.StackTrace}");
            DispatcherQueue.TryEnqueue(() =>
            {
                if (QRCodeImage != null)
                {
                    QRCodeImage.Visibility = Visibility.Collapsed;
                }
            });
        }
    }

    private void OnCardTapped(object sender, TappedRoutedEventArgs e)
    {
        if (!_isExpanded)
        {
            ExpandCard();
        }
        else
        {
            CollapseCard();
        }
    }

    private void OnOverlayTapped(object sender, TappedRoutedEventArgs e)
    {
        CollapseCard();
    }

    private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
    {
        if (_isAnimating) return;
        _initialY = CardTransform.Y;
        
        // Show pull indicator when starting to drag down from collapsed state
        if (!_isExpanded && PullIndicator != null)
        {
            PullIndicator.Visibility = Visibility.Visible;
        }
    }

    private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        if (_isAnimating) return;
        
        var deltaY = e.Delta.Translation.Y;
        _currentY = _initialY + e.Cumulative.Translation.Y;

        if (!_isExpanded)
        {
            // Only allow downward drag when collapsed
            if (_currentY > 0)
            {
                CardTransform.Y = _currentY;
                
                // Progressive opacity feedback during drag
                var progress = Math.Min(_currentY / ExpandThreshold, 1.0);
                CollapsedContent.Opacity = 1.0 - (progress * 0.3);
                
                // Animate pull indicator
                if (PullIndicator != null)
                {
                    PullIndicator.Opacity = Math.Min(progress * 1.5, 1.0);
                    var indicatorTransform = PullIndicator.RenderTransform as TranslateTransform;
                    if (indicatorTransform == null)
                    {
                        indicatorTransform = new TranslateTransform();
                        PullIndicator.RenderTransform = indicatorTransform;
                    }
                    indicatorTransform.Y = _currentY * 0.5; // Move half as fast as card
                }
            }
            else
            {
                // Pulled up while collapsed - hide indicator
                if (PullIndicator != null)
                {
                    PullIndicator.Opacity = 0;
                }
            }
        }
        else
        {
            // Allow both directions when expanded
            CardTransform.Y = _currentY;
            
            // Provide visual feedback
            if (_currentY < -50)
            {
                ExpandedContent.Opacity = 1.0 - (Math.Abs(_currentY) / 200.0);
            }
        }
    }

    private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        if (_isAnimating) return;
        
        var totalY = e.Cumulative.Translation.Y;

        if (!_isExpanded)
        {
            // Dragging down from collapsed state
            if (totalY > ExpandThreshold)
            {
                ExpandCard();
            }
            else
            {
                // Snap back to collapsed with bounce
                SnapBack(0);
                
                // Fade out pull indicator
                AnimatePullIndicatorOut();
            }
        }
        else
        {
            // Dragging from expanded state
            if (totalY < -ExpandThreshold)
            {
                // Swipe up to collapse
                CollapseCard();
            }
            else if (totalY > DismissThreshold)
            {
                // Swipe down to dismiss
                DismissCard();
            }
            else
            {
                // Snap back to expanded position
                var expandedY = 0; // Keep at top when expanded
                SnapBack(expandedY);
            }
        }
    }

    private void ExpandCard()
    {
        if (_isAnimating) return;
        _isAnimating = true;
        _isExpanded = true;
        
        Overlay.Visibility = Visibility.Visible;
        ExpandedContent.Visibility = Visibility.Visible;
        ExpandedContent.Opacity = 0;

        // Fade out pull indicator
        AnimatePullIndicatorOut();

        // Fade out collapsed content first
        var collapseContentFade = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(200))
        };
        Storyboard.SetTarget(collapseContentFade, CollapsedContent);
        Storyboard.SetTargetProperty(collapseContentFade, "Opacity");

        // Fade in expanded content
        var expandContentFade = new DoubleAnimation
        {
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            BeginTime = TimeSpan.FromMilliseconds(100)
        };
        Storyboard.SetTarget(expandContentFade, ExpandedContent);
        Storyboard.SetTargetProperty(expandContentFade, "Opacity");

        // Move card to position
        var positionAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EnableDependentAnimation = true
        };
        positionAnimation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
        Storyboard.SetTarget(positionAnimation, CardTransform);
        Storyboard.SetTargetProperty(positionAnimation, "Y");

        // Fade in overlay
        var overlayAnimation = new DoubleAnimation
        {
            To = 0.5,
            Duration = new Duration(TimeSpan.FromMilliseconds(300))
        };
        Storyboard.SetTarget(overlayAnimation, Overlay);
        Storyboard.SetTargetProperty(overlayAnimation, "Opacity");

        var storyboard = new Storyboard();
        storyboard.Children.Add(collapseContentFade);
        storyboard.Children.Add(expandContentFade);
        storyboard.Children.Add(positionAnimation);
        storyboard.Children.Add(overlayAnimation);
        
        storyboard.Completed += (s, e) => 
        { 
            CollapsedContent.Opacity = 1.0;
            _isAnimating = false; 
        };
        storyboard.Begin();
    }

    private void CollapseCard()
    {
        if (_isAnimating) return;
        _isAnimating = true;
        _isExpanded = false;
        
        // Fade out expanded content
        var expandContentFade = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(200))
        };
        Storyboard.SetTarget(expandContentFade, ExpandedContent);
        Storyboard.SetTargetProperty(expandContentFade, "Opacity");

        // Fade in collapsed content
        var collapseContentFade = new DoubleAnimation
        {
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            BeginTime = TimeSpan.FromMilliseconds(100)
        };
        Storyboard.SetTarget(collapseContentFade, CollapsedContent);
        Storyboard.SetTargetProperty(collapseContentFade, "Opacity");

        // Move card to collapsed position
        var positionAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EnableDependentAnimation = true
        };
        positionAnimation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
        Storyboard.SetTarget(positionAnimation, CardTransform);
        Storyboard.SetTargetProperty(positionAnimation, "Y");

        // Fade out overlay
        var overlayAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300))
        };
        Storyboard.SetTarget(overlayAnimation, Overlay);
        Storyboard.SetTargetProperty(overlayAnimation, "Opacity");

        var storyboard = new Storyboard();
        storyboard.Children.Add(expandContentFade);
        storyboard.Children.Add(collapseContentFade);
        storyboard.Children.Add(positionAnimation);
        storyboard.Children.Add(overlayAnimation);
        
        storyboard.Completed += (s, e) =>
        {
            Overlay.Visibility = Visibility.Collapsed;
            ExpandedContent.Visibility = Visibility.Collapsed;
            ExpandedContent.Opacity = 1.0;
            _isAnimating = false;
        };
        storyboard.Begin();
    }

    private void DismissCard()
    {
        if (_isAnimating) return;
        _isAnimating = true;
        
        var dismissStoryboard = new Storyboard();
        var dismissAnimation = new DoubleAnimation
        {
            To = -ActualHeight,
            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
            EnableDependentAnimation = true
        };
        dismissAnimation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };

        Storyboard.SetTarget(dismissAnimation, CardTransform);
        Storyboard.SetTargetProperty(dismissAnimation, "Y");
        dismissStoryboard.Children.Add(dismissAnimation);

        var overlayAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(250))
        };
        Storyboard.SetTarget(overlayAnimation, Overlay);
        Storyboard.SetTargetProperty(overlayAnimation, "Opacity");
        dismissStoryboard.Children.Add(overlayAnimation);

        var cardOpacityAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(250))
        };
        Storyboard.SetTarget(cardOpacityAnimation, CardContainer);
        Storyboard.SetTargetProperty(cardOpacityAnimation, "Opacity");
        dismissStoryboard.Children.Add(cardOpacityAnimation);

        dismissStoryboard.Completed += (s, e) =>
        {
            Overlay.Visibility = Visibility.Collapsed;
            ExpandedContent.Visibility = Visibility.Collapsed;
            CardContainer.Visibility = Visibility.Collapsed;
            _isExpanded = false;
            CardTransform.Y = 0;
            CardContainer.Opacity = 1;
            _isAnimating = false;
        };

        dismissStoryboard.Begin();
    }

    private void SnapBack(double targetY)
    {
        _isAnimating = true;
        
        var animation = new DoubleAnimation
        {
            To = targetY,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EnableDependentAnimation = true
        };
        animation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };

        var storyboard = new Storyboard();
        Storyboard.SetTarget(animation, CardTransform);
        Storyboard.SetTargetProperty(animation, "Y");
        storyboard.Children.Add(animation);
        
        // Reset opacity
        var opacityAnimation = new DoubleAnimation
        {
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(200))
        };
        Storyboard.SetTarget(opacityAnimation, _isExpanded ? ExpandedContent : CollapsedContent);
        Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
        storyboard.Children.Add(opacityAnimation);
        
        storyboard.Completed += (s, e) => { _isAnimating = false; };
        storyboard.Begin();
    }

    private void AnimateToPosition(double targetY)
    {
        var animation = new DoubleAnimation
        {
            To = targetY,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EnableDependentAnimation = true
        };
        animation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

        var storyboard = new Storyboard();
        Storyboard.SetTarget(animation, CardTransform);
        Storyboard.SetTargetProperty(animation, "Y");
        storyboard.Children.Add(animation);
        storyboard.Begin();
    }

    private void AnimatePullIndicatorOut()
    {
        if (PullIndicator == null) return;

        var storyboard = new Storyboard();
        
        // Fade out opacity
        var opacityAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(200))
        };
        Storyboard.SetTarget(opacityAnimation, PullIndicator);
        Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
        storyboard.Children.Add(opacityAnimation);
        
        // Move back to original position
        var indicatorTransform = PullIndicator.RenderTransform as TranslateTransform;
        if (indicatorTransform != null)
        {
            var positionAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                EnableDependentAnimation = true
            };
            positionAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTarget(positionAnimation, indicatorTransform);
            Storyboard.SetTargetProperty(positionAnimation, "Y");
            storyboard.Children.Add(positionAnimation);
        }
        
        storyboard.Completed += (s, e) =>
        {
            PullIndicator.Visibility = Visibility.Collapsed;
        };
        
        storyboard.Begin();
    }
}
