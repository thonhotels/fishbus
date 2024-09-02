using System.Text.Json;
using System.Text.Json.Serialization;

namespace Thon.Hotels.FishBus;

public class DefaultJsonOptions
{
    public static JsonSerializerOptions Get { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}