using System.Net.Http.Json;
using SpaceXhistory.Helpers;
using SpaceXhistory.Models;

namespace SpaceXhistory.ViewModels;

public class HomePageViewModel : BaseViewModel
{
    private readonly HttpClient _client;

    private Root? _nextLaunch;
    public Root? NextLaunch
    {
        get => _nextLaunch;
        set => SetProperty(ref _nextLaunch, value);
    }

    private Root? _latestLaunch;
    public Root? LatestLaunch
    {
        get => _latestLaunch;
        set => SetProperty(ref _latestLaunch, value);
    }

    private Roadster? _roadsterInfo;
    public Roadster? RoadsterInfo
    {
        get => _roadsterInfo;
        set => SetProperty(ref _roadsterInfo, value);
    }

    public HomePageViewModel(HttpClient client)
    {
        _client = client;
    }

    public async Task GetNextLaunchAsync()
    {
        try
        {
            NextLaunch = await _client.GetFromJsonAsync<Root>("launches/next");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching next launch: {ex.Message}");
        }
    }

    public async Task GetLatestLaunchAsync()
    {
        try
        {
            LatestLaunch = await _client.GetFromJsonAsync<Root>("launches/latest");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching latest launch: {ex.Message}");
        }
    }

    public async Task GetRoadsterInfoAsync()
    {
        try
        {
            RoadsterInfo = await _client.GetFromJsonAsync<Roadster>("roadster");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching roadster info: {ex.Message}");
        }
    }
}
