using System.Text.Json;
using System.Text.Json.Serialization;

namespace PLATEAU.Snap.Models;

public class JsonStringEnumSnakeCaseLowerConverter : JsonStringEnumConverter
{
    public JsonStringEnumSnakeCaseLowerConverter() : base(JsonNamingPolicy.SnakeCaseLower)
    {
    }
}
