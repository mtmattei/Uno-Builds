using AdTokensIDE.Models;
using Microsoft.UI.Xaml;

namespace AdTokensIDE.Services;

public class AdRotationService : IAdRotationService
{
    private readonly DispatcherTimer _timer;
    private Action<int>? _onAdChanged;
    private int _currentIndex;

    public IReadOnlyList<Advertisement> Ads { get; } = new List<Advertisement>
    {
        new("1", "NullPointer Coffee",
            "The only coffee strong enough to debug your code at 3 AM. Now with 50% more caffeine!",
            "*Results may vary. Coffee cannot fix your code.",
            "\uE7F4", 50),
        new("2", "ErgoCode Chair Pro",
            "Sit for 16 hours straight without regret. Your spine will thank you (eventually).",
            "*Not responsible for actual spine health.",
            "\uE7FC", 75),
        new("3", "BrainStack Supplements",
            "10x your coding speed with our proprietary nootropic blend!",
            "*FDA has not evaluated these claims.",
            "\uE94E", 60),
        new("4", "CloudLess Hosting",
            "Host your apps on our servers. They're just computers we found.",
            "*99.9% uptime not guaranteed.",
            "\uE753", 45),
        new("5", "GitBlame Insurance",
            "When production goes down, we'll find someone else to blame!",
            "*Legal protection not included.",
            "\uE946", 55)
    };

    public AdRotationService()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _timer.Tick += OnTimerTick;
    }

    public void StartRotation(Action<int> onAdChanged)
    {
        _onAdChanged = onAdChanged;
        _currentIndex = 0;
        _onAdChanged?.Invoke(_currentIndex);
        _timer.Start();
    }

    public void StopRotation()
    {
        _timer.Stop();
        _onAdChanged = null;
    }

    private void OnTimerTick(object? sender, object e)
    {
        _currentIndex = (_currentIndex + 1) % Ads.Count;
        _onAdChanged?.Invoke(_currentIndex);
    }
}
