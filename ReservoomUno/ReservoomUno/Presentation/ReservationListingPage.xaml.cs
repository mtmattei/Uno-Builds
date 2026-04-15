using ReservoomUno.ViewModels;

namespace ReservoomUno.Presentation;

public sealed partial class ReservationListingPage : Page
{
    public ReservationListingPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ReservationListingViewModel vm)
        {
            vm.LoadReservationsCommand.Execute(null);
        }
    }
}
