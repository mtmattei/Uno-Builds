# Building Micro-Interactions in Uno Platform

You know those satisfying little animations when you click a button? Let's build one - ripple effects, particle bursts, the works. Using Uno Platform + WinUI 3.

## What We're Building

Install button that:
- Scales on hover/press
- Shows ripple waves on click (3 staggered, expanding outward)
- Bursts 8 particles in all directions
- Animates heart icon with elastic bounce
- Transitions to "Installed" state with gradient background

## 1. Setup Models

First, define your states. Keep it simple.

**Models/InstallState.cs**
```csharp
public enum InstallState { NotInstalled, Installing, Installed }

public class InstallStateChangedEventArgs : EventArgs
{
    public InstallState NewState { get; set; }
    public InstallState OldState { get; set; }
    public bool IsInstalled => NewState == InstallState.Installed;
}
```

**Models/PluginInfo.cs**
```csharp
public class PluginInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int ActiveInstallations { get; set; }
    public bool IsInstalled { get; set; }
}
```

## 2. Build the Button XAML

Key pieces:
- VisualStateManager for state transitions
- Canvas elements for dynamic effects (ripples/particles)
- Transparent overlay button for hit testing

**Controls/InstallButton.xaml**
```xml
<UserControl x:Class="Unoblueprint.Controls.InstallButton" ...>
    <UserControl.Resources>
        <LinearGradientBrush x:Key="InstalledGradientBrush" StartPoint="0,0" EndPoint="1,1">
            <GradientStop Color="#C4FF0D" Offset="0" />
            <GradientStop Color="#A0D80B" Offset="1" />
        </LinearGradientBrush>
    </UserControl.Resources>

    <Grid Width="110" Height="40">
        <VisualStateManager.VisualStateGroups>
            <!-- Hover/Press states -->
            <VisualStateGroup x:Name="CommonStates">
                <VisualState x:Name="PointerOver">
                    <Storyboard>
                        <DoubleAnimation Storyboard.TargetName="ButtonScale"
                                       Storyboard.TargetProperty="ScaleX"
                                       To="1.03" Duration="0:0:0.15">
                            <DoubleAnimation.EasingFunction>
                                <CubicEase EasingMode="EaseOut" />
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                        <!-- ScaleY same as ScaleX -->
                    </Storyboard>
                </VisualState>
                <VisualState x:Name="Pressed">
                    <!-- Scale to 0.97 -->
                </VisualState>
            </VisualStateGroup>

            <!-- Install states -->
            <VisualStateGroup x:Name="InstallStates">
                <VisualState x:Name="Installed">
                    <VisualState.Setters>
                        <Setter Target="ButtonBorder.Background" Value="{StaticResource InstalledGradientBrush}" />
                        <Setter Target="HeartIcon.Visibility" Value="Visible" />
                    </VisualState.Setters>
                    <Storyboard>
                        <!-- Heart animation: 0 ’ 1.3 ’ 1.0 with ElasticEase -->
                        <DoubleAnimationUsingKeyFrames Storyboard.TargetName="HeartTransform"
                                                       Storyboard.TargetProperty="ScaleX">
                            <DiscreteDoubleKeyFrame KeyTime="0:0:0" Value="0" />
                            <EasingDoubleKeyFrame KeyTime="0:0:0.2" Value="1.3">
                                <EasingDoubleKeyFrame.EasingFunction>
                                    <BackEase EasingMode="EaseOut" Amplitude="0.5" />
                                </EasingDoubleKeyFrame.EasingFunction>
                            </EasingDoubleKeyFrame>
                            <EasingDoubleKeyFrame KeyTime="0:0:0.4" Value="1.0">
                                <EasingDoubleKeyFrame.EasingFunction>
                                    <ElasticEase Oscillations="2" Springiness="4" />
                                </EasingDoubleKeyFrame.EasingFunction>
                            </EasingDoubleKeyFrame>
                        </DoubleAnimationUsingKeyFrames>
                        <!-- ScaleY same -->
                    </Storyboard>
                </VisualState>
            </VisualStateGroup>
        </VisualStateManager.VisualStateGroups>

        <Border x:Name="ButtonBorder" Background="White" BorderThickness="1.5" CornerRadius="8">
            <Border.RenderTransform>
                <ScaleTransform x:Name="ButtonScale" />
            </Border.RenderTransform>

            <Grid Padding="12,0">
                <FontIcon x:Name="HeartIcon" Glyph="&#xE00B;" Visibility="Collapsed">
                    <FontIcon.RenderTransform>
                        <CompositeTransform x:Name="HeartTransform" />
                    </FontIcon.RenderTransform>
                </FontIcon>
                <TextBlock x:Name="ButtonText" Text="Install now" />
            </Grid>
        </Border>

        <!-- Canvas for effects - MUST have explicit dimensions -->
        <Canvas x:Name="RippleCanvas" IsHitTestVisible="False" Width="200" Height="100"/>
        <Canvas x:Name="ParticleCanvas" IsHitTestVisible="False" Width="200" Height="100"/>

        <!-- Transparent button for events -->
        <Button x:Name="InteractionButton" Background="Transparent" BorderThickness="0"
                Click="OnButtonClick" PointerEntered="OnPointerEntered"
                PointerExited="OnPointerExited" PointerPressed="OnPointerPressed"
                PointerReleased="OnPointerReleased" />
    </Grid>
</UserControl>
```

