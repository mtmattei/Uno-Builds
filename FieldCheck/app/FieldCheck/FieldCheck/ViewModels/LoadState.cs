namespace FieldCheck.ViewModels;

public enum LoadState
{
    Loading,
    Ready,
    Empty,
    Error,
}

/// <summary>Shared loading/empty/error handling for screens backed by the repository.</summary>
public abstract partial class LoadableViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading), nameof(IsReady), nameof(IsEmpty), nameof(IsError))]
    public partial LoadState State { get; protected set; } = LoadState.Loading;

    public bool IsLoading => State == LoadState.Loading;

    public bool IsReady => State == LoadState.Ready;

    public bool IsEmpty => State == LoadState.Empty;

    public bool IsError => State == LoadState.Error;

    public const string ErrorMessage = "FieldCheck couldn't read the local inspection data. Your saved records are not affected.";

    private bool _started;

    /// <summary>Starts the first load once; later refreshes come from repository change notifications.</summary>
    public Task EnsureLoadedAsync()
    {
        if (_started)
        {
            return Task.CompletedTask;
        }

        _started = true;
        return LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        _started = true;
        State = LoadState.Loading;
        try
        {
            var hasData = await LoadCoreAsync();
            State = hasData ? LoadState.Ready : LoadState.Empty;
        }
        catch (Services.RepositoryException)
        {
            State = LoadState.Error;
        }
    }

    /// <summary>Loads data; returns false when the repository has nothing to show.</summary>
    protected abstract Task<bool> LoadCoreAsync();
}
