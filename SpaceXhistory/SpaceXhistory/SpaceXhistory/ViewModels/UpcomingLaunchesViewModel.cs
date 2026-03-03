using System.Collections.ObjectModel;
using System.Net.Http.Json;
using SpaceXhistory.Helpers;
using SpaceXhistory.Models;

namespace SpaceXhistory.ViewModels;

public class UpcomingLaunchesViewModel : BaseViewModel
{
    private readonly HttpClient _client;

    private ObservableCollection<Root> _nextLaunches;
    public ObservableCollection<Root> NextLaunches
    {
        get => _nextLaunches;
        set => SetProperty(ref _nextLaunches, value);
    }

    public UpcomingLaunchesViewModel(HttpClient client)
    {
        _client = client;
        _nextLaunches = new ObservableCollection<Root>();
    }

    public async Task PopulateNextLaunchesAsync()
    {
        try
        {
            var launches = await RetrieveAllNextLaunchesAsync();
            NextLaunches.Clear();
            foreach (var launch in launches)
            {
                NextLaunches.Add(launch);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error populating launches: {ex.Message}");
        }
    }

    private async Task<List<Root>> RetrieveAllNextLaunchesAsync()
    {
        var launches = await _client.GetFromJsonAsync<List<Root>>("launches/upcoming");
        return launches ?? new List<Root>();
    }
}
