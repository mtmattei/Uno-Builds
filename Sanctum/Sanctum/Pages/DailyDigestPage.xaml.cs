using Sanctum.Services;
using Sanctum.ViewModels;

namespace Sanctum.Pages;

public sealed partial class DailyDigestPage : UserControl
{
    public DailyDigestViewModel ViewModel { get; }

    public DailyDigestPage()
    {
        var mockData = App.Services!.GetRequiredService<IMockDataService>();
        ViewModel = new DailyDigestViewModel(mockData);

        this.InitializeComponent();
    }

    public Visibility HasMustSeeItems => ViewModel.MustSeeItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
}
