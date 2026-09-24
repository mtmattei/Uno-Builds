using System.Reflection;

namespace FieldCheck.Services;

public interface ISeedSource
{
    string ReadAssetsJson();

    string ReadInspectionsJson();
}

/// <summary>Reads the benchmark fixtures that are embedded (read-only) into the app assembly.</summary>
public sealed class EmbeddedSeedSource : ISeedSource
{
    public string ReadAssetsJson() => Read("FieldCheck.Seed.assets.json");

    public string ReadInspectionsJson() => Read("FieldCheck.Seed.inspections.json");

    private static string Read(string name)
    {
        using var stream = typeof(EmbeddedSeedSource).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Missing embedded seed resource '{name}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
