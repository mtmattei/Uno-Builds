using System.Runtime.InteropServices;
using AgentNotifier.Models;
using Microsoft.Extensions.Logging;

namespace AgentNotifier.Services;

public class AudioService : IAudioService
{
    private readonly ILogger<AudioService> _logger;
    private readonly string? _powerUpSoundPath;

    public bool IsEnabled { get; set; } = true;
    public double Volume { get; set; } = 0.5;

    // P/Invoke for playing WAV files on Windows
    [DllImport("winmm.dll", SetLastError = true)]
    private static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);

    private const uint SND_FILENAME = 0x00020000;
    private const uint SND_ASYNC = 0x0001;
    private const uint SND_NODEFAULT = 0x0002;

    public AudioService(ILogger<AudioService> logger)
    {
        _logger = logger;

        // Find the power-up sound file
        var soundPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Sounds", "power-up.wav");
        if (File.Exists(soundPath))
        {
            _powerUpSoundPath = soundPath;
            _logger.LogInformation("Sound file found: {Path}", soundPath);
        }
        else
        {
            _logger.LogWarning("Sound file not found at: {Path}", soundPath);
        }
    }

    public async Task PlayStatusChangeAsync(AgentStatus status)
    {
        if (!IsEnabled) return;

        try
        {
            await Task.Run(() =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Play the power-up sound for "Waiting" status (needs human input)
                    if (status == AgentStatus.Waiting && !string.IsNullOrEmpty(_powerUpSoundPath))
                    {
                        PlaySound(_powerUpSoundPath, IntPtr.Zero, SND_FILENAME | SND_ASYNC | SND_NODEFAULT);
                        _logger.LogDebug("Playing power-up sound for status: {Status}", status);
                    }
                    else
                    {
                        // Fallback to beeps for other statuses
                        var frequency = status switch
                        {
                            AgentStatus.Finished => 600,
                            AgentStatus.Error => 400,
                            _ => 0
                        };

                        var duration = status switch
                        {
                            AgentStatus.Finished => 150,
                            AgentStatus.Error => 300,
                            _ => 0
                        };

                        if (frequency > 0)
                        {
                            Console.Beep(frequency, duration);
                            if (status == AgentStatus.Error)
                            {
                                Console.Beep(frequency - 100, duration);
                            }
                            else if (status == AgentStatus.Finished)
                            {
                                Console.Beep(frequency + 100, 100);
                                Console.Beep(frequency + 200, 150);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Audio notification: {Status}", status);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to play audio notification");
        }
    }
}
