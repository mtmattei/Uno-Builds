using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClaudeDash.Models.Search;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

/// <summary>
/// SearchOverlay is a standalone UserControl (not a navigation page),
/// so it uses INotifyPropertyChanged instead of MVUX.
/// </summary>
public class SearchOverlayViewModel : INotifyPropertyChanged
{
    private readonly ISearchIndexService _searchService;
    private readonly INavigationService _navigationService;
    private CancellationTokenSource? _debounceCts;

    private string _queryText = string.Empty;
    private bool _isOpen;
    private bool _isIndexReady;
    private int _indexSize;
    private int _selectedIndex = -1;
    private string _statusText = "Building index...";

    public SearchOverlayViewModel(ISearchIndexService searchService, INavigationService navigationService)
    {
        _searchService = searchService;
        _navigationService = navigationService;
        _searchService.IndexRebuilt += OnIndexRebuilt;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? RequestClose;
    public event Action? RequestFocusInput;

    public string QueryText
    {
        get => _queryText;
        set
        {
            if (_queryText == value) return;
            _queryText = value;
            OnPropertyChanged();
            _ = DebounceSearchAsync(value);
        }
    }

    public bool IsOpen
    {
        get => _isOpen;
        private set { if (_isOpen != value) { _isOpen = value; OnPropertyChanged(); } }
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set { if (_selectedIndex != value) { _selectedIndex = value; OnPropertyChanged(); } }
    }

    public string StatusText
    {
        get => _statusText;
        private set { if (_statusText != value) { _statusText = value; OnPropertyChanged(); } }
    }

    public ObservableCollection<SearchResult> Results { get; } = new();

    private void OnIndexRebuilt()
    {
        _isIndexReady = _searchService.IsReady;
        _indexSize = _searchService.IndexSize;
        StatusText = $"{_indexSize} items indexed";
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        IsOpen = true;
        QueryText = string.Empty;
        Results.Clear();
        SelectedIndex = -1;
        RequestFocusInput?.Invoke();
    }

    public void Close()
    {
        IsOpen = false;
        QueryText = string.Empty;
        Results.Clear();
        SelectedIndex = -1;
        RequestClose?.Invoke();
    }

    private async Task DebounceSearchAsync(string query)
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        try
        {
            await Task.Delay(80, token);
            if (token.IsCancellationRequested) return;
            ExecuteSearch(query);
        }
        catch (TaskCanceledException) { }
    }

    private void ExecuteSearch(string query)
    {
        SelectedIndex = -1;

        if (string.IsNullOrWhiteSpace(query))
        {
            Results.Clear();
            StatusText = $"{_indexSize} items indexed";
            return;
        }

        var results = _searchService.Search(query, 12);
        Results.Clear();
        foreach (var r in results) Results.Add(r);

        StatusText = results.Count > 0
            ? $"{results.Count} result{(results.Count == 1 ? "" : "s")}"
            : "No results";
    }

    public void MoveSelectionUp()
    {
        if (Results.Count == 0) return;
        SelectedIndex = SelectedIndex <= 0 ? Results.Count - 1 : SelectedIndex - 1;
    }

    public void MoveSelectionDown()
    {
        if (Results.Count == 0) return;
        SelectedIndex = SelectedIndex >= Results.Count - 1 ? 0 : SelectedIndex + 1;
    }

    public void ConfirmSelection()
    {
        if (Results.Count == 0) return;
        var idx = SelectedIndex >= 0 && SelectedIndex < Results.Count ? SelectedIndex : 0;
        NavigateToResult(Results[idx]);
    }

    public void NavigateToResult(SearchResult? result)
    {
        if (result == null) return;
        _navigationService.NavigateTo(result.PageKey);
        Close();
    }

    public async Task BuildIndexAsync()
    {
        StatusText = "Building index...";
        await _searchService.BuildIndexAsync();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