## 3. Code-Behind: The Fun Part

**Controls/InstallButton.xaml.cs**

### Setup dependency property and state

```csharp
public sealed partial class InstallButton : UserControl
{
    public static readonly DependencyProperty IsInstalledProperty =
        DependencyProperty.Register(nameof(IsInstalled), typeof(bool), typeof(InstallButton),
            new PropertyMetadata(false, OnIsInstalledChanged));

    public bool IsInstalled
    {
        get => (bool)GetValue(IsInstalledProperty);
        set => SetValue(IsInstalledProperty, value);
    }

    public event EventHandler<InstallStateChangedEventArgs>? InstallStateChanged;
    private InstallState _currentState = InstallState.NotInstalled;

    public InstallButton()
    {
        this.InitializeComponent();
        this.Loaded += (s, e) => UpdateVisualState(false);
    }

    private static void OnIsInstalledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InstallButton button) button.UpdateVisualState(true);
    }

    private void UpdateVisualState(bool useTransitions)
    {
        var newState = IsInstalled ? InstallState.Installed : InstallState.NotInstalled;
        VisualStateManager.GoToState(this, newState.ToString(), useTransitions);

        if (_currentState != newState)
        {
            InstallStateChanged?.Invoke(this, new InstallStateChangedEventArgs
            {
                NewState = newState,
                OldState = _currentState
            });
            _currentState = newState;
        }
    }
```

### Click handler - triggers everything

```csharp
    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        // Always run effects (so you can click multiple times)
        CreateRippleEffect();
        CreateParticleBurst();

        // Toggle state
        IsInstalled = !IsInstalled;
    }
```

### Pointer handlers for hover/press

```csharp
    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        => VisualStateManager.GoToState(this, "PointerOver", true);

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        => VisualStateManager.GoToState(this, "Normal", true);

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        => VisualStateManager.GoToState(this, "Pressed", true);

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        => VisualStateManager.GoToState(this, "Normal", true);
```

### Ripple effect - the money shot

Create 3 staggered ripples, each expanding from button size to 2.5x, fading out.

