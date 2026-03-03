using System.Collections.ObjectModel;
using System.Net.Http.Json;
using SpaceXhistory.Helpers;
using SpaceXhistory.Models;

namespace SpaceXhistory.ViewModels;

public class PastLaunchesViewModel : BaseViewModel
{
    private readonly HttpClient _client;

    private ObservableCollection<Root> _latestLaunches;
    public ObservableCollection<Root> LatestLaunches
    {
        get => _latestLaunches;
        set => SetProperty(ref _latestLaunches, value);
    }

    public PastLaunchesViewModel(HttpClient client)
    {
        _client = client;
        _latestLaunches = new ObservableCollection<Root>();
    }

    public async Task PopulateLatestLaunchesAsync()
    {
        try
        {
            var launches = await RetrieveAllLatestLaunchesAsync();
            LatestLaunches.Clear();
            foreach (var launch in launches)
            {
                LatestLaunches.Add(launch);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error populating launches: {ex.Message}");
        }
    }

    private async Task<List<Root>> RetrieveAllLatestLaunchesAsync()
    {
        var launches = await _client.GetFromJsonAsync<List<Root>>("launches/past");
        return launches ?? new List<Root>();
    }
}
