using CommunityToolkit.Mvvm.ComponentModel;
using Sanctum.Models;
using Sanctum.Services;
using System.Collections.ObjectModel;

namespace Sanctum.ViewModels;

public partial class DailyDigestViewModel : ObservableObject
{
    private readonly IMockDataService _mockData;

    public DailyDigestViewModel(IMockDataService mockData)
    {
        _mockData = mockData;
        LoadDigest();
    }

    [ObservableProperty]
    private string _aiSummary = "You have one upcoming meeting and several non-urgent updates. Your morning is clear for focused work.";

    public ObservableCollection<DigestItem> MustSeeItems { get; } = [];
    public ObservableCollection<DigestItem> NiceToKnowItems { get; } = [];

    private void LoadDigest()
    {
        var items = _mockData.GenerateDailyDigest();

        foreach (var item in items.Where(i => i.IsUrgent))
        {
            MustSeeItems.Add(item);
        }

        foreach (var item in items.Where(i => !i.IsUrgent))
        {
            NiceToKnowItems.Add(item);
        }
    }
}
