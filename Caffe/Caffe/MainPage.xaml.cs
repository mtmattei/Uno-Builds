using Caffe.Models;
using Caffe.ViewModels;

namespace Caffe;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; } = new();

    public MainPage()
    {
        this.InitializeComponent();
        this.DataContext = ViewModel;

        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ViewModel.SelectedEspresso))
        {
            UpdateCardSelections();
        }
    }

    private void OnEspressoCardTapped(object sender, TappedRoutedEventArgs e) => SelectCard(0);
    private void OnDoppioCardTapped(object sender, TappedRoutedEventArgs e) => SelectCard(1);
    private void OnRistrettoCardTapped(object sender, TappedRoutedEventArgs e) => SelectCard(2);
    private void OnLungoCardTapped(object sender, TappedRoutedEventArgs e) => SelectCard(3);

    private void SelectCard(int index)
    {
        ViewModel.SelectedEspresso = ViewModel.EspressoItems[index];
    }

    private void UpdateCardSelections()
    {
        var selected = ViewModel.SelectedEspresso;
        EspressoCard.IsSelected = selected == ViewModel.EspressoItems[0];
        DoppioCard.IsSelected = selected == ViewModel.EspressoItems[1];
        RistrettoCard.IsSelected = selected == ViewModel.EspressoItems[2];
        LungoCard.IsSelected = selected == ViewModel.EspressoItems[3];
    }

    private async void OnBrewRequested(object sender, EventArgs e)
    {
        if (ViewModel.BrewCommand.CanExecute(null))
        {
            await ViewModel.BrewCommand.ExecuteAsync(null);
        }
    }
}
