using System.Text.Json;

namespace PLATEAU.Snap.Models;

public static class Util
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, jsonSerializerOptions);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
    }
}