```csharp
    private void CreateRippleEffect()
    {
        for (int i = 0; i < 3; i++)
        {
            var ripple = new Ellipse
            {
                Width = 110,
                Height = 40,
                Fill = new SolidColorBrush(Color.FromArgb(102, 196, 255, 13)), // 40% opacity brand color
                RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5),
                RenderTransform = new ScaleTransform()
            };

            // Center it on canvas
            Canvas.SetLeft(ripple, (RippleCanvas.Width - 110) / 2);
            Canvas.SetTop(ripple, (RippleCanvas.Height - 40) / 2);
            RippleCanvas.Children.Add(ripple);

            var storyboard = new Storyboard();
            var delay = i * 100; // Stagger by 100ms

            // Scale X
            var scaleXAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 2.5,
                Duration = TimeSpan.FromMilliseconds(800),
                BeginTime = TimeSpan.FromMilliseconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleXAnimation, ripple);
            Storyboard.SetTargetProperty(scaleXAnimation, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");

            // Scale Y (same as X)
            var scaleYAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 2.5,
                Duration = TimeSpan.FromMilliseconds(800),
                BeginTime = TimeSpan.FromMilliseconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleYAnimation, ripple);
            Storyboard.SetTargetProperty(scaleYAnimation, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");

            // Fade out
            var opacityAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(800),
                BeginTime = TimeSpan.FromMilliseconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(opacityAnimation, ripple);
            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);
            storyboard.Children.Add(opacityAnimation);

            // Cleanup when done
            storyboard.Completed += (s, e) => RippleCanvas.Children.Remove(ripple);
            storyboard.Begin();
        }
    }
```

### Particle burst

8 particles, evenly distributed (45° apart), shoot out 50px then fade.

```csharp
    private void CreateParticleBurst()
    {
        var colors = new[]
        {
            Color.FromArgb(255, 196, 255, 13),  // Brand yellow-green
            Color.FromArgb(255, 255, 71, 87),   // Heart red
            Color.FromArgb(255, 176, 232, 12)   // Alt green
        };

        var random = new Random();
        var centerX = ParticleCanvas.Width / 2;
        var centerY = ParticleCanvas.Height / 2;

        for (int i = 0; i < 8; i++)
        {
            var angle = (i * 45) * Math.PI / 180; // 360° / 8 = 45°
            var distance = 50;

            var particle = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(colors[random.Next(colors.Length)]),
                RenderTransform = new CompositeTransform()
            };

            Canvas.SetLeft(particle, centerX - 3);
            Canvas.SetTop(particle, centerY - 3);
            ParticleCanvas.Children.Add(particle);

            var storyboard = new Storyboard();

            // Translate X
            var translateX = new DoubleAnimation
            {
                From = 0,
                To = Math.Cos(angle) * distance,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(translateX, particle);
            Storyboard.SetTargetProperty(translateX, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");

            // Translate Y
            var translateY = new DoubleAnimation
            {
                From = 0,
                To = Math.Sin(angle) * distance,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(translateY, particle);
            Storyboard.SetTargetProperty(translateY, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");

            // Fade out (delayed start)
            var opacity = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                BeginTime = TimeSpan.FromMilliseconds(200),
                Duration = TimeSpan.FromMilliseconds(300)
            };
            Storyboard.SetTarget(opacity, particle);
            Storyboard.SetTargetProperty(opacity, "Opacity");

            storyboard.Children.Add(translateX);
            storyboard.Children.Add(translateY);
            storyboard.Children.Add(opacity);

            storyboard.Completed += (s, e) => ParticleCanvas.Children.Remove(particle);
            storyboard.Begin();
        }
    }
}
```

## 4. Card Container (Optional)

Wrap your button in a card with plugin info.

