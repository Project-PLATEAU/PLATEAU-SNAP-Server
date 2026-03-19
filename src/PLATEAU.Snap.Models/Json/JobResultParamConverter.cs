using PLATEAU.Snap.Models.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PLATEAU.Snap.Models.Json;

public class JobResultParamConverter : JsonConverter<AbstractJobResultParam>
{
    public override AbstractJobResultParam? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;

        if (!root.TryGetProperty("type", out var typeProp))
        {
            throw new JsonException("Missing 'type' property");
        }

        return typeProp.GetString() switch
        {
            "export_building" => JsonSerializer.Deserialize<ExportBuildingResultParam>(root.GetRawText(), options),
            "export_mesh" => JsonSerializer.Deserialize<ExportMeshResultParam>(root.GetRawText(), options),
            _ => throw new JsonException($"Unknown type: {typeProp.GetString()}")
        };
    }

    public override void Write(Utf8JsonWriter writer, AbstractJobResultParam value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case ExportBuildingResultParam exportBuildingResultParam:
                JsonSerializer.Serialize(writer, exportBuildingResultParam, options);
                break;
            case ExportMeshResultParam exportMeshResultParam:
                JsonSerializer.Serialize(writer, exportMeshResultParam, options);
                break;
            default:
                throw new JsonException("Unknown SpeechElement type");
        }
    }
}
