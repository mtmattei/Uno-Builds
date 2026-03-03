using AdTokensIDE.Models;

namespace AdTokensIDE.Services;

public interface IAdRotationService
{
    IReadOnlyList<Advertisement> Ads { get; }
    void StartRotation(Action<int> onAdChanged);
    void StopRotation();
}
