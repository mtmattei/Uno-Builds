using System.Text.Json.Serialization;

namespace FieldCheck.Models;

public enum AssetStatus
{
    Operational,
    Attention,
    Critical,
}

public sealed record Asset(
    string Id,
    string Name,
    string Type,
    string Location,
    AssetStatus Status,
    DateOnly LastInspection,
    string Description)
{
    [JsonIgnore]
    public string StatusText => Status.ToString();

    [JsonIgnore]
    public string IdAndType => $"{Id} · {Type}";

    [JsonIgnore]
    public string IdAndLocation => $"{Id} · {Location}";

    [JsonIgnore]
    public string SpokenText => ToString();

    // Accessible name for list rows.
    public override string ToString() => $"{Name}, {Id}, {Status}";
}
