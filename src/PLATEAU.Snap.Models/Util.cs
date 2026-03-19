using System.Text.Json;

namespace PLATEAU.Snap.Models;

public static class Util
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    private static readonly JsonSerializerOptions camelCaseSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, jsonSerializerOptions);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
    }

    public static string SerializeCamelCase<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, camelCaseSerializerOptions);
    }

    public static T? DeserializeCamelCase<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, camelCaseSerializerOptions);
    }
}
