using PLATEAU.Snap.Models.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PLATEAU.Snap.Models.Json;

public class JobParamConverter : JsonConverter<AbstractJobParam>
{
    public override AbstractJobParam? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;

        if (!root.TryGetProperty("type", out var typeProp))
        {
            throw new JsonException("Missing 'type' property");
        }

        return typeProp.GetString() switch
        {
            "export_building" => JsonSerializer.Deserialize<ExportBuildingParam>(root.GetRawText(), options),
            "export_mesh" => JsonSerializer.Deserialize<ExportMeshParam>(root.GetRawText(), options),
            _ => throw new JsonException($"Unknown type: {typeProp.GetString()}")
        };
    }

    public override void Write(Utf8JsonWriter writer, AbstractJobParam value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case ExportBuildingParam exportBuildingParam:
                JsonSerializer.Serialize(writer, exportBuildingParam, options);
                break;
            case ExportMeshParam exportMeshParam:
                JsonSerializer.Serialize(writer, exportMeshParam, options);
                break;
            default:
                throw new JsonException("Unknown SpeechElement type");
        }
    }
}