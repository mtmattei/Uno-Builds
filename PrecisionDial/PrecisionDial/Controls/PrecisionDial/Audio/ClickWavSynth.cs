using System;
using System.IO;
using System.Text;

namespace PrecisionDial.Controls;

/// <summary>
/// Synthesizes a short mechanical click waveform in memory — no asset download
/// required. Produces a 16-bit mono PCM WAV byte array tuned to sound like a
/// luxury rotary selector (crisp high-frequency transient + fast decay + a
/// touch of noise for material texture).
/// </summary>
internal static class ClickWavSynth
{
    public static byte[] GenerateMechanicalClick()
    {
        const int sampleRate = 44100;
        const double durationSec = 0.050; // 50ms total
        var numSamples = (int)(sampleRate * durationSec);

        var samples = new short[numSamples];
        var rng = new Random(42);

        for (int i = 0; i < numSamples; i++)
        {
            var t = (double)i / sampleRate;

            // Fast exponential decay — the "strike" envelope.
            var env = Math.Exp(-t * 55.0);

            // Primary click tone — ~2.3kHz, bright but not harsh.
            var tone = Math.Sin(2 * Math.PI * 2300 * t);

            // Metallic overtone for brightness.
            var overtone = 0.35 * Math.Sin(2 * Math.PI * 4700 * t);

            // Short noise burst at the attack for the "material strike" texture.
            var noiseEnv = Math.Exp(-t * 350.0);
            var noise = (rng.NextDouble() * 2 - 1) * noiseEnv * 0.35;

            // Subtle low thump for body.
            var thumpEnv = Math.Exp(-t * 180.0);
            var thump = 0.20 * Math.Sin(2 * Math.PI * 420 * t) * thumpEnv;

            var sample = (tone + overtone + noise + thump) * env * 0.55;
            samples[i] = (short)Math.Clamp(sample * short.MaxValue, short.MinValue, short.MaxValue);
        }

        return BuildWav(samples, sampleRate);
    }

    private static byte[] BuildWav(short[] samples, int sampleRate)
    {
        const short channels = 1;
        const short bitsPerSample = 16;
        var byteRate = sampleRate * channels * bitsPerSample / 8;
        var dataSize = samples.Length * bitsPerSample / 8;
        var fileSize = 36 + dataSize;

        using var ms = new MemoryStream(fileSize + 8);
        using var w = new BinaryWriter(ms);

        // RIFF header
        w.Write(Encoding.ASCII.GetBytes("RIFF"));
        w.Write(fileSize);
        w.Write(Encoding.ASCII.GetBytes("WAVE"));

        // fmt chunk
        w.Write(Encoding.ASCII.GetBytes("fmt "));
        w.Write(16);                         // chunk size
        w.Write((short)1);                   // format = PCM
        w.Write(channels);
        w.Write(sampleRate);
        w.Write(byteRate);
        w.Write((short)(channels * bitsPerSample / 8)); // block align
        w.Write(bitsPerSample);

        // data chunk
        w.Write(Encoding.ASCII.GetBytes("data"));
        w.Write(dataSize);
        for (int i = 0; i < samples.Length; i++)
            w.Write(samples[i]);

        return ms.ToArray();
    }
}
