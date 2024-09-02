using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Thon.Hotels.FishBus.Options;

public class JsonOptions
{
    public List<JsonConverterFactory> Converters { get; set; } = [];
}