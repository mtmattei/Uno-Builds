using FieldCheck.ViewModels;
using Microsoft.UI.Xaml.Navigation;

namespace FieldCheck.Views;

public sealed partial class NewInspectionPage : Page
{
    public NewInspectionPage()
    {
        ViewModel = App.Services.GetRequiredService<NewInspectionViewModel>();
        InitializeComponent();
    }

    public NewInspectionViewModel ViewModel { get; }

    protected override async void OnNavigatedTo(NavigationEventArgs e) => await ViewModel.LoadAsync((string)e.Parameter);
}