**Controls/PluginCard.xaml**
```xml
<Border Width="350" Height="420" CornerRadius="16" BorderThickness="1">
    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" /> <!-- Illustration -->
            <RowDefinition Height="Auto" /> <!-- Name + Badge -->
            <RowDefinition Height="Auto" /> <!-- Description -->
            <RowDefinition Height="Auto" /> <!-- Divider -->
            <RowDefinition Height="Auto" /> <!-- Button -->
            <RowDefinition Height="Auto" /> <!-- Stats -->
        </Grid.RowDefinitions>

        <!-- Illustration area with logo -->
        <Grid Grid.Row="0" Height="140">
            <Border Background="#F5F5F5" CornerRadius="12" />
            <Border Background="#C4FF0D" Width="64" Height="64"
                    CornerRadius="16" HorizontalAlignment="Left" VerticalAlignment="Bottom">
                <TextBlock x:Name="LogoText" Text="W" FontSize="32" FontWeight="Bold" />
            </Border>
        </Grid>

        <!-- Name + Badge inline -->
        <Grid Grid.Row="1">
            <TextBlock x:Name="PluginNameText" Text="Wordsometric" FontSize="24" FontWeight="Bold" />
            <Border Background="#F5F5F5" CornerRadius="12" Padding="10,4">
                <TextBlock x:Name="CategoryText" Text="Third-party payment" FontSize="11" />
            </Border>
        </Grid>

        <!-- Description -->
        <TextBlock Grid.Row="2" x:Name="DescriptionText" TextWrapping="Wrap" />

        <!-- Divider -->
        <Border Grid.Row="3" Height="1" Background="#E5E5E5" />

        <!-- Button -->
        <local:InstallButton Grid.Row="4" x:Name="InstallBtn" />

        <!-- Stats -->
        <StackPanel Grid.Row="5" Orientation="Horizontal">
            <FontIcon Glyph="&#xE8A7;" FontSize="14" />
            <TextBlock x:Name="InstallationsText" Text="300,00 active installations" />
        </StackPanel>
    </Grid>
</Border>
```

**Controls/PluginCard.xaml.cs**
```csharp
public sealed partial class PluginCard : UserControl
{
    public static readonly DependencyProperty PluginProperty =
        DependencyProperty.Register(nameof(Plugin), typeof(PluginInfo), typeof(PluginCard),
            new PropertyMetadata(null, OnPluginChanged));

    public PluginInfo? Plugin
    {
        get => (PluginInfo?)GetValue(PluginProperty);
        set => SetValue(PluginProperty, value);
    }

    private static void OnPluginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PluginCard card && e.NewValue is PluginInfo plugin)
        {
            card.CategoryText.Text = plugin.Category;
            card.PluginNameText.Text = plugin.Name;
            card.DescriptionText.Text = plugin.Description;
            card.InstallBtn.IsInstalled = plugin.IsInstalled;
            card.LogoText.Text = plugin.Name[0].ToString();

            // Format count (300000 ’ "300,00")
            if (plugin.ActiveInstallations >= 1000)
            {
                var thousands = plugin.ActiveInstallations / 1000.0;
                card.InstallationsText.Text = $"{thousands:0.00} active installations".Replace(".", ",");
            }
        }
    }
}
```

## Key Takeaways

**Canvas positioning matters**
- Must set explicit Width/Height on Canvas elements
- Without dimensions, particles/ripples won't show
- Use `Canvas.SetLeft()` and `Canvas.SetTop()` to position children

**Storyboard targeting syntax**
- Format: `"(UIElement.RenderTransform).(ScaleTransform.ScaleX)"`
- Must match the type of RenderTransform you're using
- CompositeTransform if you need multiple transforms

**Multiple effects = stagger them**
- Delay each ripple by 100-150ms
- Makes the effect feel richer, not just "louder"

**VisualStateManager placement**
- Goes on first child of UserControl's content
- Not on UserControl itself

**Easing functions = personality**
- `ElasticEase` for bouncy/playful (heart icon)
- `BackEase` for slight overshoot
- `CubicEase` for smooth, natural motion (ripples)

**Always cleanup**
- Remove Canvas children in `Storyboard.Completed` handler
- Otherwise you're leaking DOM nodes on every click

**Testing tip**
- Make click toggle the state (not one-way)
- Lets you spam-click to see the effect in rapid succession
- Real users will only install once, but you need to test it 50 times

## Common Pitfalls

1. **Canvas has no size** ’ effects invisible
2. **Forgot `IsHitTestVisible="False"` on Canvas** ’ blocks clicks
3. **Opacity animation but Fill has alpha=255** ’ looks bad
4. **Too many particles** ’ looks chaotic, not satisfying
5. **All ripples start at once** ’ looks like 1 big ripple
6. **Forgot to remove elements** ’ memory leak + perf hit

---

Built this for a plugin marketplace UI. Same principles work for like buttons, purchase flows, notification toasts - anywhere you want that extra polish.

The math is simple (some trig for particles), the payoff is huge. Users notice this stuff even if they can't articulate why your UI "feels better."
