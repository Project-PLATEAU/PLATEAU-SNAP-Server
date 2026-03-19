using System.Text.Json.Serialization;

namespace PLATEAU.Snap.Models.Lambda;

public class LambdaResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    public string Body { get; set; } = null!;
}
