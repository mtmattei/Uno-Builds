using FieldCheck.ViewModels;
using Microsoft.UI.Xaml.Navigation;

namespace FieldCheck.Views;

public sealed partial class InspectionSuccessPage : Page
{
    public InspectionSuccessPage()
    {
        ViewModel = App.Services.GetRequiredService<InspectionSuccessViewModel>();
        InitializeComponent();
    }

    public InspectionSuccessViewModel ViewModel { get; }

    protected override async void OnNavigatedTo(NavigationEventArgs e) => await ViewModel.LoadAsync((string)e.Parameter);
}
