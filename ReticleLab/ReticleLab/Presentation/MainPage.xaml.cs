namespace ReticleLab.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, _) => Bindings.Update();
    }

    // Navigation assigns the view model as DataContext; x:Bind needs it typed.
    public MainViewModel? ViewModel => DataContext as MainViewModel;
}
