using System.Text.Json.Serialization;

namespace PLATEAU.Snap.Models.Client;

[JsonConverter(typeof(JsonStringEnumSnakeCaseLowerConverter))]
public enum StatusType
{
    Success,
    Error,
}
