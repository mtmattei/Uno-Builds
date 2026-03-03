using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace HorizontalCalendar.Extensions;

/// <summary>
/// Extension methods for DispatcherQueue.
/// </summary>
public static class DispatcherQueueExtensions
{
    /// <summary>
    /// Enqueues a task to be executed on the dispatcher queue asynchronously.
    /// </summary>
    public static Task EnqueueAsync(this DispatcherQueue dispatcher, Func<Task> function)
    {
        var tcs = new TaskCompletionSource<bool>();
        dispatcher.TryEnqueue(async () =>
        {
            try
            {
                await function();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }
    
    /// <summary>
    /// Enqueues an action to be executed on the dispatcher queue asynchronously.
    /// </summary>
    public static Task EnqueueAsync(this DispatcherQueue dispatcher, Action action)
    {
        var tcs = new TaskCompletionSource<bool>();
        dispatcher.TryEnqueue(() =>
        {
            try
            {
                action();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }
}
