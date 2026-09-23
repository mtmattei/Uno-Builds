using System.Text.Json;
using System.Text.Json.Serialization;
using FieldCheck.Models;

namespace FieldCheck.Services;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(List<Asset>))]
[JsonSerializable(typeof(List<Inspection>))]
internal partial class FieldCheckJsonContext : JsonSerializerContext
{
}
