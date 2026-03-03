using Microsoft.UI.Dispatching;

namespace UnoVox.Services;

/// <summary>
/// Provides safe access to UI dispatcher with proper initialization and error handling.
/// Eliminates null reference exceptions and dispatcher queue exhaustion issues.
/// </summary>
public class DispatcherContext
{
    private DispatcherQueue? _dispatcher;
    private readonly ILogger<DispatcherContext> _logger;

    public DispatcherContext(ILogger<DispatcherContext> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets whether the dispatcher has been initialized.
    /// </summary>
    public bool IsInitialized => _dispatcher != null;

    /// <summary>
    /// Initializes the dispatcher from the current thread.
    /// Should be called from the UI thread during application startup.
    /// </summary>
    public void Initialize()
    {
        _dispatcher = DispatcherQueue.GetForCurrentThread();

        if (_dispatcher == null)
        {
            // Fallback: Try to get from main window
            _dispatcher = App.MainWindow?.DispatcherQueue;
        }

        if (_dispatcher == null)
        {
            _logger.LogWarning("Dispatcher not available during initialization - will retry on first use");
        }
        else
        {
            _logger.LogInformation("Dispatcher initialized successfully");
        }
    }

    /// <summary>
    /// Ensures the dispatcher is initialized, attempting lazy initialization if needed.
    /// </summary>
    /// <returns>True if dispatcher is available, false otherwise</returns>
    private bool EnsureDispatcher()
    {
        if (_dispatcher != null)
            return true;

        // Lazy initialization attempt
        _dispatcher = DispatcherQueue.GetForCurrentThread() ?? App.MainWindow?.DispatcherQueue;

        if (_dispatcher == null)
        {
            _logger.LogError("Dispatcher still not available after lazy initialization attempt");
            return false;
        }

        _logger.LogInformation("Dispatcher lazy initialized successfully");
        return true;
    }

    /// <summary>
    /// Executes an action on the UI thread asynchronously with proper error handling.
    /// </summary>
    /// <param name="action">The action to execute on the UI thread</param>
    /// <param name="priority">Priority for dispatcher queue (default: Normal)</param>
    /// <returns>Task that completes when the action has been executed</returns>
    /// <exception cref="InvalidOperationException">Thrown if dispatcher is not available or queue is not accepting work</exception>
    public async Task RunOnUIThreadAsync(Action action, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (!EnsureDispatcher())
        {
            throw new InvalidOperationException("Dispatcher is not available. Ensure Initialize() is called from the UI thread.");
        }

        var tcs = new TaskCompletionSource<bool>();

        var enqueued = _dispatcher!.TryEnqueue(priority, () =>
        {
            try
            {
                action();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing dispatched action");
                tcs.SetException(ex);
            }
        });

        if (!enqueued)
        {
            _logger.LogError("Failed to enqueue work to dispatcher - queue may be full or shutting down");
            throw new InvalidOperationException("Dispatcher queue is not accepting work");
        }

        await tcs.Task;
    }

    /// <summary>
    /// Executes a function on the UI thread asynchronously and returns the result.
    /// </summary>
    /// <typeparam name="T">The return type of the function</typeparam>
    /// <param name="func">The function to execute on the UI thread</param>
    /// <param name="priority">Priority for dispatcher queue (default: Normal)</param>
    /// <returns>Task that completes with the function's result</returns>
    public async Task<T> RunOnUIThreadAsync<T>(Func<T> func, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        if (!EnsureDispatcher())
        {
            throw new InvalidOperationException("Dispatcher is not available. Ensure Initialize() is called from the UI thread.");
        }

        var tcs = new TaskCompletionSource<T>();

        var enqueued = _dispatcher!.TryEnqueue(priority, () =>
        {
            try
            {
                var result = func();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing dispatched function");
                tcs.SetException(ex);
            }
        });

        if (!enqueued)
        {
            _logger.LogError("Failed to enqueue work to dispatcher - queue may be full or shutting down");
            throw new InvalidOperationException("Dispatcher queue is not accepting work");
        }

        return await tcs.Task;
    }

    /// <summary>
    /// Attempts to execute an action on the UI thread without throwing exceptions.
    /// Returns false if the operation could not be queued.
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <param name="priority">Priority for dispatcher queue</param>
    /// <returns>True if the action was queued successfully, false otherwise</returns>
    public bool TryRunOnUIThread(Action action, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
    {
        if (action == null || !EnsureDispatcher())
            return false;

        try
        {
            return _dispatcher!.TryEnqueue(priority, () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in TryRunOnUIThread action");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in TryRunOnUIThread");
            return false;
        }
    }

    /// <summary>
    /// Checks if the current thread is the UI thread.
    /// </summary>
    /// <returns>True if on UI thread, false otherwise</returns>
    public bool IsUIThread()
    {
        if (!EnsureDispatcher())
            return false;

        return _dispatcher!.HasThreadAccess;
    }
}
