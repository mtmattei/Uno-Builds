using Sanctum.Services;
using Sanctum.ViewModels;

namespace Sanctum.Pages;

public sealed partial class SignalFeedPage : UserControl
{
    public SignalFeedViewModel ViewModel { get; }

    public SignalFeedPage()
    {
        var mockData = App.Services!.GetRequiredService<IMockDataService>();
        ViewModel = new SignalFeedViewModel(mockData);

        this.InitializeComponent();
    }
}
