using CommunityToolkit.Mvvm.ComponentModel;
using Sanctum.Models;
using Sanctum.Services;
using System.Collections.ObjectModel;

namespace Sanctum.ViewModels;

public partial class SignalFeedViewModel : ObservableObject
{
    private readonly IMockDataService _mockData;

    public SignalFeedViewModel(IMockDataService mockData)
    {
        _mockData = mockData;
        LoadFeed();
    }

    [ObservableProperty]
    private bool _isAllCaughtUp;

    public ObservableCollection<FeedItem> FeedItems { get; } = [];

    private void LoadFeed()
    {
        foreach (var item in _mockData.GenerateSignalFeed())
        {
            FeedItems.Add(item);
        }

        // Finite feed - mark as caught up when loaded
        IsAllCaughtUp = FeedItems.Count > 0;
    }
}
