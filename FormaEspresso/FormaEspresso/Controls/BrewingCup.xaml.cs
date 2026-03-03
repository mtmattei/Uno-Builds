namespace FormaEspresso.Controls;

public sealed partial class BrewingCup : UserControl
{
    private const double MaxCoffeeHeight = 72;

    public BrewingCup()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var storyboard = Resources["SteamAnimation"] as Storyboard;
        storyboard?.Begin();
    }

    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(nameof(Progress), typeof(double), typeof(BrewingCup),
            new PropertyMetadata(0.0));

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public static double GetCoffeeHeight(double progress)
    {
        return (progress / 100.0) * MaxCoffeeHeight;
    }

    public static double GetCremaOffset(double progress)
    {
        return GetCoffeeHeight(progress) - 6;
    }

    public static double GetCremaOpacity(double progress)
    {
        return progress > 10 ? 0.9 : 0;
    }

    public static double GetSteamOpacity(double progress)
    {
        return progress > 50 ? 1 : 0;
    }
}
